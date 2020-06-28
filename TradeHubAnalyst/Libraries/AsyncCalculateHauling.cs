using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using TradeHubAnalyst.Models;

namespace TradeHubAnalyst.Libraries
{
    public class AsyncCalculateHauling
    {
        private bool hasSell = false;
        private bool hasBuy = false;
        private decimal minProfit = 0.05m;
        private decimal sellPrice = 0.0m;
        private decimal buyPrice = 0.0m;
        private List<string> allStations;
        private ItemFiltersModel filters;

        public ObservableCollection<CalculateTradeModel> DoWork(IProgress<DownloadProgressReportModel> progress, CancellationToken cancellationToken, List<FormattedTradesModel> formatedTrades)
        {
            List<string[]> filteredItems = StaticMethods.GetFilteredItems();
            int totalItems = filteredItems.Count;

            filters = SqliteDataAccess.LoadItemFilters();

            allStations = StaticMethods.GetAllStations();

            ObservableCollection<CalculateTradeModel> cTrades = new ObservableCollection<CalculateTradeModel>();

            long startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            for (int i = 0; i < totalItems; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int type_id = Int32.Parse(filteredItems[i][0]);

                if (formatedTrades[i].type_id == type_id)
                {
                    CalculateTradeModel temp = TradePrepare(type_id, filteredItems[i][1], filteredItems[i][2], formatedTrades[i]);

                    if (temp != null)
                    {
                        cTrades.Add(temp);
                    }
                }

                int progressPercentage = Convert.ToInt32(((double)i / filteredItems.Count) * 100);

                DownloadProgressReportModel report = new DownloadProgressReportModel();

                report.PercentageComplete = progressPercentage;
                report.MessageRemaining = "Step 2/2: Calculating data" + StaticMethods.EstimatedTime(startTime, i, totalItems);

                progress.Report(report);
            }

            return cTrades;
        }

