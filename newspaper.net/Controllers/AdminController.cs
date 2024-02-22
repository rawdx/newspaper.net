using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using newspaper.net.Models;
using newspaper.net.Services;

namespace newspaper.net.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult UserList()
        {
            var users = _userService.GetAllUsers();
            return View(users);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateUser(User user)
        {
            try
            {
                // Set the user role to "User" by default
                user.Role = "User";

                var registeredUser = _userService.RegisterUser(user);
                return RedirectToAction("UserList");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Email", ex.Message);
                return View(user);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult EditUser(int id)
        {
            var user = _userService.GetUserById(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult EditUser(User user)
        {
            try
            {
                _userService.UpdateUser(user);
                return RedirectToAction("UserList");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Email", ex.Message);
                return View(user);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUser(int id)
        {
            var user = _userService.GetUserById(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        [Authorize(Roles = "Admin")]
        public IActionResult ConfirmDeleteUser(int id)
        {
            _userService.DeleteUser(id);
            return RedirectToAction("UserList");
        }
    }
}