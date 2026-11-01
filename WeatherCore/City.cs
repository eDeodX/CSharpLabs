using System.Globalization;
namespace WeatherCore
{
    public class City
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public override string ToString() { return Name; }
        public static List<City> Load(string path)
        {
            List<City> result = new List<City>();
            foreach (string line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split('\t');
                if (parts.Length != 2) throw new FormatException("Неверная строка города.");
                string[] coordinates = parts[1].Split(',');
                if (coordinates.Length != 2) throw new FormatException("Неверные координаты.");
                City city = new City();
                city.Name = parts[0].Trim();
                city.Latitude = double.Parse(coordinates[0], CultureInfo.InvariantCulture);
                city.Longitude = double.Parse(coordinates[1], CultureInfo.InvariantCulture);
                if (city.Latitude < -90 || city.Latitude > 90
                    || city.Longitude < -180 || city.Longitude > 180)
                    throw new FormatException("Координаты вне диапазона.");
                result.Add(city);
            }
            return result;
        }
    }
}