        public CalculateTradeModel TradePrepare(int TypeID, string itemName, string itemVolume, FormattedTradesModel formattedTrades)
        {
            List<string> stationList = allStations;

            TradePerStation sellPerStation = formattedTrades.sell_per_station;
            TradePerStation buyPerStation = formattedTrades.buy_per_station;

            hasSell = formattedTrades.has_sell;
            hasBuy = formattedTrades.has_buy;

            sellPrice = formattedTrades.sell_price;
            buyPrice = formattedTrades.buy_price;

            sellPrice = sellPrice + (sellPrice * minProfit);
            buyPrice = buyPrice - (buyPrice * minProfit);

            if (hasBuy && hasSell && sellPrice < buyPrice)
            {
                string sellStationName = "";
                string buyStationName = "";
                int tradeQuantity = 0;
                decimal purchasePrice = 0;
                decimal tradeProfit = 0;
                decimal tradeROI = 0;
                int maxQuantity = 0;
                bool isMaxQuantityValid = true;

                if (filters.user_cargo_capacity > 0)
                {
                    string[] stringValue1 = (filters.user_cargo_capacity / Decimal.Parse(itemVolume, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture).Split('.');
                    maxQuantity = Convert.ToInt32(stringValue1[0]);
                }

                decimal userAvailableMoney = filters.user_available_money;

                for (int i = 0; i < sellPerStation.station_ids.Count; i++)
                {
                    if (filters.selected_hauling_station_id > 0 && filters.selected_hauling_station_id != sellPerStation.station_ids[i])
                    {
                        continue;
                    }

                    List<SingleTradeModel> sellTrades = sellPerStation.trades[i];

                    for (int j = 0; j < buyPerStation.station_ids.Count; j++)
                    {
                        List<SingleTradeModel> buyTrades = buyPerStation.trades[j];

                        int newTradeQuantity = 0;
                        decimal newPurchasePrice = 0;
                        decimal newTradeProfit = 0;

                        int currentSellOrder = 0;
                        int currentBuyOrder = 0;

                        while (currentSellOrder < sellTrades.Count && currentBuyOrder < buyTrades.Count)
                        {
                            decimal adjustedSellTradePrice = sellTrades[currentSellOrder].Price + (sellTrades[currentSellOrder].Price * minProfit);
                            decimal adjustedBuyTradePrice = buyTrades[currentBuyOrder].Price - (buyTrades[currentBuyOrder].Price * minProfit);

                            if (adjustedSellTradePrice < adjustedBuyTradePrice && isMaxQuantityValid)
                            {
                                if (filters.user_available_money > 0)
                                {
                                    if (maxQuantity == 0)
                                    {
                                        string[] stringValue2 = (userAvailableMoney / sellTrades[currentSellOrder].Price).ToString(CultureInfo.InvariantCulture).Split('.');
                                        maxQuantity = Convert.ToInt32(stringValue2[0]);
                                    }
                                    else
                                    {
                                        if ((maxQuantity * sellTrades[currentSellOrder].Price) > userAvailableMoney)
                                        {
                                            string[] stringValue3 = (userAvailableMoney / sellTrades[currentSellOrder].Price).ToString(CultureInfo.InvariantCulture).Split('.');
                                            maxQuantity = Convert.ToInt32(stringValue3[0]);
                                        }
                                    }
                                }

                                if (filters.user_cargo_capacity > 0 || filters.user_available_money > 0)
                                {
                                    if (sellTrades[currentSellOrder].Quantity > maxQuantity)
                                    {
                                        sellTrades[currentSellOrder].Quantity = maxQuantity;
                                    }

                                    if (buyTrades[currentBuyOrder].Quantity > maxQuantity)
                                    {
                                        buyTrades[currentBuyOrder].Quantity = maxQuantity;
                                    }
                                }

                                if (sellTrades[currentSellOrder].Quantity < buyTrades[currentBuyOrder].Quantity)
                                {
                                    newTradeQuantity += sellTrades[currentSellOrder].Quantity;

                                    if (filters.user_cargo_capacity > 0 || filters.user_available_money > 0)
                                    {
                                        maxQuantity -= sellTrades[currentSellOrder].Quantity;
                                    }

                                    newPurchasePrice += sellTrades[currentSellOrder].Price * sellTrades[currentSellOrder].Quantity;

                                    if (filters.user_available_money > 0)
                                    {
                                        userAvailableMoney -= sellTrades[currentSellOrder].Price * sellTrades[currentSellOrder].Quantity;
                                    }

                                    newTradeProfit += (buyTrades[currentBuyOrder].Price - sellTrades[currentSellOrder].Price) * sellTrades[currentSellOrder].Quantity;
                                    buyTrades[currentBuyOrder].Quantity -= sellTrades[currentSellOrder].Quantity;
                                    currentSellOrder++;
                                }
                                else if (sellTrades[currentSellOrder].Quantity == buyTrades[currentBuyOrder].Quantity)
                                {
                                    newTradeQuantity += sellTrades[currentSellOrder].Quantity;

                                    if (filters.user_cargo_capacity > 0 || filters.user_available_money > 0)
                                    {
                                        maxQuantity -= sellTrades[currentSellOrder].Quantity;
                                    }

                                    newPurchasePrice += sellTrades[currentSellOrder].Price * sellTrades[currentSellOrder].Quantity;

                                    if (filters.user_available_money > 0)
                                    {
                                        userAvailableMoney -= sellTrades[currentSellOrder].Price * sellTrades[currentSellOrder].Quantity;
                                    }

                                    newTradeProfit += (buyTrades[currentBuyOrder].Price - sellTrades[currentSellOrder].Price) * sellTrades[currentSellOrder].Quantity;
                                    buyTrades[currentBuyOrder].Quantity -= sellTrades[currentSellOrder].Quantity;
                                    currentBuyOrder++;
                                    currentSellOrder++;
                                }
                                else
                                {
                                    newTradeQuantity += buyTrades[currentBuyOrder].Quantity;

                                    if (filters.user_cargo_capacity > 0 || filters.user_available_money > 0)
                                    {
                                        maxQuantity -= buyTrades[currentBuyOrder].Quantity;
                                    }

                                    newPurchasePrice += sellTrades[currentSellOrder].Price * buyTrades[currentBuyOrder].Quantity;

                                    if (filters.user_available_money > 0)
                                    {
                                        userAvailableMoney -= sellTrades[currentSellOrder].Price * buyTrades[currentBuyOrder].Quantity;
                                    }

                                    newTradeProfit += (buyTrades[currentBuyOrder].Price - sellTrades[currentSellOrder].Price) * buyTrades[currentBuyOrder].Quantity;
                                    sellTrades[currentSellOrder].Quantity -= buyTrades[currentBuyOrder].Quantity;
                                    currentBuyOrder++;
                                }

                                if (filters.user_cargo_capacity > 0 || filters.user_available_money > 0)
                                {
                                    if (maxQuantity == 0)
                                    {
                                        isMaxQuantityValid = false;
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (newTradeProfit > tradeProfit)
                        {
                            sellStationName = stationList[i];
                            buyStationName = stationList[j];
                            tradeQuantity = newTradeQuantity;
                            purchasePrice = newPurchasePrice;
                            tradeProfit = newTradeProfit;
                            tradeROI = (tradeProfit / purchasePrice) * 100;
                        }
                    }
                }

                if (tradeQuantity > 0)
                {
                    CalculateTradeModel newTrade = new CalculateTradeModel();
                    newTrade.Item = itemName;
                    string[] from = sellStationName.Split(new[] { " " }, StringSplitOptions.None);
                    newTrade.From = from[0];
                    newTrade.FromFull = sellStationName;
                    string[] to = buyStationName.Split(new[] { " " }, StringSplitOptions.None);
                    newTrade.To = to[0];
                    newTrade.ToFull = buyStationName;
                    newTrade.NumItems = tradeQuantity;
                    newTrade.Price = purchasePrice;
                    newTrade.Profit = tradeProfit;
                    newTrade.ROI = tradeROI;
                    newTrade.Link = "https://evemarketer.com/types/" + TypeID.ToString();

                    return newTrade;
                }
            }

            return null;
        }
    }
}