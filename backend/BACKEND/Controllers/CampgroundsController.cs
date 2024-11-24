using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BACKEND.Data;
using BACKEND.Models;

namespace BACKEND.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampgroundsController : ControllerBase
    {
        private readonly AlbCampDbContext _context;

        public CampgroundsController(AlbCampDbContext context)
        {
            _context = context;
        }

        // GET: api/Campgrounds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Campground>>> GetCampgrounds()
        {
            var campgrounds = await _context.Campgrounds
                                            .Include(c => c.Author)
                                            .ToListAsync();
            return Ok(campgrounds);
        }

        // GET: api/Campgrounds/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Campground>> GetCampground(int id)
        {
            var campground = await _context.Campgrounds
                                           .Include(c => c.Author)
                                           .Include(c => c.Reviews)
                                           .FirstOrDefaultAsync(c => c.CampgroundId == id);
            if (campground == null)
            {
                return NotFound("Cannot find that campground!");
            }
            return Ok(campground);
        }

        // POST: api/Campgrounds
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCampground([FromBody] CampgroundDto createDto)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var campground = new Campground
            {
                Name = createDto.Name,
                Location = createDto.Location,
                Description = createDto.Description,
                Price = createDto.Price,
                UserId = userId,
                Geometry = new Geometry
                {
                    Type = "Point",
                    Coordinates = new double[] { 20.0, 41.0 } 
                }
            };

            _context.Campgrounds.Add(campground);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCampground), new { id = campground.CampgroundId }, campground);
        }

        // PUT: api/Campgrounds/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCampground(int id, [FromBody] UpdateCampgroundDto updateDto)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var campground = await _context.Campgrounds.FindAsync(id);

            if (campground == null)
                return NotFound("Campground not found.");

            if (campground.UserId != userId)
                return Unauthorized("You can only update your campgrounds.");

            campground.Name = updateDto.Name;
            campground.Location = updateDto.Location;
            campground.Description = updateDto.Description;
            campground.Price = updateDto.Price;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Successfully updated campground." });
        }

        // DELETE: api/Campgrounds/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCampground(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var campground = await _context.Campgrounds.FindAsync(id);

            if (campground == null)
                return NotFound("Campground not found.");

            if (campground.UserId != userId)
                return Unauthorized("You can only delete your campgrounds.");

            _context.Campgrounds.Remove(campground);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Campground deleted successfully." });
        }
    }
}
