using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace newspaper.net.Models.Dtos
{
	public class UserDto
	{
        public int Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
