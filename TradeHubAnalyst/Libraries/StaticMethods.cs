using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using TradeHubAnalyst.Models;

namespace TradeHubAnalyst.Libraries
{
    internal class StaticMethods
    {
        public static ObservableCollection<CalculateTradeModel> LoadEmptyTrades(int amount)
        {
            ObservableCollection<CalculateTradeModel> displayTrades = new ObservableCollection<CalculateTradeModel>();

            for (int x = 0; x < amount; x++)
            {
                displayTrades.Add(new CalculateTradeModel());
            }

            return displayTrades;
        }

        public static ObservableCollection<CalculateStationTradeModel> LoadEmptyStationTrades(int amount)
        {
            ObservableCollection<CalculateStationTradeModel> displayTrades = new ObservableCollection<CalculateStationTradeModel>();

            for (int x = 0; x < amount; x++)
            {
                displayTrades.Add(new CalculateStationTradeModel());
            }

            return displayTrades;
        }

        private static IEnumerable<string> EnumerateLines(TextReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        public static string[] ReadAllResourceLines(string resourceText)
        {
            using (StringReader reader = new StringReader(resourceText))
            {
                return EnumerateLines(reader).ToArray();
            }
        }

        public static string EstimatedTime(long startTime, int itemsDone, int itemsTotal)
        {
            long timeSpent = DateTimeOffset.Now.ToUnixTimeSeconds() - startTime;

            string formattedTime = ", estimated ";

            double timeLeft;
            double minutesLeft;
            double minutes = 0;
            double hours = 0;
            double secondsLeft;

            if (timeSpent < 5 || itemsDone == 0)
            {
                return "...";
            }

            timeLeft = (timeSpent / (double)itemsDone) * (double)(itemsTotal - itemsDone);

            if (timeLeft > (59 * 60))
            {
                minutesLeft = timeLeft % 3600;

                secondsLeft = minutesLeft % 60;
                minutes = (minutesLeft - secondsLeft) / 60;
                hours = (timeLeft - minutesLeft) / 3600;

                if (hours <= 1)
                {
                    formattedTime = formattedTime + hours.ToString("N0") + " hour and ";
                }
                else
                {
                    formattedTime = formattedTime + hours.ToString("N0") + " hours and ";
                }

                if (minutes <= 1)
                {
                    formattedTime = formattedTime + minutes.ToString("N0") + " minute";
                }
                else
                {
                    formattedTime = formattedTime + minutes.ToString("N0") + " minutes";
                }
            }
            else if (timeLeft > 60 && timeLeft <= (59 * 60))
            {
                minutesLeft = timeLeft;
                secondsLeft = minutesLeft % 60;
                minutes = (minutesLeft - secondsLeft) / 60;

                if (minutes <= 1)
                {
                    formattedTime = formattedTime + minutes.ToString("N0") + " minute";
                }
                else
                {
                    formattedTime = formattedTime + minutes.ToString("N0") + " minutes";
                }
                if (minutes < 5)
                {
                    if (secondsLeft == 1)
                    {
                        formattedTime = formattedTime + " and " + secondsLeft.ToString("N0") + " second";
                    }
                    else
                    {
                        formattedTime = formattedTime + " and " + secondsLeft.ToString("N0") + " seconds";
                    }
                }
            }
            else
            {
                secondsLeft = timeLeft;

                if (secondsLeft == 1)
                {
                    formattedTime = formattedTime + secondsLeft.ToString("N0") + " second";
                }
                else
                {
                    formattedTime = formattedTime + secondsLeft.ToString("N0") + " seconds";
                }
            }

            formattedTime += " remaining.";

            if (hours < 1 && minutes < 1 && secondsLeft < 10)
            {
                formattedTime = ", finishing up...";
            }

            return formattedTime;
        }

        public static List<string[]> GetFilteredItems()
        {
            List<ItemModel> items = SqliteDataAccess.LoadItems();

            ItemFiltersModel filters = SqliteDataAccess.LoadItemFilters();

            List<string[]> filteredItems = new List<string[]>();

            foreach (ItemModel item in items)
            {
                bool isIgnorePassed = false;
                bool isFilterVolumePassed = false;
                bool isFilterPricePassed = false;

                if (filters.ignore_zero > 0)
                {
                    if (item.volume > 0 && item.sell_price > 0)
                    {
                        isIgnorePassed = true;
                    }
                }
                else
                {
                    isIgnorePassed = true;
                }

                decimal filterMaxVolume = filters.max_volume;

                if (filters.user_cargo_capacity < filters.max_volume && filters.user_cargo_capacity > 0)
                {
                    filterMaxVolume = filters.user_cargo_capacity;
                }

                if (filterMaxVolume > 0)
                {
                    if (item.volume < filterMaxVolume)
                    {
                        isFilterVolumePassed = true;
                    }
                }
                else
                {
                    isFilterVolumePassed = true;
                }

                decimal filterMaxPrice = filters.max_price;

                if (filters.user_available_money < filters.max_price && filters.user_available_money > 0)
                {
                    filterMaxPrice = filters.user_available_money;
                }

                if (filterMaxPrice > 0)
                {
                    if (item.sell_price < filterMaxPrice)
                    {
                        isFilterPricePassed = true;
                    }
                }
                else
                {
                    isFilterPricePassed = true;
                }

                if (isIgnorePassed && isFilterVolumePassed && isFilterPricePassed && item.trade_volume > filters.min_trade_volume)
                {
                    filteredItems.Add(new string[] { item.type_id.ToString(), item.name, item.volume.ToString(CultureInfo.InvariantCulture) });
                }
            }

            return filteredItems;
        }

        public static ItemFiltersModel SaveDefaultItemFilterModel()
        {
            ItemFiltersModel defaultModel = new ItemFiltersModel();
            defaultModel.id = 1;
            defaultModel.max_volume = 0;
            defaultModel.max_price = 0;
            defaultModel.min_trade_volume = 0;
            defaultModel.ignore_zero = 0;
            defaultModel.filtered_items = 0;
            defaultModel.selected_hauling_station_id = 0;
            defaultModel.user_cargo_capacity = 0;
            defaultModel.user_available_money = 0;
            defaultModel.selected_station_trading_station_id = 0;
            defaultModel.updated_item_max_age = 7;
            defaultModel.max_async_tasks = 5;
            defaultModel.user_brokers_fee = 5;
            defaultModel.user_sales_tax = 5;

            SqliteDataAccess.SaveItemFilters(defaultModel);

            return defaultModel;
        }

        public static List<string> GetAllStations()
        {
            List<StationModel> stations = SqliteDataAccess.LoadStations();

            List<string> allStations = new List<string>();

            foreach (StationModel station in stations)
            {
                allStations.Add(station.name);
            }

            return allStations;
        }

        public static int compareString(string x, string y, int sortDirection)
        {
            if (string.IsNullOrEmpty(x))
            {
                return 1;
            }
            else if (string.IsNullOrEmpty(y))
            {
                return -1;
            }
            else
            {
                if (sortDirection == 0)
                {
                    return string.Compare(x, y);
                }
                else
                {
                    return (string.Compare(x, y) * -1);
                }
            }
        }

        public static int compareInt(int x, int y, int sortDirection)
        {
            if (x == 0 && y > 0)
            {
                return 1;
            }
            else if (x > 0 && y == 0)
            {
                return -1;
            }
            else if (x == y)
            {
                return 0;
            }
            else
            {
                int result = -1;

                if (x > y)
                {
                    result = 1;
                }

                if (sortDirection == 0)
                {
                    return result;
                }
                else
                {
                    return (result * -1);
                }
            }
        }

        public static int compareDecimal(decimal x, decimal y, int sortDirection)
        {
            if (x == 0 && y > 0)
            {
                return 1;
            }
            else if (x > 0 && y == 0)
            {
                return -1;
            }
            else
            {
                if (sortDirection == 0)
                {
                    return decimal.Compare(x, y);
                }
                else
                {
                    return (decimal.Compare(x, y) * -1);
                }
            }
        }

        public static bool hasNewVersion()
        {
            string url = "https://raw.githubusercontent.com/ip-code/TradeHubAnalystWeb/master/api/version";
            WebClient webClient = new WebClient();
            string newVersion = webClient.DownloadString(url);
            webClient.Dispose();

            if (!newVersion.Equals(Properties.Resources.Version))
            {
                return true;
            }

            return false;
        }

        public static string getNewVersion()
        {
            string url = "https://raw.githubusercontent.com/ip-code/TradeHubAnalystWeb/master/api/version";
            WebClient webClient = new WebClient();
            string newVersion = webClient.DownloadString(url);
            webClient.Dispose();

            return newVersion;
        }

        public static string getNewDownloadLink()
        {
            string url = "https://raw.githubusercontent.com/ip-code/TradeHubAnalystWeb/master/api/download";
            WebClient webClient = new WebClient();
            string newDownloadLink = webClient.DownloadString(url);
            webClient.Dispose();

            return newDownloadLink;
        }
    }
}