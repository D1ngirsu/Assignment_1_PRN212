// Updated CategoryController.cs
using BusinessObjects.Models;
using FUNewsManagement.Filters;
using FUNewsManagement.Hubs;
using FUNewsManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services;

namespace FUNewsManagement.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<SignalrServer> _hubContext;

        public CategoryController(ICategoryService categoryService,
            IHubContext<SignalrServer> hubContext)
        {
            _categoryService = categoryService;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string? keyword = null)
        {
            // FIXED: Managers see all categories, others see only active ones
            var categories = (IsAdmin || HasRole(1))
                ? await _categoryService.GetAllCategoriesAsync()
                : await _categoryService.GetActiveCategoriesAsync();

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

            TempData["ErrorMessage"] = "Use the modal in Index to create categories.";
            return RedirectToAction(nameof(Index));
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

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"Invalid data for creation: {errors}";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var category = new Category
                {
                    CategoryName = vm.CategoryName,
                    CategoryDesciption = vm.CategoryDesciption,
                    ParentCategoryId = vm.ParentCategoryId,
                    IsActive = vm.CategoryStatus
                };

                await _categoryService.CreateCategoryAsync(category);
                await _hubContext.Clients.All.SendAsync("LoadCategories");
                TempData["SuccessMessage"] = "Category created successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [AuthorizeSession]
        public async Task<IActionResult> GetEditData(short id)
        {
            if (!IsAdmin && !HasRole(1))
                return Json(new { success = false, message = "No permission" });

            // FIXED: Use GetAllCategoriesAsync to include inactive categories in parent list
            var parentCategories = (await _categoryService.GetAllCategoriesAsync())
                .Where(c => c.CategoryId != id)
                .Select(c => new { Value = c.CategoryId.ToString(), Text = c.CategoryName })
                .ToList();

            if (id <= 0)
            {
                return Json(new
                {
                    success = true,
                    categoryId = 0,
                    categoryName = "",
                    categoryDesciption = "",
                    parentCategoryId = (short?)null,
                    categoryStatus = true,
                    parentCategories
                });
            }

            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return Json(new { success = false, message = "Category not found" });

            // FIXED: Return proper boolean value
            return Json(new
            {
                success = true,
                categoryId = category.CategoryId,
                categoryName = category.CategoryName,
                categoryDesciption = category.CategoryDesciption,
                parentCategoryId = category.ParentCategoryId,
                categoryStatus = category.IsActive == true, // Ensure boolean
                parentCategories
            });
        }

        [AuthorizeSession]
        public async Task<IActionResult> Edit(short id)
        {
            if (id <= 0)
                return NotFound();

            if (!IsAdmin && !HasRole(1))
            {
                TempData["ErrorMessage"] = "You don't have permission to edit categories.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Use the modal in Index to edit categories.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Edit(short id, CategoryFormViewModel vm)
        {
            // FIXED: Handle nullable CategoryId properly
            if (vm.CategoryId == null || id != vm.CategoryId.Value)
                return NotFound();

            if (!IsAdmin && !HasRole(1))
            {
                TempData["ErrorMessage"] = "You don't have permission to edit categories.";
                return RedirectToAction(nameof(Index));
            }

            // DEBUG: Log the received values
            System.Diagnostics.Debug.WriteLine($"Edit Category {id}: Name={vm.CategoryName}, Status={vm.CategoryStatus}");

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"Invalid data for update: {errors}";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound();

                System.Diagnostics.Debug.WriteLine($"Before update: IsActive={category.IsActive}");

                category.CategoryName = vm.CategoryName;
                category.CategoryDesciption = vm.CategoryDesciption;
                category.ParentCategoryId = vm.ParentCategoryId;
                category.IsActive = vm.CategoryStatus;

                System.Diagnostics.Debug.WriteLine($"After assignment: IsActive={category.IsActive}");

                await _categoryService.UpdateCategoryAsync(category);
                await _hubContext.Clients.All.SendAsync("LoadCategories");
                TempData["SuccessMessage"] = "Category updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
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
                await _hubContext.Clients.All.SendAsync("LoadCategories");
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