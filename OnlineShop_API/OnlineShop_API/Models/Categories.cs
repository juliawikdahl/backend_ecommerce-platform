using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.Models
{
    public class Categories
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
        public string Name { get; set; }

        public ICollection<SubCategories> SubCategories { get; set; }
    }
}
