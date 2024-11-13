using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; } // Beloppet för betalningen

        [Required]
        public string PaymentMethod { get; set; } // Betalningsmetod (t.ex. kreditkort, PayPal)

        [Required]
        public DateTime PaymentDate { get; set; } // Datum för betalningen

        public string Status { get; set; } // Status för betalningen (t.ex. 'Completed', 'Pending', 'Failed')

        // Referens till ordern som betalningen gäller
        [Required]
        public int OrderId { get; set; }
        public Orders Order { get; set; } // Navigation property
    }
}
