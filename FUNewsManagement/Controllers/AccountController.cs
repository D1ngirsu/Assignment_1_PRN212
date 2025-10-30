using BusinessObjects.Models;
using FUNewsManagement.Helpers;
using FUNewsManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace FUNewsManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly ISystemAccountService _accountService;

        public AccountController(
            IAuthenticationService authService,
            ISystemAccountService accountService)
        {
            _authService = authService;
            _accountService = accountService;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã login rồi thì redirect về Home
            if (HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Authenticate user
                var user = await _authService.AuthenticateAsync(model.Email, model.Password);

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                // Set session
                HttpContext.Session.SetCurrentUser(user);

                // Log successful login
                Console.WriteLine($"User {user.AccountEmail} logged in successfully");

                // Redirect
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Login failed: {ex.Message}");
                return View(model);
            }
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // Nếu đã login rồi thì redirect về Home
            if (HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check if email exists
                if (await _accountService.EmailExistsAsync(model.AccountEmail))
                {
                    ModelState.AddModelError("AccountEmail", "Email already exists");
                    return View(model);
                }

                // Create new account
                var account = new SystemAccount
                {
                    AccountName = model.AccountName,
                    AccountEmail = model.AccountEmail,
                    AccountPassword = model.AccountPassword,
                    AccountRole = 3 // Default role: Lecturer
                };

                await _accountService.CreateAccountAsync(account);

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Registration failed: {ex.Message}");
                return View(model);
            }
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Clear session
            HttpContext.Session.ClearSession();

            TempData["InfoMessage"] = "You have been logged out successfully.";
            return RedirectToAction(nameof(Login));
        }

        // GET: Account/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            // Check if logged in
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            var currentUser = HttpContext.Session.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(currentUser);
        }

        // GET: Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            // Check if logged in
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = HttpContext.Session.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return RedirectToAction(nameof(Login));
                }

                // Verify current password
                var currentUser = await _accountService.GetAccountByIdAsync(userId.Value);
                if (currentUser == null || currentUser.AccountPassword != model.CurrentPassword)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
                    return View(model);
                }

                // Change password
                await _accountService.ChangePasswordAsync(userId.Value, model.NewPassword);

                // Update session
                currentUser.AccountPassword = model.NewPassword;
                HttpContext.Session.SetCurrentUser(currentUser);

                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Failed to change password: {ex.Message}");
                return View(model);
            }
        }

        // GET: Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}