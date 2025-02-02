using BACKEND.Models;
using Microsoft.EntityFrameworkCore;

namespace BACKEND.Data
{
    public class DbInitializer
    {
        public static void Initialize(AlbCampDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Add a default user if not exists
            if (!context.Users.Any())
            {
                var user = new User
                {
                    Username = "Default User",
                    Email = "user@example.com",
                    Password = "DefaultPassword123!"
                };
                context.Users.Add(user);
                context.SaveChanges();
            }

            // Remove existing campgrounds if necessary
            if (context.Campgrounds.Any())
            {
                return; // DB already seeded
            }

            // Seed campgrounds
            var userId = context.Users.First().UserId;
            Random rnd = new Random();
            var random1000 = CitiesSeeder.Cities.Count;

            for (int i = 0; i < 300; i++)
            {
                var randomCity = CitiesSeeder.Cities[rnd.Next(0, random1000)];
                var price = rnd.Next(10, 30);

                var camp = new Campground
                {
                    UserId = userId,
                    Location = $"{randomCity.CityName}, {randomCity.State}",
                    Name = $"{GetRandomItem(SeedHelper.Descriptors)} {GetRandomItem(SeedHelper.Places)}",
                    Description = GetRandomDescription(),
                    Price = price,
                    Latitude = randomCity.Latitude,
                    Longitude = randomCity.Longitude,
                    Images = new List<Image>
                    {
                        new Image { Url = $"https://picsum.photos/400?random={rnd.Next(1000)}", Filename = "placeholder" }
                    }
                };

                context.Campgrounds.Add(camp);
            }

            context.SaveChanges();
        }

        private static string GetRandomItem(List<string> items)
        {
            Random rnd = new Random();
            return items[rnd.Next(items.Count)];
        }

        private static string GetRandomDescription()
        {
            var descriptions = new List<string>
            {
                "A serene place to relax and enjoy nature.",
                "Perfect for family outings and weekend getaways.",
                "Experience the beauty of the outdoors.",
                "A peaceful retreat in the heart of nature.",
                "Ideal for adventurers and nature lovers."
            };
            Random rnd = new Random();
            return descriptions[rnd.Next(descriptions.Count)];
        }
    }
}
