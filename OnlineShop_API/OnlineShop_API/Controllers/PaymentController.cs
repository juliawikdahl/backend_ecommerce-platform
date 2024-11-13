using Microsoft.AspNetCore.Mvc;
using Stripe;
using Microsoft.Extensions.Configuration;
using OnlineShop_API.Data;
using OnlineShop_API.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using OnlineShop_API.Dto;
using Microsoft.AspNetCore.Authorization;

namespace OnlineShop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly OnlineShopDbContext _context;

        public PaymentController(IConfiguration configuration, OnlineShopDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // Skapa betalning och returnera clientSecret
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentDto paymentDto)
        {
            if (paymentDto == null || paymentDto.OrderId == 0)
            {
                return BadRequest("Invalid payment data.");
            }

            try
            {
                var stripeSecretKey = _configuration["Stripe:SecretKey"];
                StripeConfiguration.ApiKey = stripeSecretKey;

                // Hämta ordern från databasen
                var order = await _context.Orders
                    .Where(o => o.Id == paymentDto.OrderId && o.Status == Status.Pending)
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound("Order not found or already processed.");
                }

                // Omvandla belopp till cent (öre)
                long amount = (long)(order.TotalAmount * 100);

                // Skapa PaymentIntent
                var paymentIntentService = new PaymentIntentService();
                var paymentIntentCreateOptions = new PaymentIntentCreateOptions
                {
                    Amount = amount,
                    Currency = "sek",
                    PaymentMethodTypes = new List<string> { paymentDto.PaymentMethod },
                    Description = $"Payment for Order {order.Id}"
                };

                var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentCreateOptions);

                return Ok(new { clientSecret = paymentIntent.ClientSecret });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating payment intent: {ex.Message}");
            }
        }

        // Bekräfta ordern och uppdatera statusen när betalningen är slutförd
        [HttpPost("{orderId}/confirm")]
        public async Task<IActionResult> ConfirmOrder(int orderId, [FromBody] PaymentConfirmationDto confirmationDto)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);

                if (order == null)
                {
                    return NotFound("Order not found.");
                }

                // Här kollar vi om betalningen är slutförd och om statusen är 'succeeded' från frontend
                if (confirmationDto.PaymentStatus == "succeeded" && order.Status == Status.Pending)
                {
                    // Uppdatera orderstatus till 'Shipped' (eller 'Confirmed' om så önskas)
                    order.Status = Status.Shipped;
                    await _context.SaveChangesAsync();

                    // Bekräfta betalning till användaren (kan vara e-post eller annan åtgärd)
                    return Ok(new { message = "Order confirmed and status updated to Shipped." });
                }

                return BadRequest("Payment failed or order already processed.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error confirming order: {ex.Message}");
            }
        }
    }
}
