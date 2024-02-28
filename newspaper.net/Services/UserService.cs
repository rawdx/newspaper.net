using newspaper.net.Data;
using newspaper.net.Models;
using System.Net.Mail;
using System.Net;
using newspaper.net.Controllers;

namespace newspaper.net.Services
{
    /// <summary>
    /// Service for managing user-related operations.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Converts a <see cref="User"/> entity to a <see cref="UserDto"/> data transfer object.
        /// </summary>
        /// <param name="user">The user entity to convert.</param>
        /// <returns>The corresponding <see cref="UserDto"/>.</returns>
        UserDto ConvertToUserDto(User user);

        /// <summary>
        /// Retrieves a list of all users as <see cref="UserDto"/>.
        /// </summary>
        /// <returns>The list of all users.</returns>
        List<UserDto> GetAllUsers();

        /// <summary>
        /// Retrieves a user by their unique identifier as <see cref="UserDto"/>.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The user as <see cref="UserDto"/>.</returns>
        UserDto GetUserById(int id);

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        User GetUserByEmail(string email);

        /// <summary>
        /// Updates user information and profile image.
        /// </summary>
        /// <param name="userDto">The updated user information.</param>
        /// <param name="profileImage">The new profile image.</param>
        void UpdateUser(UserDto user, IFormFile profileImage);

        /// <summary>
        /// Creates a new user with the specified information and profile image.
        /// </summary>
        /// <param name="user">The user entity to create.</param>
        /// <param name="profileImage">The profile image of the user.</param>
        void CreateUser(User user, IFormFile profileImage);

        /// <summary>
        /// Deletes a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        void DeleteUser(int id);

    }

    /// <summary>
    /// Implementation of the <see cref="IUserService"/> interface.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly Context _context;
        private readonly IVerificationService _verificationService;
        private readonly IAccountService _accountService;

        public UserService(Context context, IVerificationService verificationService, IAccountService accountService)
        {
            _context = context;
            _verificationService = verificationService;
            _accountService = accountService;
        }

        /// <inheritdoc />
        public User GetUserByEmail(string email)
        {
            return _context.Users.First(u => u.Email == email);
        }

        /// <inheritdoc />
        public UserDto ConvertToUserDto (User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                Name = user.Name,
                Phone = user.Phone,
                ProfileImage = user.ProfileImage != null ? Convert.ToBase64String(user.ProfileImage) : null,
                IsVerified = user.IsVerified,
            };
        }

        /// <inheritdoc />
        public List<UserDto> GetAllUsers()
        {
            List<UserDto> userDtos = new List<UserDto>();

            var users = _context.Users.ToList();

            foreach (var user in users)
            {
                userDtos.Add(ConvertToUserDto(user));
            }
            return userDtos;
        }

        /// <inheritdoc />
        public UserDto GetUserById(int id)
        {
            User user = _context.Users.Find(id);

            if (user != null)
            {
                return ConvertToUserDto(user);
            }
            return null;
		}

        /// <inheritdoc />
        public void UpdateUser(UserDto userDto, IFormFile profileImage)
        {

            var user = _context.Users.Find(userDto.Id);

            if (user != null)
            {
				user.Name = userDto.Name;
				user.Phone = userDto.Phone;
				user.IsVerified = userDto.IsVerified;
				user.Role = userDto.Role;

				if (profileImage != null)
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						profileImage.CopyTo(memoryStream);
						user.ProfileImage = memoryStream.ToArray();
					}
				}

				_context.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException("User not found.");
            }
        }

        /// <inheritdoc />
        public void CreateUser(User user, IFormFile profileImage)
        {
            if (_accountService.IsEmailAlreadyRegistered(user.Email))
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.VerificationToken = Guid.NewGuid().ToString();

			if (profileImage != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					profileImage.CopyTo(memoryStream);
					user.ProfileImage = memoryStream.ToArray();
				}
			}

			_context.Users.Add(user);
            _context.SaveChanges();

            if(!user.IsVerified)
                _verificationService.SendVerificationEmail(user.Email, user.VerificationToken);
        }

        /// <inheritdoc />
        public void DeleteUser(int id)
        {
            var user = _context.Users.Find(id);

            if (user != null)
            {
				_context.Users.Remove(user);
                _context.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException("User not found.");
            }
        }
    }
}
