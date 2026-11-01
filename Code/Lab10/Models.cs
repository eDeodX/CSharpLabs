namespace Lab10
{
    public class Stock
    {
        public int Id { get; set; }
        public string Ticker { get; set; }
    }
    public class StockPrice
    {
        public int Id { get; set; }
        public int StockId { get; set; }
        public DateTime Date { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }
    public class TodaysCondition
    {
        public int Id { get; set; }
        public int StockId { get; set; }
        public DateTime Date { get; set; }
        public DateTime PreviousDate { get; set; }
        public decimal LastPrice { get; set; }
        public decimal PreviousPrice { get; set; }
        public string Condition { get; set; }
    }
}
