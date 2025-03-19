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
using OnlineShop_API.Dto;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        // Skapa betalning (Stripe eller fejkad betalning baserat på useFakePayment)
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentDto paymentDto, [FromQuery] bool useFakePayment = false)
        {
            if (paymentDto == null || paymentDto.OrderId == 0)
            {
                return BadRequest("Invalid payment data.");
            }

            try
            {
                // Hämta ordern från databasen och kontrollera användarens ID
                var order = await _context.Orders
                    .Where(o => o.Id == paymentDto.OrderId && o.Status == Status.Pending)
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound("Order not found or already processed.");
                }

                // Kontrollera om den aktuella användaren är den som har gjort beställningen
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId) || userId != order.UserId)
                {
                    return Unauthorized("You are not authorized to make this payment.");
                }

                // Kolla om Stripe är aktiverat via appsettings.json
                var stripeEnabled = _configuration.GetValue<bool>("Stripe:Enabled");

                if (useFakePayment)
                {
                    // Om fejkbetalning är aktiverat
                    return Ok(new { clientSecret = "fake_payment_client_secret" }); // Fejkad betalning
                }

                // Om Stripe är aktiverat och fejkbetalning inte används
                if (stripeEnabled)
                {
                    var stripeSecretKey = _configuration["Stripe:SecretKey"];
                    StripeConfiguration.ApiKey = stripeSecretKey;

                    // Omvandla belopp till cent (öre)
                    long amount = (long)(order.TotalAmount * 100);

                    // Skapa PaymentIntent via Stripe
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

                return BadRequest("Stripe is not enabled.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating payment intent: {ex.Message}");
            }
        }

        // Skapa en fejkad betalning (utan Stripe)
        [HttpPost("fake/create")]
        [Authorize]
        public IActionResult CreateFakePayment([FromBody] PaymentDto paymentDto)
        {
            if (paymentDto == null || paymentDto.OrderId == 0)
            {
                return BadRequest("Invalid payment data.");
            }

            try
            {
                // Här kan du simulera logiken för fejkad betalning
                var fakeClientSecret = "fake_payment_client_secret";

                // Simulera betalningen utan att använda Stripe
                return Ok(new { clientSecret = fakeClientSecret });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating fake payment: {ex.Message}");
            }
        }

        // Bekräfta ordern och uppdatera statusen när betalningen är slutförd
        [HttpPost("{orderId}/confirm")]
        [Authorize]
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
