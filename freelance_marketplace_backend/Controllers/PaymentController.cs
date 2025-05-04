using freelance_marketplace_backend.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        [HttpPost("create-checkout-session")]
        public IActionResult Create([FromBody] AmountRequest req)
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
            Currency   = "usd",
            UnitAmount = req.Amount,
            ProductData = new SessionLineItemPriceDataProductDataOptions
            { Name = "Wallet Top-Up" }
          },
          Quantity = 1
        }
      },
                Mode = "payment",
                SuccessUrl = "http://localhost:4200/payment?success=true&amount={CHECKOUT_AMOUNT}",
                CancelUrl = "http://localhost:4200/payment?canceled=true"
            };
            var service = new SessionService();
            var session = service.Create(options);
            return Ok(new { sessionId = session.Id });
        }
    }
}