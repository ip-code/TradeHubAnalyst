namespace TradeHubAnalyst.Models
{
    public class CalculateTradeModel
    {
        public string Item { get; set; }
        public string From { get; set; }
        public string FromFull { get; set; }
        public string To { get; set; }
        public string ToFull { get; set; }
        public int NumItems { get; set; }
        public decimal Price { get; set; }
        public decimal Profit { get; set; }
        public decimal ROI { get; set; }
        public string Link { get; set; }
    }
}