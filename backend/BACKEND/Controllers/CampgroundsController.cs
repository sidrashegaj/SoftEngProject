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
        private readonly GeocodingService _geocodingService;

        public CampgroundsController(AlbCampDbContext context, PhotoService photoService, GeocodingService geocodingService)
        {
            _context = context;
            _photoService = photoService;
            _geocodingService = geocodingService;
        }

        // GET: api/Campgrounds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Campground>>> GetCampgrounds([FromQuery] string location)
        {
            var query = _context.Campgrounds
                                .Include(c => c.Images)
                                .Include(c => c.Author)
                                .AsQueryable(); // Ensures the type remains IQueryable

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(c => c.Location.Contains(location));
            }

            var campgrounds = await query.ToListAsync(); // Convert to ListAsync to execute the query
            return Ok(campgrounds);
        }

        [HttpGet("test-photo-service")]
        public IActionResult TestPhotoService([FromServices] PhotoService photoService)
        {
            return Ok("PhotoService configured correctly.");
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
        public async Task<IActionResult> CreateCampground([FromForm] IFormFile[] images, [FromForm] string name, [FromForm] string description, [FromForm] string location, [FromForm] int price)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);

            var geoData = await _geocodingService.GetCoordinatesAsync(location);
            if (string.IsNullOrEmpty(geoData))
                return BadRequest("Unable to find geolocation data.");

            var coordinates = geoData.Split(',');
            if (coordinates.Length != 2 || !double.TryParse(coordinates[0], out double longitude) || !double.TryParse(coordinates[1], out double latitude))
                return BadRequest("Invalid geolocation data format.");

            var campground = new Campground
            {
                Name = name,
                Location = location,
                Description = description,
                Price = price,
                UserId = userId,
                Geometry = new BACKEND.Models.Geometry
                {
                    Type = "Point",
                    Coordinates = new double[] { longitude, latitude }
                },
                Images = new List<Image>()
            };

            foreach (var image in images)
            {
                var uploadResult = await _photoService.UploadImageAsync(image);
                if (uploadResult.Error != null)
                    return BadRequest(uploadResult.Error.Message);

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
    }
}
