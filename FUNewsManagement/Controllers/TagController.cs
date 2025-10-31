using BusinessObjects.Models;
using FUNewsManagement.Filters;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Linq;

namespace FUNewsManagement.Controllers
{
    public class TagController : BaseController
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        // GET: Tag - Public, không cần login
        public async Task<IActionResult> Index()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return View(tags);
        }

        // GET: Tag/Details/5 - Public
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return NotFound();

            var tagsWithNews = await _tagService.GetTagsWithNewsAsync();
            var tag = tagsWithNews.FirstOrDefault(t => t.TagId == id);
            if (tag == null)
            {
                // Fallback: get tag without news if not found in tags with news
                tag = await _tagService.GetTagByIdAsync(id);
                if (tag == null)
                    return NotFound();
            }

            return View(tag);
        }

        // GET: Tag/Create - Phải login và là Admin
        [AuthorizeSession]
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to create tags.";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // POST: Tag/Create - Phải login và là Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Create(Tag tag)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to create tags.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _tagService.CreateTagAsync(tag);
                    TempData["SuccessMessage"] = "Tag created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(tag);
        }

        // GET: Tag/Edit/5 - Phải login và là Admin
        [AuthorizeSession]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return NotFound();

            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
                return NotFound();

            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit tags.";
                return RedirectToAction(nameof(Index));
            }

            return View(tag);
        }

        // POST: Tag/Edit/5 - Phải login và là Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Edit(int id, Tag tag)
        {
            if (id != tag.TagId)
                return NotFound();

            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit tags.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _tagService.UpdateTagAsync(tag);
                    TempData["SuccessMessage"] = "Tag updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(tag);
        }

        // POST: Tag/Delete/5 - CHỈ Admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to delete tags.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _tagService.DeleteTagAsync(id);
                TempData["SuccessMessage"] = "Tag deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}