using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BACKEND.Data;
using BACKEND.Models;

namespace BACKEND.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly AlbCampDbContext _context;

        // Constructor
        public ReviewsController(AlbCampDbContext context)
        {
            _context = context;
        }

        // GET: api/reviews/campground/{campgroundId}
        [HttpGet("campground/{campgroundId}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsForCampground(int campgroundId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.CampgroundId == campgroundId)
                .Include(r => r.User) // Include the related user
                .Select(r => new
                {
                    r.ReviewId,
                    r.Text,
                    r.Timestamp,
                    r.Rating,
                    UserId = r.UserId, // Ensure UserId is part of the response
                    Username = r.User.Username // Optional: Include username
                })
                .ToListAsync();

            if (reviews == null || !reviews.Any())
            {
                return NotFound("No reviews found for this campground.");
            }

            return Ok(reviews);
        }

        // POST: api/reviews/campground/{campgroundId}
        [HttpPost("campground/{campgroundId}")]
        [Authorize]
        public async Task<IActionResult> PostReview(int campgroundId, [FromBody] ReviewDto reviewDto)
        {
            Console.WriteLine($"Received Review for Campground ID: {campgroundId}");

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var campground = await _context.Campgrounds.FindAsync(campgroundId);
            if (campground == null)
            {
                return NotFound("Campground not found.");
            }

            var review = new Review
            {
                Text = reviewDto.Text,
                Rating = reviewDto.Rating,
                UserId = user.UserId,
                CampgroundId = campground.CampgroundId,
                Timestamp = DateTime.UtcNow,
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Fetch the review with the User information
            var savedReview = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewId == review.ReviewId);

            return CreatedAtAction(nameof(GetReviewsForCampground), new { campgroundId }, new
            {
                savedReview.ReviewId,
                savedReview.Text,
                savedReview.Rating,
                savedReview.Timestamp,
                User = new
                {
                    savedReview.User.UserId,
                    savedReview.User.Username
                }
            });
        }

        // DELETE: api/reviews/{reviewId}
        [HttpDelete("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            // Fetch the review
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return NotFound("Review not found.");
            }

            // Extract UserId from token
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                Console.WriteLine("UserId claim not found in token.");
                return Unauthorized("User not authenticated.");
            }

            var userId = int.Parse(userIdClaim.Value);
            Console.WriteLine($"Review UserId: {review.UserId}, Token UserId: {userId}");

            // Check ownership
            if (review.UserId != userId)
            {
                return Unauthorized("You can only delete your own reviews.");
            }

            // Delete the review
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review deleted successfully." });
        }

    }
}
