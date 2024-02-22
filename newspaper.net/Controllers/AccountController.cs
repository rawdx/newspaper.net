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
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
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
                var registeredUser = _userService.RegisterUser(user);
                return View("RegisterConfirmation");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Email", ex.Message);
                return View(user);
            }
        }

        [HttpGet]
        public IActionResult VerifyEmail(string token)
        {
            if (_userService.VerifyEmail(token))
            {
                return View("VerifyEmailSuccess");
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
            var user = _userService.AuthenticateUser(email, password);

            if (user != null)
            {
                // Set authentication cookie or use your preferred authentication mechanism

                return RedirectToAction("Index", "Home");
            }

            // Invalid credentials
            ModelState.AddModelError("", "Invalid email or password");
            return View();
        }
    }
}
