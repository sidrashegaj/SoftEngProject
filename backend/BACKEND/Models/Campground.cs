using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BACKEND.Models
{
    public class Campground
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CampgroundId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        public string Description { get; set; }
        public int Price { get; set; }

        public Geometry Geometry { get; set; }

        public List<Image> Images { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User Author { get; set; }


        // Navigation property
        public ICollection<Review> Reviews { get; set; }
    }


    public class CampgroundDto
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
    }
    public class UpdateCampgroundDto
    {
        public int CampgroundId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }

        public List<IFormFile> Images { get; set; }
        public List<string> DeleteImages { get; set; }
    }




    public class GeoData
    {
        public List<Feature> Features { get; set; }
    }

    public class Feature
    {
        public Geometry Geometry { get; set; }
    }

    public class Geometry
    {

        public string Type { get; set; }
        public double[] Coordinates { get; set; }

    }



}