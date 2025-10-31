// Updated NewsController.cs
using BusinessObjects.Models;
using FUNewsManagement.Filters;
using FUNewsManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Text.Json;

namespace FUNewsManagement.Controllers
{
    public class NewsController : BaseController
    {
        private readonly INewsArticleService _newsService;
        private readonly ICategoryService _categoryService;
        private readonly ITagService _tagService;

        public NewsController(
            INewsArticleService newsService,
            ICategoryService categoryService,
            ITagService tagService)
        {
            _newsService = newsService;
            _categoryService = categoryService;
            _tagService = tagService;
        }

        public async Task<IActionResult> Index(string? keyword = null)
        {
            IEnumerable<NewsArticle> news;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                news = await _newsService.SearchNewsAsync(keyword);
            }
            else
            {
                news = await _newsService.GetActiveNewsAsync();
            }

            // Load all categories into a dictionary for efficient lookup
            var categories = await _categoryService.GetAllCategoriesAsync();
            var catDict = categories.ToDictionary(c => c.CategoryId, c => c);

            // Populate Category navigation property for each news article
            foreach (var article in news)
            {
                if (article.CategoryId.HasValue && catDict.TryGetValue(article.CategoryId.Value, out var cat))
                {
                    article.Category = cat;
                }
            }

            var vm = new NewsListViewModel
            {
                NewsArticles = news.ToList(),
                SearchKeyword = keyword,
                TotalItems = news.Count()
            };

