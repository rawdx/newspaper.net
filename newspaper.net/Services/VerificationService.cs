using System.Net.Mail;
using System.Net;
using newspaper.net.Models;
using newspaper.net.Data;

namespace newspaper.net.Services
{
    /// <summary>
    /// Service for email verification operations.
    /// </summary>
    public interface IVerificationService
    {
        /// <summary>
        /// Verifies the email using the provided verification token.
        /// </summary>
        /// <param name="token">The verification token.</param>
        /// <returns><c>true</c> if the email is verified successfully; otherwise, <c>false</c>.</returns>
        bool VerifyEmail(string token);

        /// <summary>
        /// Sends an email with a verification link to the provided email address.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="verificationToken">The verification token to include in the link.</param>
        void SendVerificationEmail(string email, string verificationToken);
    }

    /// <summary>
    /// Implementation of the <see cref="IVerificationService"/> interface.
    /// </summary>
    public class VerificationService : IVerificationService
    {
        private readonly Context _context;

        public VerificationService(Context context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public bool VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var user = _context.Users.FirstOrDefault(u => u.VerificationToken == token);

            if (user != null)
            {
                user.IsVerified = true;
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void SendVerificationEmail(string email, string verificationToken)
        {
            if (IsValidEmail(email))
            {
                string senderEmail = "jihawww@gmail.com";
                string senderPassword = "atjwgryavsfvwabf";

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
            }
            else
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
