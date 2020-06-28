namespace TradeHubAnalyst.Models
{
    public class ItemModel
    {
        public int type_id { get; set; }

        public string name { get; set; }

        public decimal volume { get; set; }

        public decimal sell_price { get; set; }

        public long trade_volume { get; set; }

        public long updated_at { get; set; }
    }
}