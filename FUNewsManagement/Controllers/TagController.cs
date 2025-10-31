// Updated TagController.cs
using BusinessObjects.Models;
using FUNewsManagement.Filters;
using FUNewsManagement.Models.ViewModels;
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

        public async Task<IActionResult> Index(string? keyword = null)
        {
            var tags = await _tagService.GetAllTagsAsync();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                tags = tags.Where(t => t.TagName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            var vm = new TagListViewModel
            {
                Tags = tags.ToList(),
                SearchKeyword = keyword
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return NotFound();

            var tagsWithNews = await _tagService.GetTagsWithNewsAsync();
            var tag = tagsWithNews.FirstOrDefault(t => t.TagId == id);
            if (tag == null)
            {
                tag = await _tagService.GetTagByIdAsync(id);
                if (tag == null)
                    return NotFound();
            }

            return View(tag);
        }

        [AuthorizeSession]
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to create tags.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new TagFormViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Create(TagFormViewModel vm)
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
                    var tag = new Tag
                    {
                        TagName = vm.TagName,
                        Note = vm.TagDescription
                    };

                    await _tagService.CreateTagAsync(tag);
                    TempData["SuccessMessage"] = "Tag created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(vm);
        }

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

            var vm = new TagFormViewModel
            {
                TagId = tag.TagId,
                TagName = tag.TagName,
                TagDescription = tag.Note
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> Edit(int id, TagFormViewModel vm)
        {
            if (id != vm.TagId)
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
                    var tag = await _tagService.GetTagByIdAsync(id);
                    if (tag == null)
                        return NotFound();

                    tag.TagName = vm.TagName;
                    tag.Note = vm.TagDescription;

                    await _tagService.UpdateTagAsync(tag);
                    TempData["SuccessMessage"] = "Tag updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(vm);
        }

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