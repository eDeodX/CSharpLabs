using MarketDataCore;
using Microsoft.EntityFrameworkCore;
namespace Lab10
{
    public class StockService
    {
        public static async Task SaveAsync(StocksContext db, string ticker, List<Candle> candles)
        {
            Stock stock = await db.Stocks.FirstOrDefaultAsync(s => s.Ticker == ticker);
            if (stock == null)
            {
                stock = new Stock { Ticker = ticker };
                db.Stocks.Add(stock);
                await db.SaveChangesAsync();
            }
            foreach (Candle candle in candles)
            {
                StockPrice price = await db.StockPrices.FirstOrDefaultAsync(
                    p => p.StockId == stock.Id && p.Date == candle.Date);
                if (price == null)
                {
                    price = new StockPrice { StockId = stock.Id, Date = candle.Date };
                    db.StockPrices.Add(price);
                }
                price.High = candle.High;
                price.Low = candle.Low;
                price.Close = candle.Close;
            }
            await db.SaveChangesAsync();
            List<StockPrice> last = await db.StockPrices.Where(p => p.StockId == stock.Id)
                .OrderByDescending(p => p.Date).Take(2).ToListAsync();
            TodaysCondition condition = await db.TodaysConditions
                .FirstOrDefaultAsync(c => c.StockId == stock.Id);
            if (last.Count < 2)
            {
                if (condition != null) db.TodaysConditions.Remove(condition);
                await db.SaveChangesAsync();
                return;
            }
            if (condition == null)
            {
                condition = new TodaysCondition { StockId = stock.Id };
                db.TodaysConditions.Add(condition);
            }
            condition.Date = last[0].Date;
            condition.PreviousDate = last[1].Date;
            condition.LastPrice = last[0].Close;
            condition.PreviousPrice = last[1].Close;
            if (condition.LastPrice > condition.PreviousPrice) condition.Condition = "выросла";
            else if (condition.LastPrice < condition.PreviousPrice) condition.Condition = "упала";
            else condition.Condition = "не изменилась";
            await db.SaveChangesAsync();
        }
    }
}
