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

        [Authorize]
        public IActionResult Index()
		{
            if (User.Identity.IsAuthenticated)
            {
                // User is authenticated
                // Access user information using User.Identity.Name, User.FindFirst, etc.
                return View();
            }
            else
            {
                // User is not authenticated, handle accordingly
                return RedirectToAction("Register", "Account");
            }
        }

		public IActionResult Privacy()
		{
            _logger.LogInformation("hfsdahfsahfsaf");
            return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
