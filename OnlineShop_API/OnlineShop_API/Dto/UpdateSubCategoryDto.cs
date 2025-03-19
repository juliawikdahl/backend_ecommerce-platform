namespace OnlineShop_API.Dto
{
    public class UpdateSubCategoryDto
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }  // Lägg till CategoryId för PUT
    }

}
