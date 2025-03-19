using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop_API.Data;
using OnlineShop_API.Dto;
using OnlineShop_API.DTOs;
using OnlineShop_API.Identity;
using OnlineShop_API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly OnlineShopDbContext _context;

        public ProductsController(OnlineShopDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductsDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.SubCategory)
                .ThenInclude(sc => sc.Category)  // Inkludera subkategori och kategori
                .ToListAsync();

            // Skapa en lista av DTO:er
            var productDtos = products.Select(product => new ProductsDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                SubCategoryId = product.SubCategoryId,
                SubCategoryName = product.SubCategory.Name,
                CategoryId = product.SubCategory.CategoryId,
                CategoryName = product.SubCategory?.Category?.Name,
                EncodedImage = product.EncodedImage,
            }).ToList();

            return Ok(productDtos);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductsDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.SubCategory)
                .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var responseDto = new ProductsDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                SubCategoryId = product.SubCategoryId,
                SubCategoryName = product.SubCategory.Name,
                CategoryId = product.SubCategory.CategoryId,
                CategoryName = product.SubCategory?.Category?.Name,
                EncodedImage = product.EncodedImage,
            };

            return Ok(responseDto);
        }

        // GET: api/products/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<object>> GetProductsByCategory(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            var products = await _context.Products
                .Where(p => p.SubCategory.CategoryId == categoryId)
                .Include(p => p.SubCategory)
                .ToListAsync();

            var response = new
            {
                categoryName = category.Name,
                products = products.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.EncodedImage,
                    p.StockQuantity,
                    SubCategory = new { p.SubCategory.Name }
                }).ToList()
            };

            return Ok(response);
        }

        // GET: api/products/subcategory/{subCategoryId}
        [HttpGet("subcategory/{subCategoryId}")]
        public async Task<ActionResult<IEnumerable<ProductsDto>>> GetProductBySubCategory(int subCategoryId)
        {
            // Hämta alla produkter som tillhör en viss subkategori
            var products = await _context.Products
                .Where(p => p.SubCategoryId == subCategoryId) // Filtrera efter subkategori
                .Include(p => p.SubCategory) // Inkludera subkategori
                .ToListAsync();

            if (products == null || !products.Any())
            {
                return NotFound("Inga produkter hittades för denna subkategori.");
            }

            // Skapa en lista av DTO:er för de produkter som finns i denna subkategori
            var productDtos = products.Select(product => new ProductsDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                EncodedImage = product.EncodedImage,
                SubCategoryId = product.SubCategoryId,
                StockQuantity = product.StockQuantity,
                SubCategoryName = product.SubCategory.Name
            }).ToList();

            return Ok(productDtos);
        }



        // POST: api/products
        [HttpPost]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<ActionResult<ProductsDto>> PostProduct([FromBody] ProductsDto productDto)
        {
            // Kontrollera om data har skickats
            if (productDto == null)
            {
                return BadRequest("Product data is required.");
            }

            // Kontrollera om subkategori finns
            var subCategoryExists = await _context.SubCategories.AnyAsync(sc => sc.Id == productDto.SubCategoryId);
            if (!subCategoryExists)
            {
                return NotFound("Subcategory not found.");
            }

            // Skapa en ny produkt
            var product = new Products
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                EncodedImage = productDto.EncodedImage,
                SubCategoryId = productDto.SubCategoryId,
                StockQuantity = productDto.StockQuantity,
                //CreatedAt = DateTime.UtcNow
            };

            // Lägg till den nya produkten i databasen
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Skapa ett DTO-svar med all produktinformation
            var responseDto = new ProductsDto
            {
                Id = product.Id,  // Skapa ett ID för den nya produkten som genererades i databasen
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                EncodedImage = product.EncodedImage,
                SubCategoryId = product.SubCategoryId,
                StockQuantity = product.StockQuantity,
                SubCategoryName = product.SubCategory?.Name, // Lägg till SubCategoryName om du vill visa den i svaret
                CategoryId = product.SubCategory?.CategoryId ?? 0, // Lägg till CategoryId om du vill inkludera den i svaret
                CategoryName = product.SubCategory?.Category?.Name, // Lägg till CategoryName om du vill inkludera den i svaret
                //CreatedAt = product.CreatedAt
            };

            // Skicka tillbaka en CreatedAtAction med information om den skapade produkten
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, responseDto);
        }


        // PUT: api/products/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<IActionResult> PutProduct(int id, [FromBody] ProductUpdateDto productDto)
        {
            if (productDto == null)
            {
                return BadRequest("Produktdata krävs.");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound("Produkten finns inte.");
            }

            // Uppdatera produktens subkategori, men rör inte subkategorins kategori
            if (productDto.SubCategoryId.HasValue)
            {
                var subCategoryExists = await _context.SubCategories.AnyAsync(sc => sc.Id == productDto.SubCategoryId);
                if (!subCategoryExists)
                {
                    return NotFound("Subkategori finns inte.");
                }
                product.SubCategoryId = productDto.SubCategoryId.Value; // Uppdatera endast produktens subkategori
            }

            // Uppdatera övriga fält om de har ändrats
            if (!string.IsNullOrEmpty(productDto.Name))
            {
                product.Name = productDto.Name;
            }

            if (!string.IsNullOrEmpty(productDto.Description))
            {
                product.Description = productDto.Description;
            }

            if (productDto.Price.HasValue)
            {
                product.Price = productDto.Price.Value;
            }

            if (productDto.StockQuantity.HasValue)
            {
                product.StockQuantity = productDto.StockQuantity.Value;
            }

            if (!string.IsNullOrEmpty(productDto.EncodedImage))
            {
                product.EncodedImage = productDto.EncodedImage;
            }

            // Vi uppdaterar **inte** subkategorins kategori här!
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Skapa ett DTO-svar för den uppdaterade produkten
            var responseDto = new ProductsDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                EncodedImage = product.EncodedImage,
                SubCategoryId = product.SubCategoryId,
                StockQuantity = product.StockQuantity,
                SubCategoryName = product.SubCategory?.Name,
                CategoryId = product.SubCategory?.CategoryId ?? 0, // Hämtar CategoryId från SubCategory
                CategoryName = product.SubCategory?.Category?.Name,
            };

            return Ok(responseDto);
        }




        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
