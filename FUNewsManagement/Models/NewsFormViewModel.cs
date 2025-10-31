using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement.Models.ViewModels
{
    public class NewsFormViewModel
    {
        public string? NewsArticleId { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(500, ErrorMessage = "Tiêu đề không được vượt quá 500 ký tự")]
        public string NewsTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        public string NewsContent { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public short CategoryId { get; set; }

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        public List<string> SelectedTagIds { get; set; } = new List<string>(); // Multi-select tags

        public List<SelectListItem> Tags { get; set; } = new List<SelectListItem>();

        public bool NewsStatus { get; set; } = true; // Default active

        [DataType(DataType.DateTime)]
        public DateTime? CreatedDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ModifiedDate { get; set; }

        public string? CreatedByName { get; set; } // For display in edit
    }
}