namespace BACKEND.Data
{
    public static class CitiesSeeder
    {
        public static List<City> Cities = new List<City>
        {
            new City
            {
                CityName = "Tirana",
                GrowthFrom2000To2013 = "4.2%",
                Latitude = 41.3275,
                Longitude = 19.8189,
                Population = "418495",
                Rank = "1",
                State = "Tirana"
            },
            new City
            {
                CityName = "Durrës",
                GrowthFrom2000To2013 = "3.5%",
                Latitude = 41.3231,
                Longitude = 19.4411,
                Population = "201513",
                Rank = "2",
                State = "Durrës"
            },
            new City
            {
                CityName = "Vlorë",
                GrowthFrom2000To2013 = "2.8%",
                Latitude = 40.4661,
                Longitude = 19.4914,
                Population = "183105",
                Rank = "3",
                State = "Vlorë"
            },
            new City
            {
                CityName = "Shkodër",
                GrowthFrom2000To2013 = "1.9%",
                Latitude = 42.0683,
                Longitude = 19.5126,
                Population = "135612",
                Rank = "4",
                State = "Shkodër"
            },
            new City
            {
                CityName = "Berat",
                GrowthFrom2000To2013 = "2.1%",
                Latitude = 40.7058,
                Longitude = 19.9528,
                Population = "64661",
                Rank = "5",
                State = "Berat"
            }
        };
    }

    public class City
    {
        public string CityName { get; set; }
        public string GrowthFrom2000To2013 { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Population { get; set; }
        public string Rank { get; set; }
        public string State { get; set; }
    }
}
