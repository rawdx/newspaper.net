using newspaper.net.Data;
using newspaper.net.Models;
using System.Net.Mail;
using System.Net;

namespace newspaper.net.Services
{
    public interface IUserService
    {
        bool IsEmailAlreadyRegistered(string email);
        User RegisterUser(User user);
        bool VerifyEmail(string token);
        User AuthenticateUser(string email, string password);
    }

    public class UserService : IUserService
    {
        private readonly Context _context;

        public UserService(Context context)
        {
            _context = context;
        }

        public bool IsEmailAlreadyRegistered(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public User RegisterUser(User user)
        {
            if (IsEmailAlreadyRegistered(user.Email))
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            user.VerificationToken = GenerateEmailVerificationToken();
            user.Password = HashPassword(user.Password);
            user.VerificationToken = Guid.NewGuid().ToString();
            user.IsVerified = false;

            _context.Users.Add(user);
            _context.SaveChanges();

            SendVerificationEmail(user.Email, user.VerificationToken);

            return user;
        }

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

        public User AuthenticateUser(string email, string password)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private string GenerateEmailVerificationToken()
        {
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
