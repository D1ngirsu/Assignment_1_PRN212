// Updated TagFormViewModel.cs
using BusinessObjects.Models;
using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement.Models.ViewModels
{
    public class TagFormViewModel
    {
        public int? TagId { get; set; }

        [Required(ErrorMessage = "Tên tag là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên tag không được vượt quá 100 ký tự")]
        public string TagName { get; set; } = string.Empty;

        public string? Note { get; set; }
    }
}