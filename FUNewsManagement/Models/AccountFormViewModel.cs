using BusinessObjects.Models;
using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement.Models.ViewModels
{
    public class AccountFormViewModel
    {
        public short? AccountId { get; set; }

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        [Display(Name = "Tên")]
        public string AccountName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [Display(Name = "Email")]
        public string AccountEmail { get; set; } = string.Empty;

        // Password is required for Create but not for Edit
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có từ 6 đến 100 ký tự")]
        [Display(Name = "Mật khẩu")]
        public string? AccountPassword { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [Display(Name = "Vai trò")]
        public int? AccountRole { get; set; } = 2; // Default Lecturer

        // Helper property to determine if this is a create or edit operation
        public bool IsEditMode => AccountId.HasValue && AccountId.Value > 0;
    }
}