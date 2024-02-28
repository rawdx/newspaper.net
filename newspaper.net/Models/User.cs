using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace newspaper.net.Models
{
    /// <summary>
    /// Represents a user entity in the application.
    /// </summary>
    public class User
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(255)]
		public required string Email { get; set; }

		[Required]
		[StringLength(255)]
		public required string Password { get; set; }

		[StringLength(50)]
		public string? Name { get; set; }

		[StringLength(15)]
		public string? Phone {  get; set; }

        public byte[]? ProfileImage { get; set; }

        public string? VerificationToken { get; set; }

		public bool IsVerified { get; set; }

        [Required]
        [StringLength(20)]
        public required string Role { get; set; }

        public string? ResetToken { get; set; }

		public DateTime? ResetTokenExpiration { get; set; }

		public User(string name, string phone, bool isVerified, string role)
		{
			Name = name;
			Phone = phone;
			IsVerified = isVerified;
			Role = role;
		}

		public User()
		{
		}
	}
}