            // Set ViewBag for modal
            ViewBag.Categories = categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
            var tags = await _tagService.GetAllTagsAsync();
            ViewBag.Tags = tags.Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = t.TagId.ToString(), Text = t.TagName }).ToList();

            return View(vm);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();

            var newsArticle = await _newsService.GetNewsByIdAsync(id);
            if (newsArticle == null || newsArticle.NewsStatus != true)
                return NotFound();

            return View(newsArticle);
        }

        [AuthorizeSession]
        public async Task<IActionResult> Create()
        {
            if (!HasRole(1) && !IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to create news articles.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new NewsFormViewModel();
            vm.Categories = (await _categoryService.GetActiveCategoriesAsync())
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
            vm.Tags = (await _tagService.GetAllTagsAsync())
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = t.TagId.ToString(), Text = t.TagName }).ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Create(NewsFormViewModel vm)
        {
            if (!HasRole(1) && !IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to create news articles.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var newsArticle = new NewsArticle
                    {
                        NewsArticleId = Guid.NewGuid().ToString(), // Assuming ID is string GUID
                        NewsTitle = vm.NewsTitle,
                        NewsContent = vm.NewsContent,
                        CategoryId = vm.CategoryId,
                        NewsStatus = vm.NewsStatus,
                        CreatedById = CurrentUserId,
                        CreatedDate = DateTime.Now
                        // Tags will be assigned via service or separate method after creation
                    };

                    await _newsService.CreateNewsAsync(newsArticle);

                    // Assuming a method to assign tags exists in _newsService or _tagService
                    // e.g., await _tagService.AssignTagsToNewsAsync(newsArticle.NewsArticleId, vm.SelectedTagIds.Select(int.Parse).ToList());

                    TempData["SuccessMessage"] = "News article created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            vm.Categories = (await _categoryService.GetActiveCategoriesAsync())
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
            vm.Tags = (await _tagService.GetAllTagsAsync())
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = t.TagId.ToString(), Text = t.TagName }).ToList();

            return View(vm);
        }

        [AuthorizeSession]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();

            var newsArticle = await _newsService.GetNewsByIdAsync(id);
            if (newsArticle == null)
                return NotFound();

            if (!HasRole(1) && !IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this article.";
                return RedirectToAction(nameof(Index));
            }

            // Fix for CS0266 and CS8629 in Edit GET action
            var vm = new NewsFormViewModel
            {
                NewsArticleId = newsArticle.NewsArticleId,
                NewsTitle = newsArticle.NewsTitle,
                NewsContent = newsArticle.NewsContent,
                CategoryId = newsArticle.CategoryId ?? default(short),
                NewsStatus = newsArticle.NewsStatus ?? false,
                CreatedDate = newsArticle.CreatedDate,
                ModifiedDate = newsArticle.ModifiedDate,
                CreatedByName = newsArticle.CreatedBy?.AccountName, // Assuming navigation
                SelectedTagIds = newsArticle.Tags?.Select(t => t.TagId.ToString()).ToList() ?? new List<string>()
            };

            vm.Categories = (await _categoryService.GetActiveCategoriesAsync())
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
            vm.Tags = (await _tagService.GetAllTagsAsync())
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = t.TagId.ToString(), Text = t.TagName }).ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Edit(string id, NewsFormViewModel vm)
        {
            if (id != vm.NewsArticleId)
                return NotFound();

            if (!HasRole(1) && !IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this article.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var existing = await _newsService.GetNewsByIdAsync(id);
                if (existing == null)
                    return NotFound();

                if (ModelState.IsValid)
                {
                    existing.NewsTitle = vm.NewsTitle;
                    existing.NewsContent = vm.NewsContent;
                    existing.CategoryId = vm.CategoryId;
                    existing.NewsStatus = vm.NewsStatus;
                    existing.UpdatedById = CurrentUserId;
                    existing.ModifiedDate = DateTime.Now;

                    await _newsService.UpdateNewsAsync(existing);

                    // Update tags
                    // e.g., await _tagService.UpdateTagsForNewsAsync(existing.NewsArticleId, vm.SelectedTagIds.Select(int.Parse).ToList());

                    TempData["SuccessMessage"] = "News article updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            vm.Categories = (await _categoryService.GetActiveCategoriesAsync())
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
            vm.Tags = (await _tagService.GetAllTagsAsync())
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = t.TagId.ToString(), Text = t.TagName }).ToList();

            return View(vm);
        }

        [HttpGet]
        [AuthorizeSession]
        public async Task<IActionResult> GetEditData(string id)
        {
            try
            {
                if (!HasRole(1) && !IsAdmin)
                {
                    return Json(new { success = false, message = "You don't have permission to edit this article." });
                }

                var newsArticle = await _newsService.GetNewsByIdAsync(id);
                if (newsArticle == null)
                {
                    return Json(new { success = false, message = "News article not found." });
                }

                var selectedTagIds = newsArticle.Tags?.Select(t => t.TagId.ToString()).ToList() ?? new List<string>();

                var result = new
                {
                    success = true,
                    newsArticleId = newsArticle.NewsArticleId,
                    newsTitle = newsArticle.NewsTitle,
                    newsContent = newsArticle.NewsContent,
                    categoryId = newsArticle.CategoryId?.ToString() ?? "",
                    selectedTagIds = selectedTagIds,
                    newsStatus = newsArticle.NewsStatus ?? false
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(1, 3)]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _newsService.DeleteNewsAsync(id);
                TempData["SuccessMessage"] = "News article deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Publish(string id)
        {
            if (!HasRole(1) && !IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to publish news articles.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                await _newsService.PublishNewsAsync(id);
                TempData["SuccessMessage"] = "News article published successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [AuthorizeSession]
        public async Task<IActionResult> MyNews()
        {
            if (!HasRole(1) && !IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to view your news history.";
                return RedirectToAction(nameof(Index));
            }

            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login", "Account");

            var myNews = await _newsService.GetNewsByAuthorAsync(CurrentUserId.Value);
            var vm = new MyNewsViewModel
            {
                NewsArticles = myNews.ToList(),
                UserId = CurrentUserId.Value
            };

            return View(vm);
        }

        [AuthorizeSession]
        public async Task<IActionResult> Report(DateTime? startDate, DateTime? endDate)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to access reports.";
                return RedirectToAction(nameof(Index));
            }

            if (!startDate.HasValue || !endDate.HasValue)
            {
                startDate = DateTime.Now.AddMonths(-1).Date;
                endDate = DateTime.Now.Date;
            }

            var newsInPeriod = await _newsService.GetNewsByDateRangeAsync(startDate.Value, endDate.Value);
            var vm = new NewsReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                NewsInPeriod = newsInPeriod.OrderByDescending(n => n.CreatedDate).ToList(),
                TotalNews = newsInPeriod.Count()
            };

            return View(vm);
        }

        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return RedirectToAction(nameof(Index));

            var news = await _newsService.SearchNewsAsync(keyword);

            // Load all categories into a dictionary for efficient lookup
            var categories = await _categoryService.GetAllCategoriesAsync();
            var catDict = categories.ToDictionary(c => c.CategoryId, c => c);

            // Populate Category navigation property for each news article
            foreach (var article in news)
            {
                if (article.CategoryId.HasValue && catDict.TryGetValue(article.CategoryId.Value, out var cat))
                {
                    article.Category = cat;
                }
            }

            var vm = new NewsListViewModel
            {
                NewsArticles = news.ToList(),
                SearchKeyword = keyword,
                TotalItems = news.Count()
            };

            // Set ViewBag for modal (consistent with Index)
            ViewBag.Categories = categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
            var tags = await _tagService.GetAllTagsAsync();
            ViewBag.Tags = tags.Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = t.TagId.ToString(), Text = t.TagName }).ToList();

            return View("Index", vm);
        }
    }
}