using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; } // Lista av subkategorier
    }

}