using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.DTOs
{
    public class ReviewDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Comment cannot be longer than 500 characters.")]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
