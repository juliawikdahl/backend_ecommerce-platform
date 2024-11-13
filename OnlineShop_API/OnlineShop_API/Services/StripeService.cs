using Microsoft.Extensions.Options;
using OnlineShop_API.data;
using Stripe;
using Stripe.Checkout;

public class StripeService
{
    private readonly string _secretKey;

    public StripeService(IOptions<StripeSettings> options)
    {
        _secretKey = options.Value.SecretKey;
        StripeConfiguration.ApiKey = _secretKey;
    }

    public PaymentIntent CreatePaymentIntent(decimal amount)
    {
        var paymentIntentCreateOptions = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Omvandla till cent
            Currency = "usd", // Sätt rätt valuta
            ReceiptEmail = "customer@example.com" // Skicka kvitto till kundens e-post (ändra efter behov)
        };

        var paymentIntentService = new PaymentIntentService();
        PaymentIntent paymentIntent = paymentIntentService.Create(paymentIntentCreateOptions);

        return paymentIntent;
    }

    public string CreateCheckoutSession(decimal amount, string successUrl, string cancelUrl)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(amount * 100), // Omvandla till cent
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Order Payment"
                            },
                        },
                        Quantity = 1,
                    },
                },
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
        };

        var service = new SessionService();
        Session session = service.Create(options);

        return session.Id;
    }
}
