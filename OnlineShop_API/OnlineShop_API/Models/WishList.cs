using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.Models
{
    public class WishList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Users User { get; set; }
        public Products Product { get; set; }
    }
}
