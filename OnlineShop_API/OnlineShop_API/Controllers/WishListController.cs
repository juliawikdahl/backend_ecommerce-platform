using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop_API.Data;
using OnlineShop_API.DTOs;
using OnlineShop_API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineShop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Endast inloggade användare får använda dessa metoder
    public class WishlistController : ControllerBase
    {
        private readonly OnlineShopDbContext _context;

        public WishlistController(OnlineShopDbContext context)
        {
            _context = context;
        }

        public class AddToWishlistDto
        {
            public int ProductId { get; set; }
        }

        // POST: api/wishlist
        [HttpPost]
        public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistDto dto)
        {
            var userId = GetCurrentUserId();

            // Kontrollera om produkten redan finns i användarens önskelista
            var existingItem = await _context.WishLists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == dto.ProductId);
            if (existingItem != null)
            {
                return Conflict("Product already in wishlist.");
            }

            // Lägg till produkt till användarens önskelista
            var wishlistItem = new WishList
            {
                UserId = userId,
                ProductId = dto.ProductId
            };

            _context.WishLists.Add(wishlistItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWishlist), new { productId = dto.ProductId }, wishlistItem);
        }


        // DELETE: api/wishlist/{productid}
        [HttpDelete("{productid}")]
        public async Task<IActionResult> RemoveFromWishlist(int productid)
        {
            var userId = GetCurrentUserId();

            var wishlistItem = await _context.WishLists // Justerat till WishLists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productid);
            if (wishlistItem == null)
            {
                return NotFound("Product not found in wishlist.");
            }

            _context.WishLists.Remove(wishlistItem); // Justerat till WishLists
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // GET: api/wishlist
        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = GetCurrentUserId();
            Console.WriteLine($"Current User ID: {userId}");
            // Hämta alla produkt-ID:n från användarens önskelista
            var wishlistItems = await _context.WishLists
                .Where(w => w.UserId == userId)
                .Select(w => w.ProductId)
                .ToListAsync();

            if (!wishlistItems.Any())
            {
                return Ok(new List<object>()); // Returnera en tom lista om ingen produkt finns i önskelistan
            }

            // Hämta produktinformation baserat på produkt-ID:n
            var products = await _context.Products
                .Where(p => wishlistItems.Contains(p.Id))
                .Select(p => new 
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    EncodedImage = p.EncodedImage,
                    SubCategoryId = p.SubCategoryId,
                    StockQuantity = p.StockQuantity
                })
                .ToListAsync();

            return Ok(products);
        }

        // Hämta nuvarande användarens ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // Om det inte finns något användar-ID i token, returnera ett lämpligt fel
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            int userId = int.Parse(userIdClaim.Value);
            Console.WriteLine($"Fetched User ID: {userId}"); // För debug, kan tas bort när det är klart
            return userId;
        }


    }
}
