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

        public ReviewsController(AlbCampDbContext context)
        {
            _context = context;
        }

        // GET: api/Reviews/campground/5
        [HttpGet("campground/{campgroundId}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsForCampground(int campgroundId)
        {
            var reviews = await _context.Reviews
                                         .Where(r => r.CampgroundId == campgroundId)
                                         .Include(r => r.User)
                                         .ToListAsync();
            return Ok(reviews);
        }

        // POST: api/Reviews/campground/5
        [HttpPost("campground/{campgroundId}")]
        [Authorize]
        public async Task<IActionResult> PostReview(int campgroundId, [FromBody] ReviewDto reviewDto)
        {
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
            return CreatedAtAction("GetReviewsForCampground", new { campgroundId }, review);
        }


        // DELETE: api/Reviews/5
        [HttpDelete("{reviewId}")]
        [Authorize] // Ensure only logged-in users can delete reviews
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return NotFound("Review not found.");
            }

            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            // Ensure user owns the review before deleting
            if (review.UserId != userId)
            {
                return Unauthorized("You can only delete your own reviews.");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review deleted successfully." });
        }
    }
}
