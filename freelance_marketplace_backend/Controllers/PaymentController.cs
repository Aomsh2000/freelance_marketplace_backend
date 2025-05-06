using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        [HttpPost("create-checkout-session")]
        public IActionResult Create([FromBody] AmountRequest req)
        {
            // Get user ID from token
            var uid = User.FindFirst("user_id")?.Value;

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
                SuccessUrl = $"http://localhost:4200/home?success=true&amount={req.Amount}",
                CancelUrl = "http://localhost:4200/home?canceled=true"};
            var service = new SessionService();
            var session = service.Create(options);
            return Ok(new { sessionId = session.Id });
        }
    }
}