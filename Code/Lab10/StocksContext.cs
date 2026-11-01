using Microsoft.EntityFrameworkCore;
namespace Lab10
{
    public class StocksContext : DbContext
    {
        private string path;
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockPrice> StockPrices { get; set; }
        public DbSet<TodaysCondition> TodaysConditions { get; set; }
        public StocksContext(string path) { this.path = path; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=" + path);
        }
        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<Stock>().HasIndex(s => s.Ticker).IsUnique();
            model.Entity<StockPrice>().HasIndex(p => new { p.StockId, p.Date }).IsUnique();
            model.Entity<TodaysCondition>().HasIndex(c => c.StockId).IsUnique();
            model.Entity<StockPrice>().HasOne<Stock>().WithMany()
                .HasForeignKey(p => p.StockId);
            model.Entity<TodaysCondition>().HasOne<Stock>().WithOne()
                .HasForeignKey<TodaysCondition>(c => c.StockId);
        }
    }
}
