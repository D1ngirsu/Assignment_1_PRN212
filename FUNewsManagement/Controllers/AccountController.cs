using BusinessObjects.Models;
using FUNewsManagement.Filters;
using FUNewsManagement.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace FUNewsManagement.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAuthenticationService _authService;
        private readonly ISystemAccountService _accountService;

        public AccountController(IAuthenticationService authService, ISystemAccountService accountService)
        {
            _authService = authService;
            _accountService = accountService;
        }

        // LOGIN
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (IsLoggedIn)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email và mật khẩu không được để trống.";
                return View();
            }

            var account = await _authService.AuthenticateAsync(email, password);
            if (account == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            HttpContext.Session.SetCurrentUser(account);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return account.AccountRole switch
            {
                3 => RedirectToAction("Index", "Admin"),
                1 => RedirectToAction("Index", "Staff"),
                2 => RedirectToAction("Index", "Lecturer"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public IActionResult Logout()
        {
            HttpContext.Session.ClearSession();
            return RedirectToAction("Login");
        }

        // REGISTER
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(SystemAccount newAccount)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Dữ liệu không hợp lệ.";
                return View(newAccount);
            }

            if (await _accountService.EmailExistsAsync(newAccount.AccountEmail))
            {
                ViewBag.Error = "Email đã được sử dụng.";
                return View(newAccount);
            }

            newAccount.AccountPassword = PasswordHelper.HashPassword(newAccount.AccountPassword);
            newAccount.AccountRole ??= 2;
            await _accountService.CreateAccountAsync(newAccount);

            ViewBag.Success = "Đăng ký tài khoản thành công!";
            return RedirectToAction("Login");
        }

        // PROFILE
        [HttpGet]
        [AuthorizeSession]
        public async Task<IActionResult> Profile()
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login");

            var user = await _accountService.GetAccountByIdAsync(CurrentUserId.Value);
            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        [HttpPost]
        [AuthorizeSession]
        public async Task<IActionResult> Profile(SystemAccount updatedAccount)
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login");

            var existing = await _accountService.GetAccountByIdAsync(CurrentUserId.Value);
            if (existing == null)
                return RedirectToAction("Login");

            existing.AccountName = updatedAccount.AccountName;
            existing.AccountEmail = updatedAccount.AccountEmail;

            await _accountService.UpdateAccountAsync(existing);
            HttpContext.Session.SetCurrentUser(existing);

            ViewBag.Success = "Cập nhật thông tin thành công.";
            return View(existing);
        }

        // CHANGE PASSWORD
        [HttpGet]
        [AuthorizeSession]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        [AuthorizeSession]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login");

            var account = await _accountService.GetAccountByIdAsync(CurrentUserId.Value);
            if (account == null)
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ mật khẩu.";
                return View();
            }

            if (!PasswordHelper.VerifyPassword(oldPassword, account.AccountPassword))
            {
                ViewBag.Error = "Mật khẩu cũ không chính xác.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Xác nhận mật khẩu mới không trùng khớp.";
                return View();
            }

            account.AccountPassword = PasswordHelper.HashPassword(newPassword);
            await _accountService.UpdateAccountAsync(account);

            ViewBag.Success = "Đổi mật khẩu thành công.";
            return View();
        }

        // ADMIN MANAGE ACCOUNTS
        [HttpGet]
        [AuthorizeSession]
        public async Task<IActionResult> ManageAccounts()
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to manage accounts.";
                return RedirectToAction("AccessDenied");
            }

            var accounts = await _accountService.GetAllAccountsAsync();
            return View(accounts);
        }

        [HttpGet]
        [AuthorizeSession]
        public async Task<IActionResult> EditUser(short id)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit user accounts.";
                return RedirectToAction("AccessDenied");
            }

            if (id <= 0)
                return NotFound();

            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound();

            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> EditUser(short id, SystemAccount account)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit user accounts.";
                return RedirectToAction("AccessDenied");
            }

            if (id != account.AccountId)
                return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    await _accountService.UpdateAccountAsync(account);
                    TempData["SuccessMessage"] = "User account updated successfully!";
                    return RedirectToAction(nameof(ManageAccounts));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(account);
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}
