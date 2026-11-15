using Microsoft.EntityFrameworkCore;

public class Stock
{
    public int Id { get; set; }
    public string Ticker { get; set; } = "";
}

public class StockPrice
{
    public int Id { get; set; }
    public int StockId { get; set; }
    public DateTime Date { get; set; }
    public decimal Close { get; set; }
}

public class StocksContext(string connection) : DbContext
{
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockPrice> StockPrices => Set<StockPrice>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite(connection);
    }
}
