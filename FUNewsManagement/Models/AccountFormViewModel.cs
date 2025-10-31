using BusinessObjects.Models;
using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement.Models.ViewModels
{
    public class AccountFormViewModel
    {
        public short? AccountId { get; set; }

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(100)]
        public string AccountName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress]
        public string AccountEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc (cho create)")]
        [StringLength(100, MinimumLength = 6)]
        public string AccountPassword { get; set; } = string.Empty;

        public int? AccountRole { get; set; } = 2; // Default Lecturer
    }
}