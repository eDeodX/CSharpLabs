using MarketDataCore;
using Microsoft.EntityFrameworkCore;
namespace Lab10
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool demo = args.Contains("--demo");
            bool sample = !demo && args.Contains("--sample");
            string path = demo ? "stocks_demo.db" : sample ? "stocks_sample.db" : "stocks.db";
            string data = Path.Combine(AppContext.BaseDirectory, "Data");
            using (StocksContext db = new StocksContext(path))
            {
                await db.Database.EnsureCreatedAsync();
                Console.WriteLine(demo ? "Учебная БД, не реальные котировки." : "База: " + path);
                if (sample) Console.WriteLine("Публичный пример API: только AAPL.");
                Console.Write("Загрузить цены? (да/нет): ");
                if (Console.ReadLine() == "да")
                {
                    string token = Environment.GetEnvironmentVariable("MARKETDATA_TOKEN");
                    if (!demo && !sample && string.IsNullOrWhiteSpace(token))
                    {
                        Console.WriteLine("Для загрузки задай MARKETDATA_TOKEN.");
                    }
                    else
                    {
                        string[] tickers = sample ? new string[] { "AAPL" } : File.ReadAllLines(Path.Combine(data,
                            demo ? "ticker_demo.txt" : "ticker.txt"))
                            .Select(t => t.Trim()).Where(t => t != "").Distinct().ToArray();
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
                                        candles = await client.LoadAsync(ticker,
                                            DateTime.Today.AddYears(-1), DateTime.Today);
                                    await StockService.SaveAsync(db, ticker, candles);
                                    Console.WriteLine(ticker + ": сохранено свечей " + candles.Count);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(ticker + ": " + e.Message);
                                    if (e.Message == "HTTP 401" || e.Message == "HTTP 403"
                                        || e.Message == "HTTP 429") break;
                                }
                                if (!demo) await Task.Delay(1100);
                            }
                        }
                    }
                }
                Console.WriteLine("Строк: акции=" + await db.Stocks.CountAsync()
                    + ", цены=" + await db.StockPrices.CountAsync()
                    + ", состояния=" + await db.TodaysConditions.CountAsync());
                while (true)
                {
                    Console.Write("Тикер (пустая строка — выход): ");
                    string ticker = Console.ReadLine()?.Trim().ToUpperInvariant();
                    if (string.IsNullOrEmpty(ticker)) break;
                    Stock stock = await db.Stocks.FirstOrDefaultAsync(s => s.Ticker == ticker);
                    TodaysCondition condition = null;
                    if (stock != null)
                        condition = await db.TodaysConditions.FirstOrDefaultAsync(c => c.StockId == stock.Id);
                    if (condition == null)
                    {
                        Console.WriteLine("Нет данных для сравнения двух торговых дней.");
                        continue;
                    }
                    Console.WriteLine(ticker + ": " + condition.Condition);
                    Console.WriteLine(condition.PreviousDate.ToString("yyyy-MM-dd") + ": "
                        + condition.PreviousPrice + " -> " + condition.Date.ToString("yyyy-MM-dd")
                        + ": " + condition.LastPrice);
                    if (condition.Date.Date != DateTime.Today)
                        Console.WriteLine("Это последние доступные торговые данные, не котировка за сегодня.");
                }
            }
        }
    }
}
