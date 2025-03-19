using OnlineShop_API.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.Dto
{
    public class OrderCreateDto
    {
     

        [Required]
        public List<OrderItemCreateDto> OrderItems { get; set; } // Lista av orderartiklar

   

    }
}
