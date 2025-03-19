using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.Dto
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }  // Kan vara produktens namn
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string EncodedImage { get; set; }

    }
}
