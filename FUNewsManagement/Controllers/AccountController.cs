// Updated AccountController.cs
using BusinessObjects.Models;
using FUNewsManagement.Filters;
using FUNewsManagement.Helpers;
using FUNewsManagement.Models.ViewModels;
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

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (IsLoggedIn)
                return RedirectToAction("Index", "Home");

            var vm = new LoginViewModel { ReturnUrl = returnUrl };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Email) || string.IsNullOrWhiteSpace(vm.Password))
            {
                ModelState.AddModelError("", "Email và mật khẩu không được để trống.");
                return View(vm);
            }

            var account = await _authService.AuthenticateAsync(vm.Email, vm.Password);
            if (account == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(vm);
            }

            HttpContext.Session.SetCurrentUser(account);

            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectToAction("Index", "News");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.ClearSession();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (await _accountService.EmailExistsAsync(vm.AccountEmail))
            {
                ModelState.AddModelError("", "Email đã được sử dụng.");
                return View(vm);
            }

            var newAccount = new SystemAccount
            {
                AccountName = vm.AccountName,
                AccountEmail = vm.AccountEmail,
                AccountPassword = vm.AccountPassword,  // Plain! Let service hash
                AccountRole = vm.AccountRole ?? 2
            };

            await _accountService.CreateAccountAsync(newAccount);  // Service will hash

            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AuthorizeSession]
        public async Task<IActionResult> Profile()
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login");

            var user = await _accountService.GetAccountByIdAsync(CurrentUserId.Value);
            if (user == null)
                return RedirectToAction("Login");

            var vm = new ProfileViewModel
            {
                AccountId = user.AccountId,
                AccountName = user.AccountName,
                AccountEmail = user.AccountEmail
                // No password
            };

            return View(vm);
        }

        [HttpPost]
        [AuthorizeSession]
        public async Task<IActionResult> Profile(ProfileViewModel vm)
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login");

            var existing = await _accountService.GetAccountByIdAsync(CurrentUserId.Value);
            if (existing == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(vm);

            existing.AccountName = vm.AccountName;
            existing.AccountEmail = vm.AccountEmail;

            await _accountService.UpdateAccountAsync(existing);
            HttpContext.Session.SetCurrentUser(existing);

            TempData["SuccessMessage"] = "Cập nhật thông tin thành công.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [AuthorizeSession]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        [AuthorizeSession]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login");

            var account = await _accountService.GetAccountByIdAsync(CurrentUserId.Value);
            if (account == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(vm);

            if (string.IsNullOrWhiteSpace(vm.OldPassword) || string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ mật khẩu.");
                return View(vm);
            }

            if (!PasswordHelper.VerifyPassword(vm.OldPassword, account.AccountPassword))
            {
                ModelState.AddModelError("", "Mật khẩu cũ không chính xác.");
                return View(vm);
            }

            if (vm.NewPassword != vm.ConfirmPassword)
            {
                ModelState.AddModelError("", "Xác nhận mật khẩu mới không trùng khớp.");
                return View(vm);
            }

            account.AccountPassword = PasswordHelper.HashPassword(vm.NewPassword);
            await _accountService.UpdateAccountAsync(account);

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(ChangePassword));
        }

        [HttpGet]
        [AuthorizeSession]
        public async Task<IActionResult> ManageAccounts(string? keyword = null)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to manage accounts.";
                return RedirectToAction("AccessDenied");
            }

            var accounts = await _accountService.GetAllAccountsAsync();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                accounts = accounts.Where(a => a.AccountName.Contains(keyword) || a.AccountEmail.Contains(keyword));
            }

            var vm = new AccountListViewModel
            {
                Accounts = accounts.ToList(),
                SearchKeyword = keyword
            };

            return View(vm);
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

            var vm = new AccountFormViewModel
            {
                AccountId = account.AccountId,
                AccountName = account.AccountName,
                AccountEmail = account.AccountEmail,
                AccountRole = account.AccountRole
                // No password for edit
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> EditUser(short id, AccountFormViewModel vm)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit user accounts.";
                return RedirectToAction("AccessDenied");
            }

            if (id != vm.AccountId)
                return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    var account = await _accountService.GetAccountByIdAsync(id);
                    if (account == null)
                        return NotFound();

                    account.AccountName = vm.AccountName;
                    account.AccountEmail = vm.AccountEmail;
                    account.AccountRole = vm.AccountRole;

                    await _accountService.UpdateAccountAsync(account);
                    TempData["SuccessMessage"] = "User account updated successfully!";
                    return RedirectToAction(nameof(ManageAccounts));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}