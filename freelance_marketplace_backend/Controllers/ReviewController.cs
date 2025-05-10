using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // Ensure that the user is authenticated for these actions
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // POST: api/reviews/submit
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitReview([FromBody] ReviewDto reviewDto)
        {
            if (string.IsNullOrWhiteSpace(reviewDto.Comment)) 
                return BadRequest("Comment is required.");
            
            if (reviewDto.Rating < 1 || reviewDto.Rating > 5) 
                return BadRequest("Rating must be between 1 and 5.");

            var reviewerId = User.FindFirst("user_id")?.Value;  // Get the user_id from the token
            if (string.IsNullOrEmpty(reviewerId))
                return Unauthorized("User not authenticated.");

            var result = await _reviewService.SubmitReviewAsync(reviewDto, reviewerId);
            return Ok(result);
        }

        // DELETE: api/reviews/{reviewId}
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var reviewerId = User.FindFirst("user_id")?.Value;  // Get the user_id from the token
            if (string.IsNullOrEmpty(reviewerId))
                return Unauthorized("User not authenticated.");

            var success = await _reviewService.DeleteReviewAsync(reviewId, reviewerId);
            if (!success)
                return Forbid("You can only delete your own review.");

            return Ok("Review deleted successfully.");
        }
    }
}
