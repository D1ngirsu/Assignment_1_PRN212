using BusinessObjects.Models;
using FUNewsManagement.Filters;
using FUNewsManagement.Helpers;
using FUNewsManagement.Hubs;
using FUNewsManagement.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services;

namespace FUNewsManagement.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAuthenticationService _authService;
        private readonly ISystemAccountService _accountService;
        private readonly IHubContext<SignalrServer> _hubContext;

        public AccountController(IAuthenticationService authService, ISystemAccountService accountService, IHubContext<SignalrServer> hubContext)  // Thêm tham số
        {
            _authService = authService;
            _accountService = accountService;
            _hubContext = hubContext;  // Gán
        }

        #region Authentication

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
                AccountPassword = vm.AccountPassword,
                AccountRole = vm.AccountRole ?? 2
            };

            await _accountService.CreateAccountAsync(newAccount);

            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
            return RedirectToAction("Login");
        }

        #endregion

        #region Profile Management

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

        #endregion

        #region Account Management (Admin Only)

        [HttpGet]
        [AuthorizeSession]
        public async Task<IActionResult> ManageAccounts(string? keyword = null)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền quản lý tài khoản.";
                return RedirectToAction("AccessDenied");
            }

            var accounts = await _accountService.GetAllAccountsAsync();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                accounts = accounts.Where(a =>
                    a.AccountName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    a.AccountEmail.Contains(keyword, StringComparison.OrdinalIgnoreCase));
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
        public IActionResult CreateUser()
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền tạo tài khoản.";
                return RedirectToAction("AccessDenied");
            }

            var vm = new AccountFormViewModel
            {
                AccountRole = 2 // Default Lecturer
            };

            return PartialView("CreateUser", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> CreateUser(AccountFormViewModel vm)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền tạo tài khoản.";
                return RedirectToAction("AccessDenied");
            }

            if (!ModelState.IsValid)
            {
                return PartialView("CreateUser", vm);
            }

            try
            {
                // Check if email already exists
                if (await _accountService.EmailExistsAsync(vm.AccountEmail))
                {
                    ModelState.AddModelError("AccountEmail", "Email đã được sử dụng.");
                    return PartialView("CreateUser", vm);
                }

                var newAccount = new SystemAccount
                {
                    AccountName = vm.AccountName,
                    AccountEmail = vm.AccountEmail,
                    AccountPassword = PasswordHelper.HashPassword(vm.AccountPassword),
                    AccountRole = vm.AccountRole ?? 2
                };

                await _accountService.CreateAccountAsync(newAccount);
                await _hubContext.Clients.All.SendAsync("LoadAccounts");
                TempData["SuccessMessage"] = "Tạo tài khoản thành công!";
                return RedirectToAction(nameof(ManageAccounts));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi tạo tài khoản: {ex.Message}");
                return PartialView("CreateUser", vm);
            }
        }

        [HttpGet]
        [AuthorizeSession]
        public async Task<IActionResult> EditUser(short id)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa tài khoản.";
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
            };

            return PartialView("EditUser", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> EditUser(short id, AccountFormViewModel vm)
        {
            if (!IsAdmin)
            {
                return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa tài khoản." });
            }

            if (id != vm.AccountId)
                return Json(new { success = false, message = "ID không hợp lệ." });

            // Remove password validation for edit
            ModelState.Remove("AccountPassword");

            if (!ModelState.IsValid)
            {
                // Trả JSON với validation errors
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                    );
                return Json(new { success = false, message = "Validation lỗi", errors = errors });
            }

            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                    return Json(new { success = false, message = "Không tìm thấy tài khoản." });

                // Check email duplicate
                if (account.AccountEmail != vm.AccountEmail &&
                    await _accountService.EmailExistsAsync(vm.AccountEmail))
                {
                    ModelState.AddModelError("AccountEmail", "Email đã được sử dụng.");
                    return Json(new { success = false, message = "Email đã được sử dụng.", field = "AccountEmail" });
                }

                account.AccountName = vm.AccountName;
                account.AccountEmail = vm.AccountEmail;
                account.AccountRole = vm.AccountRole;

                await _accountService.UpdateAccountAsync(account);
                await _hubContext.Clients.All.SendAsync("LoadAccounts");
                // Update session if editing own account
                if (CurrentUserId == id)
                {
                    HttpContext.Session.SetCurrentUser(account);
                }

                return Json(new { success = true, message = "Cập nhật tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                // Log exception (nếu có logger, dùng ILogger)
                Console.WriteLine($"EditUser error: {ex.Message}"); // Tạm thời
                return Json(new { success = false, message = $"Lỗi khi cập nhật: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession]
        public async Task<IActionResult> DeleteUser(short id)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa tài khoản.";
                return RedirectToAction("AccessDenied");
            }

            try
            {
                // Prevent deleting own account
                if (CurrentUserId == id)
                {
                    TempData["ErrorMessage"] = "Bạn không thể xóa tài khoản của chính mình!";
                    return RedirectToAction(nameof(ManageAccounts));
                }

                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài khoản.";
                    return RedirectToAction(nameof(ManageAccounts));
                }

                await _accountService.DeleteAccountAsync(id);
                await _hubContext.Clients.All.SendAsync("LoadAccounts");
                TempData["SuccessMessage"] = $"Đã xóa tài khoản '{account.AccountName}' thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa tài khoản: {ex.Message}";
            }

            return RedirectToAction(nameof(ManageAccounts));
        }

        #endregion

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}