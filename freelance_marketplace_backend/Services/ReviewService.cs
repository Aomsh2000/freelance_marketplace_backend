using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace freelance_marketplace_backend.Services
{
    public class ReviewService : IReviewService
    {
        private readonly FreelancingPlatformContext _context;

        public ReviewService(FreelancingPlatformContext context)
        {
            _context = context;
        }

        public async Task<Review> SubmitReviewAsync(ReviewDto reviewDto, string reviewerId)
        {
            if (string.IsNullOrEmpty(reviewerId))
                throw new UnauthorizedAccessException("User not authenticated.");

            var review = new Review
            {
                ProjectId = reviewDto.ProjectId,
                FromUsersid = reviewerId,
                ToUsersid = reviewDto.ToUsersid,
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<List<Review>> GetReviewsForFreelancerAsync(string freelancerId)
        {
            return await _context
                .Reviews.Where(r => r.ToUsersid == freelancerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, string reviewerId)
        {
            if (string.IsNullOrEmpty(reviewerId))
                throw new UnauthorizedAccessException("User not authenticated.");

            var review = await _context.Reviews.FirstOrDefaultAsync(r =>
                r.ReviewId == reviewId && r.FromUsersid == reviewerId
            );

            if (review == null)
                return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
