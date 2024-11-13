using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.Dto
{
    public class OrderItemDto
    {
        [Required]
        public int ProductId { get; set; } // Produktens ID

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive integer.")]
        public int Quantity { get; set; } // Antal av produkten
        public decimal Price { get; set; }

    }
}
