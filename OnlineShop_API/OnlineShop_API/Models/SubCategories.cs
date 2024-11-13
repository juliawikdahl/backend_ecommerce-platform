using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.Models
{
    public class SubCategories
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Subcategory name cannot exceed 50 characters.")]
        public string Name { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public Categories Category { get; set; }

        public ICollection<Products> Products { get; set; }
    }
}
