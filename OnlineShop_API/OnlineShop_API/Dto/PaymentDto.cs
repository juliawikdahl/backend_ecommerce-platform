namespace OnlineShop_API.Models
{
    public class PaymentDto
    {
        public int OrderId { get; set; } // Koppla betalningen till en specifik order
        public string PaymentMethod { get; set; } // Betalningsmetod (t.ex. "card")
        public decimal Amount { get; set; } // Beloppet att betala
        public string Currency { get; set; } // Valuta (t.ex. "sek")
        public string Description { get; set; } // Beskrivning av betalningen (t.ex. ordernummer)
    }
}
