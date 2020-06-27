using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using TradeHubAnalyst.Models;

namespace TradeHubAnalyst.Libraries
{
    public class AsyncCalculateStationTrading
    {
        private bool hasSell = false;
        private bool hasBuy = false;
        private decimal minProfit = 0.05m;
        private decimal sellPrice = 0.0m;
        private decimal buyPrice = 0.0m;
        private List<string> allStations;
        private ItemFiltersModel filters;

        public ObservableCollection<CalculateStationTradeModel> DoWork(IProgress<DownloadProgressReportModel> progress, CancellationToken cancellationToken, List<FormattedTradesModel> formatedTrades)
        {
            List<string[]> filteredItems = StaticMethods.GetFilteredItems();
            int totalItems = filteredItems.Count;

            if (formatedTrades.Count < totalItems)
            {
                totalItems = formatedTrades.Count;
            }

            filters = SqliteDataAccess.LoadItemFilters();

            allStations = StaticMethods.GetAllStations();

            ObservableCollection<CalculateStationTradeModel> cTrades = new ObservableCollection<CalculateStationTradeModel>();

            DateTime startTime = DateTime.Now;

            for (int i = 0; i < totalItems; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int type_id = Int32.Parse(filteredItems[i][0]);

                if (formatedTrades[i].type_id == type_id)
                {
                    CalculateStationTradeModel temp = TradePrepare(type_id, filteredItems[i][1], filteredItems[i][2], formatedTrades[i]);

                    if (temp != null)
                    {
                        cTrades.Add(temp);
                    }
                }

                int progressPercentage = Convert.ToInt32(((double)i / filteredItems.Count) * 100);

                DownloadProgressReportModel report = new DownloadProgressReportModel();

                report.PercentageComplete = progressPercentage;
                report.MessageRemaining = "Working" + StaticMethods.EstimatedTime(startTime, i, totalItems);

                progress.Report(report);

            }

            return cTrades;
        }


        public CalculateStationTradeModel TradePrepare(int TypeID, string itemName, string itemVolume, FormattedTradesModel formattedTrades)
        {
            TradePerStation sellPerStation = formattedTrades.sell_per_station;
            TradePerStation buyPerStation = formattedTrades.buy_per_station;

            hasSell = formattedTrades.has_sell;
            hasBuy = formattedTrades.has_buy;          
            
            if (hasBuy && hasSell)
            {
                for (int i = 0; i < sellPerStation.station_ids.Count; i++)
                {
                    if (filters.selected_station_trading_station_id.Equals(sellPerStation.station_ids[i]) && sellPerStation.trades[i].Count > 0)
                    {
                        List<SingleTradeModel> sellTrades = sellPerStation.trades[i];

                        buyPrice = sellTrades[0].Price; // this is now the price at which I will buy
                        buyPrice = buyPrice + (buyPrice * filters.user_brokers_fee);

                        for (int j = 0; j < buyPerStation.station_ids.Count; j++)
                        {
                            if (filters.selected_station_trading_station_id.Equals(buyPerStation.station_ids[i]) && buyPerStation.trades[j].Count > 0)
                            {
                                List<SingleTradeModel> buyTrades = buyPerStation.trades[j];

                                sellPrice = buyTrades[0].Price; // this is now the price at which I will sell
                                sellPrice = sellPrice - (sellPrice * (filters.user_brokers_fee + filters.user_sales_tax) / 100);

                                if (buyPrice < sellPrice)
                                {
                                    CalculateStationTradeModel newTrade = new CalculateStationTradeModel();
                                    newTrade.item = itemName;
                                    newTrade.buy_price = buyPrice;
                                    newTrade.sell_price = sellPrice;
                                    newTrade.spread_isk = sellPrice - buyPrice;
                                    newTrade.spread_percent = newTrade.spread_isk / sellPrice * 100;
                                    newTrade.link = "https://evemarketer.com/types/" + TypeID.ToString();

                                    return newTrade;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
