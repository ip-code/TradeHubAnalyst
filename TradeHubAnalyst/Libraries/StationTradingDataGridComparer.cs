using System.Collections;
using TradeHubAnalyst.Models;

namespace TradeHubAnalyst.Libraries
{
    public class StationTradingDataGridComparer : IComparer
    {
        private string columnName;

        private int sortDirection;

        public StationTradingDataGridComparer(string cn, int sd)
        {
            columnName = cn;
            sortDirection = sd;
        }

        public int Compare(object ob1, object ob2)
        {
            CalculateStationTradeModel x = (CalculateStationTradeModel)ob1;
            CalculateStationTradeModel y = (CalculateStationTradeModel)ob2;

            switch (columnName)
            {
                case "item":
                    return StaticMethods.compareString(x.item, y.item, sortDirection);

                case "buy_price":
                    return StaticMethods.compareDecimal(x.buy_price, y.buy_price, sortDirection);

                case "sell_price":
                    return StaticMethods.compareDecimal(x.sell_price, y.sell_price, sortDirection);

                case "spread_isk":
                    return StaticMethods.compareDecimal(x.spread_isk, y.spread_isk, sortDirection);

                case "spread_percent":
                    return StaticMethods.compareDecimal(x.spread_percent, y.spread_percent, sortDirection);

                default:
                    return 1;
            }
        }
    }
}