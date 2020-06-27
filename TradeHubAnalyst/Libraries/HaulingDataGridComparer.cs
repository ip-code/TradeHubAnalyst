using System.Collections;
using TradeHubAnalyst.Models;

namespace TradeHubAnalyst.Libraries
{
    public class HaulingDataGridComparer : IComparer
    {
        string columnName;

        int sortDirection;

        public HaulingDataGridComparer(string cn, int sd)
        {
            columnName = cn;
            sortDirection = sd;
        }

        public int Compare(object ob1, object ob2)
        {
            CalculateTradeModel x = (CalculateTradeModel)ob1;
            CalculateTradeModel y = (CalculateTradeModel)ob2;

            switch (columnName)
            {
                case "Item":
                    return StaticMethods.compareString(x.Item, y.Item, sortDirection);

                case "From":
                    return StaticMethods.compareString(x.From, y.From, sortDirection);

                case "To":
                    return StaticMethods.compareString(x.To, y.To, sortDirection);

                case "NumItems":
                    return StaticMethods.compareInt(x.NumItems, y.NumItems, sortDirection);

                case "Price":
                    return StaticMethods.compareDecimal(x.Price, y.Price, sortDirection);

                case "Profit":
                    return StaticMethods.compareDecimal(x.Profit, y.Profit, sortDirection);

                case "ROI":
                    return StaticMethods.compareDecimal(x.ROI, y.ROI, sortDirection);

                default:
                    return 1;
            }
        }
    }
}

