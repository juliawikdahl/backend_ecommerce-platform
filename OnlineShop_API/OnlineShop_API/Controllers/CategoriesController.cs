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
    public class CategoriesController : ControllerBase
    {
        private readonly OnlineShopDbContext _context;

        public CategoriesController(OnlineShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .ToListAsync();

            var result = categories.Select(c => new
            {
                Id = c.Id, 
                Name = c.Name, 
                SubCategories = c.SubCategories.Select(sc => new
                {
                    Id = sc.Id, 
                    Name = sc.Name 
                }).ToList()
            }).ToList();

            return Ok(result);
        }


        // POST: api/categories
        [HttpPost]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<ActionResult<Categories>> PostCategory([FromBody] CategoryDto categoryDto)
        {
            if (categoryDto == null || string.IsNullOrEmpty(categoryDto.Name))
            {
                return BadRequest("Category name is required.");
            }

            var category = new Categories
            {
                Name = categoryDto.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, new { category.Id, category.Name });
        }
        // GET: api/categories/subcategories
        [HttpGet("subcategories")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllSubcategories()
        {
            var subcategories = await _context.SubCategories
                .Include(sc => sc.Category)
                .ToListAsync();

            var result = subcategories.Select(sc => new
            {
                Id = sc.Id,
                Name = sc.Name,
                Category = new
                {
                    Id = sc.Category.Id,
                    Name = sc.Category.Name
                }
            }).ToList();

            return Ok(result);
        }


        // POST: api/categories/{categoryId}/subcategories
        [HttpPost("{categoryId}/subcategories")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<ActionResult<object>> PostSubcategory(int categoryId, [FromBody] SubCategoryDto subCategoryDto)
        {
            if (subCategoryDto == null || string.IsNullOrEmpty(subCategoryDto.Name))
            {
                return BadRequest("Subcategory name is required.");
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                return NotFound("Category not found.");
            }

            var subcategory = new SubCategories
            {
                CategoryId = categoryId,
                Name = subCategoryDto.Name
            };

            _context.SubCategories.Add(subcategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubcategories), new { categoryId = categoryId }, new { Id = subcategory.Id, Name = subcategory.Name });
        }

        // GET: api/categories/{categoryId}/subcategories
        [HttpGet("{categoryId}/subcategories")]
        public async Task<ActionResult<IEnumerable<SubCategories>>> GetSubcategories(int categoryId)
        {
            var subcategories = await _context.SubCategories.Where(sc => sc.CategoryId == categoryId).ToListAsync();
            return Ok(subcategories);
        }

        // GET: api/categories/{categoryId}
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<Categories>> GetCategory(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        // PUT: api/categories/{categoryId}
        [HttpPut("{categoryId}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<IActionResult> PutCategory(int categoryId, [FromBody] CategoryDto categoryDto)
        {
            if (string.IsNullOrEmpty(categoryDto.Name))
            {
                return BadRequest("Category name is required.");
            }

            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = categoryDto.Name;

            _context.Entry(category).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPut("subcategories/{subcategoryId}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<IActionResult> PutSubcategory(int subcategoryId, [FromBody] UpdateSubCategoryDto subCategoryDto)
        {
            if (subCategoryDto == null || string.IsNullOrEmpty(subCategoryDto.Name))
            {
                return BadRequest("Subcategory name is required.");
            }

            // Hämta subkategorin baserat på subcategoryId
            var subcategory = await _context.SubCategories.FindAsync(subcategoryId);
            if (subcategory == null)
            {
                return NotFound($"Subcategory with ID {subcategoryId} not found.");
            }

            // Om categoryId har ändrats
            if (subCategoryDto.CategoryId != subcategory.CategoryId)
            {
                // Hämta den nya kategorin baserat på det nya categoryId
                var category = await _context.Categories.FindAsync(subCategoryDto.CategoryId);
                if (category == null)
                {
                    return NotFound($"Category with ID {subCategoryDto.CategoryId} not found.");
                }

                // Uppdatera categoryId för subkategorin
                subcategory.CategoryId = subCategoryDto.CategoryId;
            }

            // Uppdatera subkategoriens namn
            subcategory.Name = subCategoryDto.Name;

            // Markera att subkategorin har ändrats
            _context.Entry(subcategory).State = EntityState.Modified;

            // Spara ändringarna i databasen
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }


        [HttpDelete("subcategories/{subcategoryId}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<IActionResult> DeleteSubcategory(int subcategoryId)
        {
            var subcategory = await _context.SubCategories.FindAsync(subcategoryId);
            if (subcategory == null)
            {
                return NotFound("Subcategory not found.");
            }

            _context.SubCategories.Remove(subcategory);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }


        // DELETE: api/categories/{categoryId}
        [HttpDelete("{categoryId}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        private bool SubcategoryExists(int id)
        {
            return _context.SubCategories.Any(e => e.Id == id);
        }
    }
}
