using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TradeHubAnalyst.Models;

namespace TradeHubAnalyst.Libraries
{
    public class ParseDownloadedString
    {
        private bool hasSell = false;
        private bool hasBuy = false;

        private decimal sellPrice = 0.0m;
        private decimal buyPrice = 0.0m;

        public FormattedTradesModel Trades(string typeID, string rawData)
        {
            string[] websiteData = rawData.Split(new[] { "tbody" }, StringSplitOptions.None);

            string[] tradeData = websiteData[3].Split(new[] { "</tr>" }, StringSplitOptions.None);

            List<string> TradeData = new List<string>(tradeData);
            TradeData.RemoveAt(tradeData.Length - 1);
            TradePerStation sellPerStation = MakeList(TradeData, 0);

            tradeData = websiteData[5].Split(new[] { "</tr>" }, StringSplitOptions.None);
            TradeData = new List<string>(tradeData);
            TradeData.RemoveAt(tradeData.Length - 1);
            TradePerStation buyPerStation = MakeList(TradeData, 1);

            FormattedTradesModel result = new FormattedTradesModel();
            result.type_id = Convert.ToInt32(typeID);
            result.has_data = true;
            result.sell_per_station = sellPerStation;
            result.buy_per_station = buyPerStation;
            result.has_sell = hasSell;
            result.has_buy = hasBuy;
            result.sell_price = sellPrice;
            result.buy_price = buyPrice;

            return result;
        }

        private TradePerStation MakeList(List<string> TradeData, int type)
        {
            List<StationModel> stationList = SqliteDataAccess.LoadStations();

            TradePerStation tradePerStation = new TradePerStation();
            tradePerStation.station_ids = new List<int>();
            tradePerStation.trades = new List<List<SingleTradeModel>>();

            foreach (StationModel station in stationList)
            {
                tradePerStation.station_ids.Add(station.id);
                tradePerStation.trades.Add(new List<SingleTradeModel>());
            }

            int j = 0;
            foreach (string singleTrade in TradeData)
            {
                if (!singleTrade.Contains("td"))
                {
                    continue;
                }

                string[] arr1 = singleTrade.Split(new[] { "</td>" }, StringSplitOptions.None);
                string[] arr2 = arr1[3].Split(new[] { "</span>" }, StringSplitOptions.None);
                string StationName = arr2[1].Trim();

                int k = 0;
                foreach (StationModel station in stationList)
                {
                    bool areEqual = String.Equals(station.name, StationName, StringComparison.Ordinal);

                    if (areEqual)
                    {
                        string[] el2 = arr1[2].Split(new[] { "\">" }, StringSplitOptions.None);
                        el2 = el2[1].Split(new[] { "ISK" }, StringSplitOptions.None);
                        el2[0] = el2[0].Trim();

                        StringBuilder cPrice = new StringBuilder();
                        foreach (char c in el2[0])
                        {
                            if ((c >= '0' && c <= '9') || (c == '.'))
                            {
                                cPrice.Append(c);
                            }
                        }
                        decimal Price = decimal.Parse(cPrice.ToString(), CultureInfo.InvariantCulture);

                        if (j == 0 && type == 0)
                        {
                            sellPrice = Price;
                            j++;
                        }
                        else if (j == 0 && type == 1)
                        {
                            buyPrice = Price;
                            j++;
                        }

                        string[] el21 = arr1[1].Split(new[] { "\">" }, StringSplitOptions.None);
                        el21[1] = el21[1].Trim();

                        StringBuilder sQuantity = new StringBuilder();
                        foreach (char c in el21[1])
                        {
                            if (c >= '0' && c <= '9')
                            {
                                sQuantity.Append(c);
                            }
                        }
                        int Quantity = Int32.Parse(sQuantity.ToString());

                        if (type == 0)
                        {
                            el2 = arr1[5].Split(new[] { " ago" }, StringSplitOptions.None);
                        }
                        else
                        {
                            el2 = arr1[7].Split(new[] { " ago" }, StringSplitOptions.None);
                        }

                        el2 = el2[0].Split(new[] { "\">" }, StringSplitOptions.None);

                        if (el2[1].Contains("s"))
                        {
                            el2[1] = "1";
                        }
                        else
                        {
                            el2[1] = el2[1].Remove(el2[1].Length - 1, 1);
                        }

                        int age = Int32.Parse(el2[1]);

                        if (age < 1000)
                        {
                            SingleTradeModel newTrade = new SingleTradeModel();
                            newTrade.Price = Price;
                            newTrade.Quantity = Quantity;

                            tradePerStation.trades[k].Add(newTrade);

                            if (type == 1)
                            {
                                hasBuy = true;
                            }
                            else
                            {
                                hasSell = true;
                            }
                        }
                    }
                    k++;
                }
            }
            return tradePerStation;
        }
    }
}