namespace TradeHubAnalyst.Models
{
    public class FormattedTradesModel
    {
        public int type_id { get; set; }

        public bool has_data { get; set; }

        public TradePerStation sell_per_station { get; set; }

        public TradePerStation buy_per_station { get; set; }

        public bool has_sell { get; set; }

        public bool has_buy { get; set; }

        public decimal sell_price { get; set; }

        public decimal buy_price { get; set; }
    }
}