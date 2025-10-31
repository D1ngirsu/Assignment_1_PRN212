using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement.Models.ViewModels
{
    public class CategoryFormViewModel
    {
        public short? CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên không được vượt quá 200 ký tự")]
        public string CategoryName { get; set; } = string.Empty;

        public string? CategoryDescription { get; set; }

        public short? ParentCategoryId { get; set; }

        public List<SelectListItem> ParentCategories { get; set; } = new List<SelectListItem>();

        public bool CategoryStatus { get; set; } = true; // Default active
    }
}