using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop_API.Data;
using OnlineShop_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OnlineShop_API.Dto;
using OnlineShop_API.DTOs;
using OnlineShop_API.Identity;
using Microsoft.AspNetCore.Authorization;
using Stripe.Climate;

namespace OnlineShop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly OnlineShopDbContext _context;
        private readonly IConfiguration _config;

        public UsersController(OnlineShopDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new Users
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Password = HashPassword(registerDto.Password), // Hasha lösenordet
                Address = registerDto.Address,
                Mobile = registerDto.Mobile,
                City = registerDto.City,
                Zipcode = registerDto.Zipcode,
                IsAdmin = false // Se till att användaren inte kan sätta sig själv som admin
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }

        // POST: api/users/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == loginDto.Email);

            // Kontrollera att användaren existerar och lösenordet stämmer
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            if (!VerifyPassword(user.Password, loginDto.Password))
            {
                return Unauthorized("Invalid password.");
            }

            // Generera JWT-token för användaren
            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        // POST: api/users/logout
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // Eftersom JWT inte lagras server-side, görs ingenting här
            return Ok("User logged out successfully.");
        }

        // GET: api/users
        [HttpGet]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync(); // Hämtar alla användare från databasen
            return Ok(users); // Returnerar användarna som JSON
        }
        // GET: api/users/profile
        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetUserProfile()
        {
            // Hämta användarens ID från JWT-tokenet
            var userId = GetCurrentUserId();

            // Hämta användaren från databasen baserat på användarens ID
            var user = _context.Users
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Ta bort lösenordet från användarobjektet för att inte exponera det
            user.Password = null;

            return Ok(user); // Returnera hela användarobjektet utan lösenord
        }


        [HttpPut("profile")]
        [Authorize]
        public IActionResult UpdateProfile([FromBody] UserDto updatedUser)
        {
            if (updatedUser == null)
            {
                return BadRequest("User data is required."); // Returnera 400 om ingen data skickas
            }

            // Hämta användarens ID från claims
            var userId = GetCurrentUserId();

            // Hämta användaren med det angivna ID:t
            var user = _context.Users.Find(userId);

            if (user == null)
            {
                return NotFound(); // Returnera 404 om användaren inte hittas
            }

            // Uppdatera användarens information utan att ändra lösenordet
            user.Name = updatedUser.Name;
            user.Address = updatedUser.Address;
            user.Mobile = updatedUser.Mobile;
            user.City = updatedUser.City;
            user.Zipcode = updatedUser.Zipcode;

            _context.SaveChanges(); // Spara ändringarna i databasen
            return Ok("Profile updated successfully."); // Returnera framgångsmeddelande
        }
        [HttpPut("change-password")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            // Kontrollera att DTO:n inte är null
            if (changePasswordDto == null)
            {
                return BadRequest("Change password data is required.");
            }

            // Hämta användarens ID från claims
            var userId = GetCurrentUserId();

            // Hämta användaren med det angivna ID:t
            var user = _context.Users.Find(userId);

            if (user == null)
            {
                return NotFound("User not found."); // Returnera 404 om användaren inte hittas
            }

            // Kontrollera att det gamla lösenordet är korrekt
            if (!VerifyPassword(user.Password, changePasswordDto.OldPassword))
            {
                return BadRequest("Old password is incorrect."); // Returnera 400 om det gamla lösenordet är felaktigt
            }

            // Hasha det nya lösenordet
            user.Password = HashPassword(changePasswordDto.NewPassword);
            _context.SaveChanges(); // Spara ändringarna i databasen

            return Ok("Password changed successfully."); // Returnera framgångsmeddelande
        }

        [HttpGet("orders")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrderCreatedDto>>> GetUserOrders()
        {
            // Hämta användarens ID från JWT-tokenet
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Användar-ID saknas i token.");
            }

            // Hämta ordrar för den specifika användaren
            var orders = await _context.Orders
                .Where(o => o.UserId == userId && o.Status != Status.Pending && o.Status != Status.Canceled)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Hämtar även produktinformation
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound("Inga ordrar hittades för denna användare.");
            }

            // Konvertera till DTO för att returnera en ren JSON-struktur
            var orderDtos = orders.Select(o => new OrderCreatedDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Product?.Price ?? 0, // Om produkten finns, hämta priset
                    ProductName = oi.Product?.Name ?? "Okänd produkt", // Hämtar produktens namn
                    EncodedImage = oi.Product?.EncodedImage // Lägg till bildens URL
                }).ToList()
            }).ToList();

            return Ok(orderDtos);
        }






        // Helper methods
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool VerifyPassword(string hashedPassword, string password)
        {
            var hashedInputPassword = HashPassword(password);
            return hashedPassword == hashedInputPassword;
        }

        private string GenerateJwtToken(Users user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Användarens ID
        new Claim(ClaimTypes.Email, user.Email) // Användarens e-post
    };

            // Lägg till ett claim för admin om användaren är admin
            if (user.IsAdmin)
            {
                claims.Add(new Claim(IdentityData.AdminUserClaimName, "true"));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}
