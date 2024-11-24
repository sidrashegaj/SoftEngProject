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
        public async Task<IActionResult> PostReview(int campgroundId, [FromBody] ReviewDto reviewDto)
        {
            var user = await _context.Users.FindAsync(reviewDto.UserId);
            var campground = await _context.Campgrounds.FindAsync(campgroundId);

            if (user == null || campground == null)
                return BadRequest("Invalid user or campground.");

            var review = new Review
            {
                Text = reviewDto.Text,
                Rating = reviewDto.Rating,
                UserId = user.UserId,
                CampgroundId = campground.CampgroundId
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetReviewsForCampground", new { campgroundId }, review);
        }
    }
}
