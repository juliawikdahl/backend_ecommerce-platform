using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.Models
{
    public class Products
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Product name must not exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal Price { get; set; }

        public string EncodedImage { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        public SubCategories SubCategory { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a non-negative integer.")]
        public int StockQuantity { get; set; }

        public ICollection<WishList> WishList { get; set; }
        public ICollection<Reviews> Reviews { get; set; }
        public ICollection<OrderItems> OrderItems { get; set; }
    }
}
