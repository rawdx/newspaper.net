using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using newspaper.net.Data;
using newspaper.net.Models;
using System.Net.Mail;
using System.Net;

namespace newspaper.net.Controllers
{
	public class AccountController : Controller
	{
		private readonly Context _context;

		public AccountController(Context context)
		{
			_context = context;
		}

		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Register(User user)
		{
			if (ModelState.IsValid)
			{
				if (IsEmailAlreadyRegistered(user.Email))
				{
					ModelState.AddModelError("Email", "Email is already registered.");
					return View(user);
				}
                user.VerificationToken = GenerateEmailVerificationToken();

                user.Password = HashPassword(user.Password);

				user.VerificationToken = Guid.NewGuid().ToString();
				user.IsVerified = false;

				_context.Users.Add(user);
				_context.SaveChanges();

				SendVerificationEmail(user.Email, user.VerificationToken);

				return RedirectToAction("Login");
			}

			return View(user);
		}

		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Login(string email, string password)
		{
			// Verify user credentials and set authentication cookie if successful

			var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

			if (user != null)
			{
				HttpContext.Response.Cookies.Append("UserId", user.Id.ToString());

				return RedirectToAction("Index", "Home");
			}

			// Invalid credentials
			ModelState.AddModelError("", "Invalid email or password");
			return View();
		}

        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyEmail(string verificationToken)
        {
            if (string.IsNullOrEmpty(verificationToken))
            {
                // Handle invalid or missing token
                return View("VerifyEmailFailure");
            }

            var user = _context.Users.FirstOrDefault(u => u.VerificationToken == verificationToken);

            if (user != null)
            {
                user.IsVerified = true;
                _context.SaveChanges();

                return View("VerifyEmailSuccess");
            }

            return View("VerifyEmailFailure");
        }

        private bool IsEmailAlreadyRegistered(string email)
		{
			return _context.Users.Any(u => u.Email == email);
		}

		private string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password);
		}

        private string GenerateEmailVerificationToken()
        {
            // Generate a unique token (you may use a library or a specific algorithm)
            return Guid.NewGuid().ToString();
        }

        private void SendVerificationEmail(string email, string verificationToken)
        {
            if (IsValidEmail(email))
            {
                string senderEmail = "jihawww@gmail.com"; // Your Gmail email address
                string senderPassword = "atjwgryavsfvwabf"; // Your Gmail password or App Password

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail);
                    mail.To.Add(email);
                    mail.Subject = "Email Verification";
                    mail.Body = $"Click the link to verify your email: https://localhost:7062/Account/VerifyEmail?token={verificationToken}";
                    mail.IsBodyHtml = false;

                    using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com"))
                    {
                        smtpClient.Port = 587;
                        smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                        smtpClient.EnableSsl = true;

                        try
                        {
                            smtpClient.Send(mail);
                        }
                        catch (Exception ex)
                        {
                            // Handle exceptions, log, or throw
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            } else
            {
                Console.WriteLine("Invalid email address");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
