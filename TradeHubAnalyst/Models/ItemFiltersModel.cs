namespace TradeHubAnalyst.Models
{
    public class ItemFiltersModel
    {
        public int id { get; set; }

        public decimal max_volume { get; set; }

        public decimal max_price { get; set; }

        public long min_trade_volume { get; set; }

        public int ignore_zero { get; set; }

        public int filtered_items { get; set; }

        public int selected_hauling_station_id { get; set; }

        public decimal user_cargo_capacity { get; set; }

        public decimal user_available_money { get; set; }

        public int selected_station_trading_station_id { get; set; }

        public int updated_item_max_age { get; set; }

        public int max_async_tasks { get; set; }

        public decimal user_brokers_fee { get; set; }

        public decimal user_sales_tax { get; set; }
    }
}
