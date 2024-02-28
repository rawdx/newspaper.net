using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using newspaper.net.Models;
using newspaper.net.Services;

namespace newspaper.net.Controllers
{
    /// <summary>
    /// Controller for user administration actions in the admin section.
    /// </summary>
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
		private readonly ILogger<AdminController> _logger;

		public AdminController(IUserService userService, ILogger<AdminController> logger)
        {
            _userService = userService;
			_logger = logger;
		}

        /// <summary>
        /// Displays a list of users for administrative purposes.
        /// </summary>
        /// <returns>The user list view if the user is an admin; otherwise, redirects to the home page.</returns>

        [HttpGet]
		public IActionResult UserList()
		{
			try
			{
				if (User.IsInRole("Admin"))
				{
					var users = _userService.GetAllUsers();
					return View(users);
				}
				else
				{
					return RedirectToAction("Index", "Home");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in UserList action: {ex.Message}");
				return RedirectToAction("Index", "Home");
			}
		}

        /// <summary>
        /// Displays the form for creating a new user.
        /// </summary>
        /// <returns>The create user view.</returns>
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        /// <summary>
        /// Handles the submission of the create user form.
        /// </summary>
        /// <param name="user">The user information.</param>
        /// <param name="profileImage">The user's profile image.</param>
        /// <returns>Redirects to the user list view if successful; otherwise, shows errors on the create user view.</returns>
        [HttpPost]
        public IActionResult CreateUser(User user, IFormFile profileImage)
        {
			try
			{
				_userService.CreateUser(user, profileImage);
				return RedirectToAction("UserList");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in CreateUser action: {ex.Message}");
				ModelState.AddModelError("Email", ex.Message);
				return View(user);
			}

		}

        /// <summary>
        /// Displays the form for editing a user.
        /// </summary>
        /// <param name="id">The user ID to edit.</param>
        /// <returns>The edit user view if the user is found; otherwise, redirects to the user list.</returns>
        [HttpGet]
        public IActionResult EditUser(int id)
        {
			try
			{
				var user = _userService.GetUserById(id);

				if (user == null)
				{
					return RedirectToAction("UserList");
				}

				return View(user);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in EditUser (GET) action: {ex.Message}");
				return RedirectToAction("UserList");
			}
		}

        /// <summary>
        /// Handles the submission of the edit user form.
        /// </summary>
        /// <param name="user">The user information.</param>
        /// <param name="profileImage">The user's profile image.</param>
        /// <returns>Redirects to the user list view if successful; otherwise, shows errors on the edit user view.</returns>
        [HttpPost]
        public IActionResult EditUser(UserDto user, IFormFile profileImage)
        {
			try
			{
				_userService.UpdateUser(user, profileImage);
				return RedirectToAction("UserList");
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogError($"Error in EditUser (POST) action: {ex.Message}");
				ModelState.AddModelError("Email", ex.Message);
				return View(user);
			}
		}

        /// <summary>
        /// Displays the confirmation page for deleting a user.
        /// </summary>
        /// <param name="id">The user ID to delete.</param>
        /// <returns>The delete user view if the user is found; otherwise, redirects to the user list.</returns>	
        [HttpGet]
        public IActionResult DeleteUser(int id)
        {
			try
			{
				var user = _userService.GetUserById(id);

				if (user == null)
				{
					return NotFound();
				}

				if (user.Role.Equals("Admin"))
				{
					return RedirectToAction("UserList");
				}

				return View(user);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in DeleteUser (GET) action: {ex.Message}");
				return RedirectToAction("UserList");
			}
		}

        /// <summary>
        /// Handles the confirmation of deleting a user.
        /// </summary>
        /// <param name="id">The user ID to delete.</param>
        /// <returns>Redirects to the user list view if successful; otherwise, redirects to the user list.</returns>
        [HttpPost, ActionName("DeleteUser")]
        public IActionResult ConfirmDeleteUser(int id)
        {
			try
			{
				_userService.DeleteUser(id);
				return RedirectToAction("UserList");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in ConfirmDeleteUser action: {ex.Message}");
				return RedirectToAction("UserList");
			}
		}
    }
}