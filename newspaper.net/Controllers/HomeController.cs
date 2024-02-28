using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using newspaper.net.Models;
using System.Diagnostics;

namespace newspaper.net.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

        /// <summary>
        /// Action for the home page.
        /// If the user is authenticated, displays a personalized welcome message.
        /// Otherwise, redirects to the login page.
        /// </summary>
        /// <returns>ViewResult</returns>
        public IActionResult Index()
		{
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    ViewData["UserName"] = User.Identity.Name;

                    return View();
                }
                else
                {
                    return Redirect("/Account/Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Index action: {ex.Message}");
                return RedirectToAction("Login", "Account");
            }
        }

        /// <summary>
        /// Action for handling errors.
        /// </summary>
        /// <returns>ViewResult</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
