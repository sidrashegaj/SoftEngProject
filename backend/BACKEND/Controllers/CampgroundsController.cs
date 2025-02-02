using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BACKEND.Data;
using BACKEND.Models;
using BACKEND.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BACKEND.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampgroundsController : ControllerBase
    {
        private readonly AlbCampDbContext _context;
        private readonly PhotoService _photoService;

        public CampgroundsController(AlbCampDbContext context, PhotoService photoService)
        {
            _context = context;
            _photoService = photoService;
        }

        // GET: api/Campgrounds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Campground>>> GetCampgrounds([FromQuery] string? location = null)
        {
            var query = _context.Campgrounds
                                .Include(c => c.Images)
                                .Include(c => c.Author)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(c => c.Location.Contains(location));
            }

            var campgrounds = await query.ToListAsync();
            return Ok(campgrounds);
        }

        // GET: api/Campgrounds/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Campground>> GetCampground(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid ID provided.");

            var campground = await _context.Campgrounds
                                           .Include(c => c.Images)
                                           .Include(c => c.Reviews)
                                           .ThenInclude(r => r.User)
                                           .Include(c => c.Author)
                                           .AsSplitQuery()
                                           .FirstOrDefaultAsync(c => c.CampgroundId == id);

            if (campground == null)
                return NotFound("Cannot find that campground!");

            return Ok(campground);
        }

        // POST: api/Campgrounds
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCampground(
            [FromForm] IFormFile[] images,
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] string location,
            [FromForm] double latitude,
            [FromForm] double longitude,
            [FromForm] int price)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null)
                {
                    return Unauthorized("User is not authenticated.");
                }

                var userId = int.Parse(userIdClaim.Value);
                var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
                if (!userExists)
                {
                    return BadRequest("Invalid UserId. User does not exist.");
                }

                var campground = new Campground
                {
                    Name = name,
                    Location = location,
                    Description = description,
                    Latitude = latitude,
                    Longitude = longitude,
                    Price = price,
                    UserId = userId,
                    Images = new List<Image>()
                };

                foreach (var image in images)
                {
                    var uploadResult = await _photoService.UploadImageAsync(image);
                    if (uploadResult.Error != null)
                    {
                        return BadRequest($"Image upload error: {uploadResult.Error.Message}");
                    }

                    campground.Images.Add(new Image
                    {
                        Url = uploadResult.SecureUrl.ToString(),
                        Filename = uploadResult.PublicId
                    });
                }

                _context.Campgrounds.Add(campground);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCampground), new { id = campground.CampgroundId }, campground);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCampground(int id, [FromForm] UpdateCampgroundDto updateDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId").Value);

                var campground = await _context.Campgrounds
                                               .Include(c => c.Images) // Include existing images
                                               .FirstOrDefaultAsync(c => c.CampgroundId == id);

                if (campground == null)
                {
                    return NotFound("Campground not found.");
                }

                if (campground.UserId != userId)
                {
                    return Unauthorized("You can only update campgrounds you created.");
                }

                // Update basic details
                campground.Name = updateDto.Name;
                campground.Location = updateDto.Location;
                campground.Description = updateDto.Description;
                campground.Price = updateDto.Price;
                campground.Latitude = updateDto.Latitude;
                campground.Longitude = updateDto.Longitude;

                // Handle image deletion
                if (updateDto.DeleteImages != null && updateDto.DeleteImages.Any())
                {
                    foreach (var filename in updateDto.DeleteImages)
                    {
                        // Remove from Cloudinary (or your image service)
                        await _photoService.DeleteImageAsync(filename);

                        // Remove from the database
                        campground.Images.RemoveAll(img => img.Filename == filename);
                    }
                }

                // Handle new image uploads
                if (updateDto.Images != null && updateDto.Images.Count > 0)
                {
                    foreach (var image in updateDto.Images)
                    {
                        var uploadResult = await _photoService.UploadImageAsync(image);
                        if (uploadResult.Error != null)
                        {
                            return BadRequest(uploadResult.Error.Message);
                        }

                        campground.Images.Add(new Image
                        {
                            Url = uploadResult.SecureUrl.ToString(),
                            Filename = uploadResult.PublicId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Successfully updated campground" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        // DELETE: api/Campgrounds/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCampground(int id)
        {
            try
            {
                // Extract UserId from JWT
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null)
                {
                    return Unauthorized("User is not authenticated.");
                }

                var userId = int.Parse(userIdClaim.Value);
                Console.WriteLine($"Logged-in User ID: {userId}");

                // Fetch campground
                var campground = await _context.Campgrounds.FindAsync(id);
                if (campground == null)
                {
                    Console.WriteLine("Campground not found.");
                    return NotFound("Campground not found.");
                }

                Console.WriteLine($"Campground User ID: {campground.UserId}");
                if (campground.UserId != userId)
                {
                    Console.WriteLine("Unauthorized attempt to delete campground.");
                    return Unauthorized("You can only delete campgrounds you created.");
                }

                // Delete campground
                _context.Campgrounds.Remove(campground);
                await _context.SaveChangesAsync();

                Console.WriteLine("Campground deleted successfully.");
                return Ok(new { message = "Campground deleted successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the campground.");
            }
        }


        // GET: api/Campgrounds/search
        [HttpGet("search")]
        public IActionResult SearchCampgrounds([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return BadRequest("Search query cannot be empty.");
            }

            var campgrounds = _context.Campgrounds
                .Where(c => c.Name.Contains(q) || c.Location.Contains(q))
                .ToList();

            return Ok(campgrounds);
        }

        [HttpGet("nearby")]
        public async Task<IActionResult> GetCampgroundsNearby(
            [FromQuery] double userLat,
            [FromQuery] double userLng,
            [FromQuery] double radiusInKm = 50)
        {
            var campgrounds = await _context.Campgrounds.ToListAsync();

            var nearbyCampgrounds = campgrounds.Where(c =>
            {
                var distance = CalculateDistance(userLat, userLng, c.Latitude, c.Longitude);
                return distance <= radiusInKm;
            });

            return Ok(nearbyCampgrounds);
        }

        private double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
        {
            const double EarthRadiusKm = 6371;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLng = DegreesToRadians(lng2 - lng1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        private double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
    }
}
