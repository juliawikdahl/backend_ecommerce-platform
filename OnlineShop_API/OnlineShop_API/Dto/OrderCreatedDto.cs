namespace OnlineShop_API.Dto
{
    public class OrderCreatedDto
    {
        public int OrderId { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } // Listan med ordervaror
        public string Status { get; set; } // Orderstatus
        public DateTime OrderDate { get; set; } // Datum för ordern
        public decimal TotalAmount { get; set; } // Totalt belopp för ordern
    }
}
