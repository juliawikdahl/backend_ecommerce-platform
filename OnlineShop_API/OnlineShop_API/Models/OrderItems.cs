using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.Models
{
    public class OrderItems
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Orders Order { get; set; }
        public Products Product { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive integer.")]
        public int Quantity { get; set; }
    }
}
