using OnlineShop_API.Models;

namespace OnlineShop_API.Dto
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public Status Status { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
