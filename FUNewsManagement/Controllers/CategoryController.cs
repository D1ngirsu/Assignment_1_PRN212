// Updated CategoryController.cs
using BusinessObjects.Models;
using FUNewsManagement.Filters;
using FUNewsManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace FUNewsManagement.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? keyword = null)
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                categories = categories.Where(c => c.CategoryName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            var vm = new CategoryListViewModel
            {
                Categories = categories.ToList(),
                SearchKeyword = keyword
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(short id)
        {
            if (id <= 0)
                return NotFound();

            var category = await _categoryService.GetCategoryWithNewsAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [AuthorizeSession]
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin && !HasRole(1))
            {
                TempData["ErrorMessage"] = "You don't have permission to create categories.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new CategoryFormViewModel();
            vm.ParentCategories = (await _categoryService.GetActiveCategoriesAsync())
                .Where(c => c.CategoryId != vm.ParentCategoryId) // Avoid self-reference
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Create(CategoryFormViewModel vm)
        {
            if (!IsAdmin && !HasRole(1))
            {
                TempData["ErrorMessage"] = "You don't have permission to create categories.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var category = new Category
                    {
                        CategoryName = vm.CategoryName,
                        CategoryDesciption = vm.CategoryDescription,
                        ParentCategoryId = vm.ParentCategoryId,
                        IsActive = vm.CategoryStatus
                    };

                    await _categoryService.CreateCategoryAsync(category);
                    TempData["SuccessMessage"] = "Category created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            vm.ParentCategories = (await _categoryService.GetActiveCategoriesAsync())
                .Where(c => c.CategoryId != vm.ParentCategoryId)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();

            return View(vm);
        }

        [AuthorizeSession]
        public async Task<IActionResult> Edit(short id)
        {
            if (id <= 0)
                return NotFound();

            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            if (!IsAdmin && !HasRole(1))
            {
                TempData["ErrorMessage"] = "You don't have permission to edit categories.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new CategoryFormViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDesciption,
                ParentCategoryId = category.ParentCategoryId,
                CategoryStatus = category.IsActive ?? true
            };

            vm.ParentCategories = (await _categoryService.GetActiveCategoriesAsync())
                .Where(c => c.CategoryId != id)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Edit(short id, CategoryFormViewModel vm)
        {
            if (id != vm.CategoryId)
                return NotFound();

            if (!IsAdmin && !HasRole(1))
            {
                TempData["ErrorMessage"] = "You don't have permission to edit categories.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(id);
                    if (category == null)
                        return NotFound();

                    category.CategoryName = vm.CategoryName;
                    category.CategoryDesciption = vm.CategoryDescription;
                    category.ParentCategoryId = vm.ParentCategoryId;
                    category.IsActive = vm.CategoryStatus;

                    await _categoryService.UpdateCategoryAsync(category);
                    TempData["SuccessMessage"] = "Category updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            vm.ParentCategories = (await _categoryService.GetActiveCategoriesAsync())
                .Where(c => c.CategoryId != id)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            if (!IsAdmin && !HasRole(1))
            {
                TempData["ErrorMessage"] = "You don't have permission to delete categories.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var category = await _categoryService.GetCategoryWithNewsAsync(id);
                if (category?.NewsArticles?.Any() == true)
                {
                    TempData["ErrorMessage"] = "Cannot delete category because it is associated with news articles.";
                    return RedirectToAction(nameof(Index));
                }

                await _categoryService.DeleteCategoryAsync(id);
                TempData["SuccessMessage"] = "Category deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}