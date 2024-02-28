using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace newspaper.net.Models
{
    /// <summary>
    /// Represents an article in the application.
    /// </summary>
    public class Article
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Subtitle { get; set; }

        [Required]
        [Column(TypeName = "TEXT")]
        public required string Content { get; set; }
 
        public byte[]? Image { get; set; }

        [Required]
        public required DateTime CreationDate { get; set; }

        public DateTime? PublicationDate { get; set; }

        [Required]
        public required string Status { get; set; }

        public string? Priority { get; set; }

        public int WriterId { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("WriterId")]
        public User User { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}
