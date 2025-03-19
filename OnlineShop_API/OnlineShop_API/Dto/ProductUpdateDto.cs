using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.DTOs
{
    public class ProductUpdateDto
    {
        [Required]
        public int Id { get; set; } // ID, krävs för att hitta produkten

        [StringLength(100, ErrorMessage = "Produktnamnet får inte vara längre än 100 tecken.")]
        public string Name { get; set; } // Namnet på produkten, valfritt vid uppdatering

        [StringLength(500, ErrorMessage = "Beskrivningen får inte vara längre än 500 tecken.")]
        public string Description { get; set; } // Beskrivning av produkten, valfritt vid uppdatering

        [Range(0.01, double.MaxValue, ErrorMessage = "Priset måste vara större än noll.")]
        public decimal? Price { get; set; } // Priset på produkten, valfritt vid uppdatering

        [Range(0, int.MaxValue, ErrorMessage = "Lagerantalet måste vara ett icke-negativt heltal.")]
        public int? StockQuantity { get; set; } // Lagerantalet för produkten, valfritt vid uppdatering

        public int? SubCategoryId { get; set; } // ID för subkategorin, valfritt vid uppdatering
        public string SubCategoryName { get; set; } // Subkategori namn, valfritt vid uppdatering

        public int? CategoryId { get; set; } // ID för huvudkategori, valfritt vid uppdatering
        public string CategoryName { get; set; } // Kategorinamn, valfritt vid uppdatering

        public string EncodedImage { get; set; } // Base64-sträng för produktens bild, valfritt vid uppdatering
    }
}
