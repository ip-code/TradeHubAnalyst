using System.Collections.Generic;

namespace TradeHubAnalyst.Models
{
    public class TradePerStation
    {
        public List<int> station_ids { get; set; }

        public List<List<SingleTradeModel>> trades { get; set; }
    }
}