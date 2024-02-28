using newspaper.net.Models;
using System.Net.Mail;
using System.Net;
using newspaper.net.Data;

namespace newspaper.net.Services
{
    /// <summary>
    /// Service for handling password change and reset operations.
    /// </summary>
    public interface IPasswordChangeService
    {
        /// <summary>
        /// Sends a password reset email to the specified email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="resetToken">The password reset token.</param>
        void SendPasswordResetEmail(string email, string resetToken);

        /// <summary>
        /// Validates a password reset token for a given email.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="token">The password reset token.</param>
        /// <returns>The user if the token is valid; otherwise, null.</returns>
        User ValidatePasswordResetToken(string email, string token);

        /// <summary>
        /// Resets the password for the specified user.
        /// </summary>
        /// <param name="user">The user for whom the password should be reset.</param>
        /// <param name="newPassword">The new password to set.</param>
        void ResetPassword(User user, string newPassword);

        /// <summary>
        /// Saves a password reset token for the user.
        /// </summary>
        /// <param name="user">The user for whom the token is saved.</param>
        /// <param name="token">The password reset token.</param>
        void SavePasswordResetToken(User user, string token);
    }

    /// <summary>
    /// Implementation of the <see cref="IPasswordChangeService"/> interface.
    /// </summary>
    public class PasswordChangeService : IPasswordChangeService
    {
        private readonly Context _context;
        private readonly IUserService _userService;

        public PasswordChangeService(Context context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        /// <inheritdoc />
        public void SendPasswordResetEmail(string email, string resetToken)
        {
            if (IsValidEmail(email))
            {
                string senderEmail = "jihawww@gmail.com";
                string senderPassword = "atjwgryavsfvwabf";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail);
                    mail.To.Add(email);
                    mail.Subject = "Password Reset Request";
                    mail.Body = $"Click the link to reset your password: https://localhost:7062/Account/ResetPassword?token={resetToken}&email={email}";
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
            }
            else
            {
                Console.WriteLine("Invalid email address");
            }
        }

        /// <inheritdoc />
        public void SavePasswordResetToken(User user, string token)
        {

            user.ResetToken = token;
            user.ResetTokenExpiration = DateTime.UtcNow.AddHours(1);

            _context.SaveChanges();
        }

        /// <inheritdoc />
        public User ValidatePasswordResetToken(string email, string token)
        {
            // Retrieve user by email
            var user = _userService.GetUserByEmail(email);

            if (user != null && IsPasswordResetTokenValid(user, token))
            {
                return user;
            }
            return null;
        }

        /// <inheritdoc />
        public void ResetPassword(User user, string newPassword)
        {
            // Hash the new password and update it in the database
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Update other necessary fields, e.g., reset the password reset token
            user.ResetToken = null;
            user.ResetTokenExpiration = null;

            // Save changes to the database
            _context.SaveChanges();
        }

        private bool IsPasswordResetTokenValid(User user, string token)
        {
            // Check if the token matches and has not expired
            return user.ResetToken == token
                && user.ResetTokenExpiration.HasValue
                && user.ResetTokenExpiration > DateTime.UtcNow;
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
