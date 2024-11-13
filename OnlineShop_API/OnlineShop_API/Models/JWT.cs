using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.Models
{
    public class JWT
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public Users User { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }
    }
}
