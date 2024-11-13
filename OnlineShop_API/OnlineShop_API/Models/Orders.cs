using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.Models
{
    public class Orders
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public Users User { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be a positive value.")]
        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public ICollection<OrderItems> OrderItems { get; set; }

        [Required]
        public Status Status { get; set; }
    }

    public enum Status
    {
        Pending,
        Shipped,
        Delivered,
        Canceled
    }
}
