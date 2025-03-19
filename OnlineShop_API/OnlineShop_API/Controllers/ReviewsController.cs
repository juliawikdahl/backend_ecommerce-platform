using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop_API.Data;
using OnlineShop_API.DTOs; 
using OnlineShop_API.Models;
using System.Linq;
using System.Security.Claims;

namespace OnlineShop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly OnlineShopDbContext _context;

        public ReviewsController(OnlineShopDbContext context)
        {
            _context = context;
        }

        // GET: api/reviews/product/{productId}
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductReviews(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    UserId = r.UserId, // Inkludera användarens ID
                    UserName = _context.Users.FirstOrDefault(u => u.Id == r.UserId).Name,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // POST: api/reviews/product/{productId}
        [HttpPost("product/{productId}")]
        [Authorize] // Endast inloggade användare kan posta recensioner
        public async Task<IActionResult> PostReview(int productId, [FromBody] ReviewDto reviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId(); // Hämta den aktuella användarens ID

            var review = new Reviews
            {
                ProductId = productId,
                UserId = userId, // Sätt användarens ID
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
              
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Skapa en instans av ett svar DTO
            var responseDto = new
            {
                Rating = review.Rating,
                Comment = review.Comment,
                UserId = userId, // Inkludera userId i svaret
                
            };

            return CreatedAtAction(nameof(GetProductReviews), new { productId }, responseDto);
        }

        // Hjälpmetod för att hämta aktuell användarens ID från token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}
