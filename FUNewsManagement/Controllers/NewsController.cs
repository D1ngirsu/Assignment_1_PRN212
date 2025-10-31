// Updated NewsController.cs (adjusted to use List<string> for SelectedTagIds, and integrate validation messages)
using BusinessObjects.Models;
using FUNewsManagement.Filters;
using FUNewsManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index(string? keyword = null, int page = 1)
        {
            IEnumerable<NewsArticle> news;
            bool isAdminOrStaff = HasRole(1) || IsAdmin;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                news = await _newsService.SearchNewsAsync(keyword);
            }
            else
            {
                if (isAdminOrStaff)
                {
                    news = await _newsService.GetAllNewsAsync();
                }
                else
                {
                    news = await _newsService.GetActiveNewsAsync();
                }
            }

            if (!isAdminOrStaff)
            {
                news = news.Where(n => n.NewsStatus == true);
            }

            int pageSize = 10;
            var totalItems = news.Count();
            var pagedNews = news.OrderByDescending(n => n.CreatedDate)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var categories = await _categoryService.GetAllCategoriesAsync();
            var catDict = categories.ToDictionary(c => c.CategoryId, c => c);

            foreach (var article in pagedNews)
            {
                if (article.CategoryId.HasValue && catDict.TryGetValue(article.CategoryId.Value, out var cat))
                {
                    article.Category = cat;
                }
            }

            var vm = new NewsListViewModel
            {
                NewsArticles = pagedNews,
                SearchKeyword = keyword,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems
            };

            ViewBag.Categories = categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
            var tags = await _tagService.GetAllTagsAsync();
            ViewBag.Tags = tags.Select(t => new SelectListItem { Value = t.TagId.ToString(), Text = t.TagName }).ToList();

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

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
                    .ToList();
                TempData["ErrorMessage"] = "Validation errors: " + string.Join("; ", errorMessages);
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var nextId = await _newsService.GetNextNewsIdAsync();
                var newsId = nextId.ToString();

                var newsArticle = new NewsArticle
                {
                    NewsArticleId = newsId,
                    NewsTitle = vm.NewsTitle,
                    Headline = vm.Headline,
                    NewsContent = vm.NewsContent,
                    CategoryId = vm.CategoryId,
                    NewsStatus = vm.NewsStatus,
                    CreatedById = CurrentUserId,
                    CreatedDate = DateTime.Now
                };

                await _newsService.CreateNewsAsync(newsArticle);

                // Handle tags: Add to NewsTag junction table
                if (vm.SelectedTagIds != null && vm.SelectedTagIds.Any())
                {
                    var tagIds = vm.SelectedTagIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToList();
                    if (tagIds.Any())
                    {
                        await _newsService.AddTagsToNewsAsync(newsId, tagIds);
                    }
                }

                TempData["SuccessMessage"] = $"News article created successfully with ID: {newsId}!";
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
        public async Task<IActionResult> Edit(NewsFormViewModel vm)
        {
            if (!HasRole(1) && !IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this article.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
                    .ToList();
                TempData["ErrorMessage"] = "Validation errors: " + string.Join("; ", errorMessages);
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var existing = await _newsService.GetNewsByIdAsync(vm.NewsArticleId);
                if (existing == null)
                {
                    TempData["ErrorMessage"] = "News article not found.";
                    return RedirectToAction(nameof(Index));
                }

                existing.NewsTitle = vm.NewsTitle;
                existing.Headline = vm.Headline;
                existing.NewsContent = vm.NewsContent;
                existing.CategoryId = vm.CategoryId;
                existing.NewsStatus = vm.NewsStatus;
                existing.UpdatedById = CurrentUserId;
                existing.ModifiedDate = DateTime.Now;

                await _newsService.UpdateNewsAsync(existing);

                // Handle tags: Remove old and add new to NewsTag junction table
                await _newsService.RemoveAllTagsFromNewsAsync(vm.NewsArticleId);
                if (vm.SelectedTagIds != null && vm.SelectedTagIds.Any())
                {
                    var tagIds = vm.SelectedTagIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToList();
                    if (tagIds.Any())
                    {
                        await _newsService.AddTagsToNewsAsync(vm.NewsArticleId, tagIds);
                    }
                }

                TempData["SuccessMessage"] = "News article updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
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
                    headline = newsArticle.Headline ?? newsArticle.NewsTitle,
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
                // Remove tags before deleting news (cascade or explicit)
                await _newsService.RemoveAllTagsFromNewsAsync(id);
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
                return RedirectToAction(nameof(Index));
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

            return RedirectToAction(nameof(Index));
        }

        [AuthorizeSession]
        public async Task<IActionResult> MyNews(string? keyword = null, int page = 1)
        {
            if (!HasRole(1) && !IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to view your news history.";
                return RedirectToAction(nameof(Index));
            }

            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login", "Account");

            var myNews = await _newsService.GetNewsByAuthorAsync(CurrentUserId.Value);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                myNews = myNews.Where(n => n.NewsTitle.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            int pageSize = 10;
            var totalItems = myNews.Count();
            var pagedNews = myNews.OrderByDescending(n => n.CreatedDate)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var categories = await _categoryService.GetAllCategoriesAsync();
            var catDict = categories.ToDictionary(c => c.CategoryId, c => c);
            foreach (var article in pagedNews)
            {
                if (article.CategoryId.HasValue && catDict.TryGetValue(article.CategoryId.Value, out var cat))
                {
                    article.Category = cat;
                }
            }

            var vm = new MyNewsViewModel
            {
                NewsArticles = pagedNews,
                SearchKeyword = keyword,
                UserId = CurrentUserId.Value,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems
            };

            ViewBag.Categories = categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
            var tags = await _tagService.GetAllTagsAsync();
            ViewBag.Tags = tags.Select(t => new SelectListItem { Value = t.TagId.ToString(), Text = t.TagName }).ToList();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Report(NewsReportViewModel model)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to access reports.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra ModelState để xử lý lỗi binding (ví dụ: ngày không hợp lệ hoặc rỗng)
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
                    .ToList();
                TempData["ErrorMessage"] = "Validation errors: " + string.Join("; ", errorMessages);
                // Fallback to defaults
                model.StartDate = DateTime.Now.AddMonths(-1).Date;
                model.EndDate = DateTime.Now.Date;
                model.NewsInPeriod = new List<NewsArticle>();
                model.TotalNews = 0;
                return View(model);
            }

            // Validate dates
            if (model.StartDate > model.EndDate)
            {
                TempData["ErrorMessage"] = "Start date must be before or equal to end date.";
                // Fallback to defaults
                model.StartDate = DateTime.Now.AddMonths(-1).Date;
                model.EndDate = DateTime.Now.Date;
            }

            try
            {
                var newsInPeriod = await _newsService.GetNewsByDateRangeAsync(model.StartDate, model.EndDate);
                model.NewsInPeriod = newsInPeriod
                    .OrderByDescending(n => model.SortBy == "CreatedDate" ? n.CreatedDate : n.ModifiedDate ?? n.CreatedDate)  // Dynamic sort nếu cần
                    .ToList();
                model.TotalNews = newsInPeriod.Count();
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                // Reload with defaults
                model.StartDate = DateTime.Now.AddMonths(-1).Date;
                model.EndDate = DateTime.Now.Date;
                model.NewsInPeriod = new List<NewsArticle>();
                model.TotalNews = 0;
            }

            return View(model);
        }

        public async Task<IActionResult> Search(string keyword, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return RedirectToAction(nameof(Index));

            var news = await _newsService.SearchNewsAsync(keyword);

            int pageSize = 10;
            var totalItems = news.Count();
            var pagedNews = news.OrderByDescending(n => n.CreatedDate)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var categories = await _categoryService.GetAllCategoriesAsync();
            var catDict = categories.ToDictionary(c => c.CategoryId, c => c);

            foreach (var article in pagedNews)
            {
                if (article.CategoryId.HasValue && catDict.TryGetValue(article.CategoryId.Value, out var cat))
                {
                    article.Category = cat;
                }
            }

            var vm = new NewsListViewModel
            {
                NewsArticles = pagedNews,
                SearchKeyword = keyword,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems
            };

            ViewBag.Categories = categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
            var tags = await _tagService.GetAllTagsAsync();
            ViewBag.Tags = tags.Select(t => new SelectListItem { Value = t.TagId.ToString(), Text = t.TagName }).ToList();

            return View("Index", vm);
        }
    }
}