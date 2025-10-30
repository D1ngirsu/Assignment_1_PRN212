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

        // GET: News - Public, không cần login
        public async Task<IActionResult> Index()
        {
            var news = await _newsService.GetActiveNewsAsync();
            return View(news);
        }

        // GET: News/Details/5 - Public
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();

            var newsArticle = await _newsService.GetNewsByIdAsync(id);
            if (newsArticle == null)
                return NotFound();

            return View(newsArticle);
        }

        // GET: News/Create - Phải login
        [AuthorizeSession]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Tags = await _tagService.GetAllTagsAsync();
            return View();
        }

        // POST: News/Create - Phải login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Create(NewsArticle newsArticle)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Tự động set CreatedById từ session
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

        // GET: News/Edit/5 - Phải login VÀ là chính người tạo hoặc Admin
        [AuthorizeSession]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();

            var newsArticle = await _newsService.GetNewsByIdAsync(id);
            if (newsArticle == null)
                return NotFound();

            // Kiểm tra quyền: chỉ người tạo hoặc Admin mới được edit
            if (newsArticle.CreatedById != CurrentUserId && !IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this article.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Tags = await _tagService.GetAllTagsAsync();
            return View(newsArticle);
        }

        // POST: News/Edit/5 - Phải login VÀ có quyền
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Edit(string id, NewsArticle newsArticle)
        {
            if (id != newsArticle.NewsArticleId)
                return NotFound();

            try
            {
                var existing = await _newsService.GetNewsByIdAsync(id);
                if (existing == null)
                    return NotFound();

                // Kiểm tra quyền
                if (existing.CreatedById != CurrentUserId && !IsAdmin)
                {
                    TempData["ErrorMessage"] = "You don't have permission to edit this article.";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    // Set UpdatedById
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

        // POST: News/Delete/5 - CHỈ Admin hoặc Staff
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(1, 2)] // Admin (1) hoặc Staff (2)
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

        // POST: News/Publish/5 - CHỈ Admin hoặc Staff
        [HttpPost]
        [ValidateAntiForgeryToken]
        [StaffOnly] // Sử dụng custom attribute
        public async Task<IActionResult> Publish(string id)
        {
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

        // GET: News/MyNews - Tin tức của user hiện tại
        [AuthorizeSession]
        public async Task<IActionResult> MyNews()
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login", "Account");

            var myNews = await _newsService.GetNewsByAuthorAsync(CurrentUserId.Value);
            return View(myNews);
        }

        // Search - Public
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