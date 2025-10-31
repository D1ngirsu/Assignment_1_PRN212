using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}