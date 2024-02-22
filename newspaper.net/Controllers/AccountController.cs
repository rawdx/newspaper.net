using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using newspaper.net.Data;
using newspaper.net.Models;
using System.Net.Mail;
using System.Net;
using newspaper.net.Services;

namespace newspaper.net.Controllers
{
	public class AccountController : Controller
	{
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;

        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            try
            {
                user.Role = "User";
                var registeredUser = _userService.RegisterUser(user);
                _logger.LogInformation($"User registered successfully. Email: {user.Email}, UserId: {registeredUser.Id}");
                return View("RegisterConfirmation");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"Error registering user with email {user.Email}: {ex.Message}");
                ModelState.AddModelError("Email", ex.Message);
                return View(user);
            }
        }

        [HttpGet]
        public IActionResult VerifyEmail(string token)
        {
            try
            {
                if (_userService.VerifyEmail(token))
                {
                    _logger.LogInformation($"Email verification successful. Token: {token}");
                    return View("VerifyEmailSuccess");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying email with token {token}: {ex.Message}");
            }

            return View("VerifyEmailFailure");
        }

        [HttpGet]
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Login(string email, string password)
        {
            try
            {
                var user = _userService.AuthenticateUser(email, password);

                if (user != null)
                {
                    if (user.Role == "Admin")
                    {
                        _logger.LogInformation($"Admin user authenticated successfully. Email: {user.Email}, UserId: {user.Id}");

                        List<User> userList = _userService.GetAllUsers();

                        return View("~/Views/Admin/UserList.cshtml", userList);
                    }
                    else
                    {
                        _logger.LogInformation($"User authenticated successfully. Email: {user.Email}, UserId: {user.Id}");
                        return View("~/Views/Home/Index");
                    }
                }

                _logger.LogWarning($"Invalid login attempt for email: {email}");
                ModelState.AddModelError("", "Invalid email or password");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error authenticating user with email {email}: {ex.Message}");
            }

            return View();
        }
    }
}
