using MarketDataCore;
using System.Globalization;
namespace Lab09Stocks
{
    internal class Program
    {
        private static object fileLock = new object();
        static void Save(string path, string ticker, List<Candle> candles)
        {
            decimal average = MarketClient.Average(candles);
            string line = ticker + ":" + average.ToString("F4", CultureInfo.InvariantCulture);
            lock (fileLock)
            {
                File.AppendAllText(path, line + Environment.NewLine);
                Console.WriteLine(line);
            }
        }
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool demo = args.Contains("--demo");
            bool sample = !demo && args.Contains("--sample");
            string data = Path.Combine(AppContext.BaseDirectory, "Data");
            string output = demo ? "averages_demo.txt" : sample ? "averages_sample.txt" : "averages.txt";
            string errors = demo ? "errors_demo.txt" : sample ? "errors_sample.txt" : "errors.txt";
            string[] tickers = sample ? new string[] { "AAPL" } : File.ReadAllLines(Path.Combine(data,
                demo ? "ticker_demo.txt" : "ticker.txt"))
                .Select(x => x.Trim()).Where(x => x != "").Distinct().ToArray();
            string token = Environment.GetEnvironmentVariable("MARKETDATA_TOKEN");
            if (!demo && !sample && string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("Задай переменную MARKETDATA_TOKEN.");
                return;
            }
            File.WriteAllText(output, "");
            File.WriteAllText(errors, "");
            DateTime to = DateTime.Today;
            DateTime from = to.AddYears(-1);
            Console.WriteLine(demo ? "Учебные данные, не реальные котировки."
                : "Период: " + from.ToString("yyyy-MM-dd") + " — " + to.ToString("yyyy-MM-dd"));
            if (sample) Console.WriteLine("Публичный пример API: только AAPL.");
            List<Task> tasks = new List<Task>();
            using (MarketClient client = new MarketClient(token))
            {
                foreach (string ticker in tickers)
                {
                    try
                    {
                        List<Candle> candles;
                        if (demo)
                            candles = MarketClient.Parse(File.ReadAllText(
                                Path.Combine(data, "Demo", ticker + ".json")));
                        else
                            candles = await client.LoadAsync(ticker, from, to);
                        tasks.Add(Task.Run(() => Save(output, ticker, candles)));
                    }
                    catch (Exception e)
                    {
                        lock (fileLock)
                        {
                            File.AppendAllText(errors, ticker + ":" + e.Message + Environment.NewLine);
                            Console.WriteLine(ticker + ": ошибка — " + e.Message);
                        }
                        if (e.Message == "HTTP 401" || e.Message == "HTTP 403"
                            || e.Message == "HTTP 429") break;
                    }
                    if (!demo) await Task.Delay(1100);
                }
                await Task.WhenAll(tasks);
            }
            Console.WriteLine("Сохранено строк: " + File.ReadAllLines(output).Length);
            Console.WriteLine("Результат: " + Path.GetFullPath(output));
            Console.WriteLine("Ошибок: " + File.ReadAllLines(errors).Length);
        }
    }
}
