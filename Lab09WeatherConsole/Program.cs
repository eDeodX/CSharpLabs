using WeatherCore;
namespace Lab09WeatherConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            try
            {
                string data = Path.Combine(AppContext.BaseDirectory, "Data");
                List<City> cities = City.Load(Path.Combine(data, "city.txt"));
                for (int i = 0; i < cities.Count; i++)
                    Console.WriteLine((i + 1) + ". " + cities[i].Name);
                Console.Write("Номер города: ");
                if (!int.TryParse(Console.ReadLine(), out int index)
                    || index < 1 || index > cities.Count)
                {
                    Console.WriteLine("Неверный номер.");
                    return;
                }
                City city = cities[index - 1];
                string weather;
                if (args.Contains("--demo"))
                {
                    Console.WriteLine("Учебный ответ JSON, не текущая погода.");
                    weather = WeatherClient.Parse(File.ReadAllText(Path.Combine(data, "weather_demo.json")));
                }
                else
                {
                    string key = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY");
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        Console.WriteLine("Задай OPENWEATHER_API_KEY.");
                        return;
                    }
                    using (WeatherClient client = new WeatherClient())
                        weather = await client.LoadAsync(city, key);
                }
                Console.WriteLine(city.Name + ": " + weather);
            }
            catch (Exception e) { Console.WriteLine("Ошибка: " + e.Message); }
        }
    }
}
