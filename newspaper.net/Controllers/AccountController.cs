using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using newspaper.net.Data;
using newspaper.net.Models;
using System.Net.Mail;
using System.Net;
using newspaper.net.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace newspaper.net.Controllers
{
    /// <summary>
    /// Controller for user account-related actions such as registration, login, and logout.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly IVerificationService _verificationService;
        private readonly IPasswordChangeService _passwordChangeService;

        public AccountController(IUserService userService, ILogger<AccountController> logger, IAccountService accountService, IVerificationService verificationService, IPasswordChangeService passwordChangeService)
        {
            _userService = userService;
            _logger = logger;
            _accountService = accountService;
            _verificationService = verificationService;
            _passwordChangeService = passwordChangeService;
        }

        /// <summary>
        /// Displays the registration form.
        /// </summary>
        /// <returns>The registration view.</returns>
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        /// <summary>
        /// Handles the registration form submission.
        /// </summary>
        /// <param name="user">The user information.</param>
        /// <param name="profileImage">The user's profile image.</param>
        /// <returns>The registration confirmation view or the registration view with errors.</returns>

        [HttpPost]
        public IActionResult Register(User user, IFormFile profileImage)
        {
            try
            {
                user.Role = "User";
                var registeredUser = _accountService.RegisterUser(user, profileImage);
                _logger.LogInformation($"User registered successfully. Email: {user.Email}, UserId: {registeredUser.Id}");
                return View("RegisterConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error registering user with email {user.Email}: {ex.Message}");
                ModelState.AddModelError("Email", ex.Message);
                return View(user);
            }
        }

        /// <summary>
        /// Displays the login form.
        /// </summary>
        /// <returns>The login view.</returns>
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        /// <summary>
        /// Handles the login form submission.
        /// </summary>
        /// <param name="email">The user's email.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>Redirection to the appropriate page based on user role or the login view with errors.</returns>

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            try
            {
                var user = _accountService.AuthenticateUser(email, password);

                if (user != null)
                {
                    var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Email) };

                    if (!string.IsNullOrEmpty(user.Role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, user.Role));
                    }

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));


                    if (user.Role == "Admin")
                    {
                        _logger.LogInformation($"Admin user authenticated successfully. Email: {user.Email}, UserId: {user.Id}");

                        List<UserDto> userList = _userService.GetAllUsers();

                        return RedirectToAction("UserList", "Admin");
                    }
                    else
                    {
                        _logger.LogInformation($"User authenticated successfully. Email: {user.Email}, UserId: {user.Id}");

                        return RedirectToAction("Index", "Home");
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

        /// <summary>
        /// Logs the user out.
        /// </summary>
        /// <returns>Redirection to the login page.</returns>
        [HttpGet]
        public IActionResult Logout()
        {
			try
			{
				HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
				return RedirectToAction("Login");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error during Logout: {ex.Message}");
				return RedirectToAction("Login");
			}
		}

        /// <summary>
        /// Handles the email verification process based on the provided token.
        /// </summary>
        /// <param name="token">The verification token.</param>
        /// <returns>The verification success or failure view.</returns>
        [HttpGet]
        public IActionResult VerifyEmail(string token)
        {
            try
            {
                if (_verificationService.VerifyEmail(token))
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

        /// <summary>
        /// Displays the forgot password form.
        /// </summary>
        /// <returns>The forgot password view.</returns>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Handles the initiation of the password reset process.
        /// </summary>
        /// <param name="email">The user's email.</param>
        /// <returns>The password reset confirmation or error view.</returns>
        [HttpPost]
        public IActionResult ForgotPassword(string email)
		{
			try
			{
				var user = _userService.GetUserByEmail(email);

				if (user != null)
				{
					string token = Guid.NewGuid().ToString();

					// Save the token in the database with user ID and expiration time
					_passwordChangeService.SavePasswordResetToken(user, token);

					// Send a password reset email with the token
					_passwordChangeService.SendPasswordResetEmail(user.Email, token);

					// Redirect to a confirmation page
					return RedirectToAction("ForgotPasswordConfirmation");
				}

				// If email doesn't exist, show an error message
				ModelState.AddModelError("", "Email not found");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error initiating password reset: {ex.Message}");
				ModelState.AddModelError("", "An error occurred while processing the password reset request");
			}

			return View();
		}

        /// <summary>
        /// Displays a confirmation message after initiating the password reset process.
        /// </summary>
        /// <returns>The password reset confirmation view.</returns>
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// Displays the reset password form.
        /// </summary>
        /// <param name="email">The user's email.</param>
        /// <param name="token">The reset token.</param>
        /// <returns>The reset password view.</returns>
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {

            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        /// <summary>
        /// Handles the password reset process based on the provided token.
        /// </summary>
        /// <param name="model">The reset password view model.</param>
        /// <returns>The password reset confirmation or error view.</returns>
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
			try
			{
				// Validate the token and check if it's still valid
				var user = _passwordChangeService.ValidatePasswordResetToken(model.Email, model.Token);

				if (user != null)
				{
					// Update the user's password in the database
					_passwordChangeService.ResetPassword(user, model.Password);

					// Redirect to a password reset success page
					return RedirectToAction("ResetPasswordConfirmation");
				}
				else
				{
					// Invalid token or token expired
					ModelState.AddModelError("", "Invalid or expired password reset token");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error resetting password: {ex.Message}");
				ModelState.AddModelError("", "An error occurred while resetting the password");
			}
			return View(model);
		}

		public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
