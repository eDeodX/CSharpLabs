using System.Globalization;
using System.Text.Json;
namespace WeatherCore
{
    public class WeatherClient : IDisposable
    {
        private HttpClient client = new HttpClient();
        public WeatherClient() { client.Timeout = TimeSpan.FromSeconds(20); }
        public static string Parse(string json)
        {
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;
                double temp = root.GetProperty("main").GetProperty("temp").GetDouble();
                string description = root.GetProperty("weather")[0]
                    .GetProperty("description").GetString();
                return temp.ToString("F2", CultureInfo.InvariantCulture)
                    + " °C, " + description;
            }
        }
        public async Task<string> LoadAsync(City city, string key)
        {
            string url = "https://api.openweathermap.org/data/2.5/weather?lat="
                + city.Latitude.ToString(CultureInfo.InvariantCulture)
                + "&lon=" + city.Longitude.ToString(CultureInfo.InvariantCulture)
                + "&appid=" + Uri.EscapeDataString(key) + "&units=metric&lang=ru";
            using (HttpResponseMessage response = await client.GetAsync(url))
            {
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException("HTTP " + (int)response.StatusCode);
                return Parse(await response.Content.ReadAsStringAsync());
            }
        }
        public void Dispose() { client.Dispose(); }
    }
}
