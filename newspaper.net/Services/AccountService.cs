using newspaper.net.Data;
using newspaper.net.Models;

namespace newspaper.net.Services
{
    /// <summary>
    /// Service for user account operations.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Registers a new user with the provided information.
        /// </summary>
        /// <param name="user">The user information.</param>
        /// <param name="profileImage">The user's profile image.</param>
        /// <returns>The registered user.</returns>
        User RegisterUser(User user, IFormFile profileImage);

        /// <summary>
        /// Checks if the specified email is already registered.
        /// </summary>
        /// <param name="email">The email to check.</param>
        /// <returns><c>true</c> if the email is already registered; otherwise, <c>false</c>.</returns>
        bool IsEmailAlreadyRegistered(string email);

        /// <summary>
        /// Authenticates a user with the provided email and password.
        /// </summary>
        /// <param name="email">The user's email.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The authenticated user or <c>null</c> if authentication fails.</returns>
        User? AuthenticateUser(string email, string password);
    }

    /// <summary>
    /// Implementation of the <see cref="IAccountService"/> interface.
    /// </summary>
    public class AccountService: IAccountService
    {
        private readonly Context _context;
        private readonly IVerificationService _verificationService;

        public AccountService(Context context, IVerificationService verificationService)
        {
            _context = context;
            _verificationService = verificationService;
        }

        /// <inheritdoc />
        public User RegisterUser(User user, IFormFile profileImage)
        {
            if (IsEmailAlreadyRegistered(user.Email))
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.VerificationToken = Guid.NewGuid().ToString();
            user.IsVerified = false;

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

            _verificationService.SendVerificationEmail(user.Email, user.VerificationToken);

            return user;
        }

        /// <inheritdoc />
        public bool IsEmailAlreadyRegistered(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        /// <inheritdoc />
        public User? AuthenticateUser(string email, string password)
        {
            var storedUser = _context.Users.FirstOrDefault(u => u.Email == email);

            if (storedUser != null)
            {
                if (BCrypt.Net.BCrypt.Verify(password, storedUser.Password))
                {
                    return storedUser;
                }
            }

            return null;
        }
    }
}
