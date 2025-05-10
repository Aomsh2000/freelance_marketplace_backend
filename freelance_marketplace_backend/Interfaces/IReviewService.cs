using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Interfaces
{
    public interface IReviewService
    {
        // Submits a review by the user
        Task<Review> SubmitReviewAsync(ReviewDto dto, string reviewerId);

        // Fetches all reviews for a specific freelancer
        Task<List<Review>> GetReviewsForFreelancerAsync(string freelancerId);

        // Deletes a review by the reviewer
        Task<bool> DeleteReviewAsync(int reviewId, string reviewerId);
    }
}

