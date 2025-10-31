using BusinessObjects.Models;
using FUNewsManagement.Filters;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace FUNewsManagement.Controllers
{
    // Kế thừa BaseController để có sẵn CurrentUser, IsLoggedIn, etc.
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

        public async Task<IActionResult> Index()
        {
            var news = await _newsService.GetActiveNewsAsync();
            return View(news);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();

            var newsArticle = await _newsService.GetNewsByIdAsync(id);
            if (newsArticle == null || !newsArticle.NewsStatus != true)
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

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Tags = await _tagService.GetAllTagsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Create(NewsArticle newsArticle)
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
                    newsArticle.CreatedById = CurrentUserId;
                    newsArticle.CreatedDate = DateTime.Now;

                    await _newsService.CreateNewsAsync(newsArticle);
                    TempData["SuccessMessage"] = "News article created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Tags = await _tagService.GetAllTagsAsync();
            return View(newsArticle);
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

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Tags = await _tagService.GetAllTagsAsync();
            return View(newsArticle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Edit(string id, NewsArticle newsArticle)
        {
            if (id != newsArticle.NewsArticleId)
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
                    newsArticle.UpdatedById = CurrentUserId;
                    newsArticle.ModifiedDate = DateTime.Now;

                    await _newsService.UpdateNewsAsync(newsArticle);
                    TempData["SuccessMessage"] = "News article updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Tags = await _tagService.GetAllTagsAsync();
            return View(newsArticle);
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
            return View(myNews);
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
            ViewBag.StartDate = startDate.Value.ToShortDateString();
            ViewBag.EndDate = endDate.Value.ToShortDateString();
            return View(newsInPeriod);
        }

        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return RedirectToAction(nameof(Index));

            var news = await _newsService.SearchNewsAsync(keyword);
            ViewBag.Keyword = keyword;
            return View("Index", news);
        }
    }
}
