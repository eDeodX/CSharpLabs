using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
namespace MarketDataCore
{
    public class MarketClient : IDisposable
    {
        private HttpClient client = new HttpClient();
        public MarketClient(string token)
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }
        public static List<Candle> Parse(string json)
        {
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;
                string status = root.GetProperty("s").GetString();
                if (status != "ok")
                    throw new InvalidOperationException("Статус API: " + status);
                JsonElement times = root.GetProperty("t");
                JsonElement highs = root.GetProperty("h");
                JsonElement lows = root.GetProperty("l");
                JsonElement closes = root.GetProperty("c");
                int count = times.GetArrayLength();
                if (count == 0 || highs.GetArrayLength() != count
                    || lows.GetArrayLength() != count || closes.GetArrayLength() != count)
                    throw new InvalidOperationException("Пустые или несогласованные массивы свечей.");
                List<Candle> result = new List<Candle>();
                for (int i = 0; i < count; i++)
                {
                    Candle candle = new Candle();
                    candle.Date = DateTimeOffset.FromUnixTimeSeconds(times[i].GetInt64()).UtcDateTime.Date;
                    candle.High = highs[i].GetDecimal();
                    candle.Low = lows[i].GetDecimal();
                    candle.Close = closes[i].GetDecimal();
                    result.Add(candle);
                }
                return result;
            }
        }
        public async Task<List<Candle>> LoadAsync(string ticker, DateTime from, DateTime to)
        {
            string url = "https://api.marketdata.app/v1/stocks/candles/D/"
                + Uri.EscapeDataString(ticker) + "/?from="
                + from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                + "&to=" + to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            using (HttpResponseMessage response = await client.GetAsync(url))
            {
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException("HTTP " + (int)response.StatusCode);
                return Parse(await response.Content.ReadAsStringAsync());
            }
        }
        public static decimal Average(List<Candle> candles)
        {
            if (candles.Count == 0) throw new ArgumentException("Нет свечей.");
            decimal sum = 0;
            foreach (Candle c in candles) sum += (c.High + c.Low) / 2;
            return sum / candles.Count;
        }
        public void Dispose() { client.Dispose(); }
    }
}
