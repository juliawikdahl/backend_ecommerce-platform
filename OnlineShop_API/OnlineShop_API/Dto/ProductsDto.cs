using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.DTOs
{
    public class ProductsDto
    {
        [Key]
        [Required]
        public int Id { get; set; } // ID 


        [Required]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        public string Name { get; set; } // Namnet på produkten

        [Required]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } // Beskrivning av produkten

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; } // Priset på produkten

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a non-negative integer.")]
        public int StockQuantity { get; set; } // Lagerantalet för produkten

        [Required]
        public int SubCategoryId { get; set; } // ID för subkategorin produkten tillhör
        public string SubCategoryName { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        [Required]
        public string EncodedImage { get; set; } // Base64-sträng för produktens bild
        public DateTime CreatedAt { get; set; }

    }
}
