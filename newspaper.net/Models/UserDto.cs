namespace newspaper.net.Models
{
    /// <summary>
    /// Data transfer object (DTO) for user information.
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }

        public string Role { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string ProfileImage { get; set; }

        public bool IsVerified { get; set; }
    }
}
