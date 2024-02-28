using System.ComponentModel.DataAnnotations;

namespace newspaper.net.Models
{
    /// <summary>
    /// Represents a view model for the forgot password functionality in the application.
    /// </summary>
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
