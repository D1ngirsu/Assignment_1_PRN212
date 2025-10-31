using BusinessObjects.Models;
using FUNewsManagement.Filters;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace FUNewsManagement.Controllers
{
    // Kế thừa BaseController để có sẵn CurrentUser, IsLoggedIn, etc.
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Category - Public, không cần login
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return View(categories);
        }

        // GET: Category/Details/5 - Public
        public async Task<IActionResult> Details(short id)
        {
            if (id <= 0)
                return NotFound();

            var category = await _categoryService.GetCategoryWithNewsAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // GET: Category/Create - Phải login và là Staff hoặc Admin
        [AuthorizeSession]
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin && !HasRole(1))
            {
                TempData["ErrorMessage"] = "You don't have permission to create categories.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ParentCategories = await _categoryService.GetActiveCategoriesAsync();
            return View();
        }

        // POST: Category/Create - Phải login và là Staff hoặc Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Create(Category category)
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
                    await _categoryService.CreateCategoryAsync(category);
                    TempData["SuccessMessage"] = "Category created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            ViewBag.ParentCategories = await _categoryService.GetActiveCategoriesAsync();
            return View(category);
        }

        // GET: Category/Edit/5 - Phải login và là Staff hoặc Admin
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

            ViewBag.ParentCategories = await _categoryService.GetActiveCategoriesAsync();
            return View(category);
        }

        // POST: Category/Edit/5 - Phải login và là Staff hoặc Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Edit(short id, Category category)
        {
            if (id != category.CategoryId)
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
                    await _categoryService.UpdateCategoryAsync(category);
                    TempData["SuccessMessage"] = "Category updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            ViewBag.ParentCategories = await _categoryService.GetActiveCategoriesAsync();
            return View(category);
        }

        // POST: Category/Delete/5 - Staff hoặc Admin, chỉ xóa nếu không có news liên kết
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
