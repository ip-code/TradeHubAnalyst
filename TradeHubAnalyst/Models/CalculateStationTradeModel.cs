namespace TradeHubAnalyst.Models
{
    public class CalculateStationTradeModel
    {
        public string item { get; set; }
        public decimal buy_price { get; set; }
        public decimal sell_price { get; set; }
        public decimal spread_isk { get; set; }
        public decimal spread_percent { get; set; }
        public string link { get; set; }
    }
}