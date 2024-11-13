using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.DTOs
{
    public class SubCategoryDto
    {
      
        [Required]
        [StringLength(50, ErrorMessage = "Subcategory name cannot exceed 50 characters.")]
        public string Name { get; set; } // Namnet på subkategorin
    }
}
