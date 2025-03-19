using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop_API.Data;
using OnlineShop_API.Dto;
using OnlineShop_API.Identity;
using OnlineShop_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineShop_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OnlineShopDbContext _context;

        public OrdersController(OnlineShopDbContext context)
        {
            _context = context;
        }

        // POST: api/orders
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<OrderCreatedDto>> CreateOrder([FromBody] OrderCreateDto orderCreateDto)
        {
            if (orderCreateDto == null || orderCreateDto.OrderItems == null || !orderCreateDto.OrderItems.Any())
            {
                return BadRequest("Order items cannot be empty.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("User not found.");
            }

            var totalAmount = 0m;
            var orderItems = new List<OrderItems>();

            foreach (var item in orderCreateDto.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || product.StockQuantity < item.Quantity)
                {
                    return BadRequest($"Product with ID {item.ProductId} not found or insufficient stock.");
                }

                var orderItem = new OrderItems
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                orderItems.Add(orderItem);
                totalAmount += product.Price * item.Quantity;
            }

            var order = new Orders
            {
                UserId = userId,
                OrderItems = orderItems,
                Status = Status.Pending, // Statusen är Pending initialt
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Skicka tillbaka orderId tillsammans med andra relevanta data
            return Ok(new
            {
                Message = "Order created successfully. Please complete the payment to confirm the order.",
                OrderId = order.Id // Lägg till orderId i svaret
            });
        }


        // Kontrollera om ordern ska avbrytas (om den är äldre än 24 timmar)
        private async Task CancelExpiredOrders()
        {
            var expiredOrders = await _context.Orders
                .Where(o => o.Status == Status.Pending &&
                            EF.Functions.DateDiffHour(o.OrderDate, DateTime.UtcNow) > 24)  // Använd EF.Functions.DateDiffHour
                .ToListAsync();

            foreach (var order in expiredOrders)
            {
                order.Status = Status.Canceled;
            }

            await _context.SaveChangesAsync();
        }


        // GET: api/orders
        [HttpGet]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)] // Endast administratörer kan hämta alla ordrar
        public async Task<ActionResult<IEnumerable<OrderCreatedDto>>> GetOrders()
        {
            // Kontrollera och avbryt förfallna ordrar innan vi hämtar dem
            await CancelExpiredOrders();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderCreatedDto
            {
                OrderId = o.Id,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity
                }).ToList(),
                Status = o.Status.ToString(),
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount
            }).ToList();

            return Ok(orderDtos);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)] // Endast administratörer kan hämta specifik order
        public async Task<ActionResult<OrderCreatedDto>> GetOrder(int id)
        {
            // Kontrollera och avbryt förfallna ordrar innan vi hämtar den specifika
            await CancelExpiredOrders();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var orderDto = new OrderCreatedDto
            {
                OrderId = order.Id,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity
                }).ToList(),
                Status = order.Status.ToString(),
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount
            };

            return Ok(orderDto);
        }

        // PUT: api/orders/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateDto)
        {
            if (updateDto == null || string.IsNullOrWhiteSpace(updateDto.Status))
            {
                return BadRequest("Status is required.");
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            if (!Enum.TryParse(typeof(Status), updateDto.Status, true, out var newStatus))
            {
                return BadRequest("Invalid status value.");
            }

            order.Status = (Status)newStatus;
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // PUT: api/orders/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)] // Endast administratörer kan uppdatera order
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderCreateDto orderUpdateDto)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            order.OrderItems.Clear();
            foreach (var item in orderUpdateDto.OrderItems)
            {
                var newItem = new OrderItems
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    OrderId = order.Id

                };
                order.OrderItems.Add(newItem);
            }

            order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * _context.Products.First(p => p.Id == oi.ProductId).Price);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)] // Endast administratörer kan ta bort order
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
