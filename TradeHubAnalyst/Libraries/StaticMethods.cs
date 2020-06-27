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
    class StaticMethods
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

        static IEnumerable<string> EnumerateLines(TextReader reader)
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

        public static string EstimatedTime(DateTime startTime, int itemsDone, int itemsTotal)
        {
            var timeSpent = DateTime.Now - startTime;

            string formattedTime = ", estimated ";

            int rawTime;
            int minutesLeft = 0;
            int minutes = 0;
            int hours = 0;
            int secondsLeft = 0;

            if (timeSpent.TotalSeconds < 5)
            {
                return "...";
            }

            double timeLeft = timeSpent.TotalSeconds / itemsDone * (itemsTotal - itemsDone);
            rawTime = Convert.ToInt32(timeLeft);

            if (rawTime > (59 * 60))
            {
                minutesLeft = rawTime % 3600;
                secondsLeft = minutesLeft % 60;
                minutes = (minutesLeft - secondsLeft) / 60;
                hours = (rawTime - minutesLeft) / 3600;

                if (hours == 1)
                {
                    formattedTime = formattedTime + hours.ToString() + " hour and ";
                }
                else
                {
                    formattedTime = formattedTime + hours.ToString() + " hours and ";
                }

                if (minutes == 1)
                {
                    formattedTime = formattedTime + minutes.ToString() + " minute";
                }
                else
                {
                    formattedTime = formattedTime + minutes.ToString() + " minutes";
                }
            }
            else if (rawTime > 60 && rawTime <= (59 * 60))
            {
                minutesLeft = rawTime;
                secondsLeft = minutesLeft % 60;
                minutes = (minutesLeft - secondsLeft) / 60;

                if (minutes == 1)
                {
                    formattedTime = formattedTime + minutes.ToString() + " minute";
                }
                else
                {
                    formattedTime = formattedTime + minutes.ToString() + " minutes";
                }
                if (minutes < 5)
                {
                    if (secondsLeft == 1)
                    {
                        formattedTime = formattedTime + " and " + secondsLeft.ToString() + " second";
                    }
                    else
                    {
                        formattedTime = formattedTime + " and " + secondsLeft.ToString() + " seconds";
                    }
                }
            }
            else
            {
                secondsLeft = rawTime;

                if (secondsLeft == 1)
                {
                    formattedTime = formattedTime + secondsLeft.ToString() + " second";
                }
                else
                {
                    formattedTime = formattedTime + secondsLeft.ToString() + " seconds";
                }
            }

            formattedTime += " remaining.";

            if (hours < 1 && minutes < 1 && secondsLeft == 1)
            {
                formattedTime = ", finishing download...";
            }

            return formattedTime;
        }

        public static bool checkVersion()
        {
            string url = "http://bretonis.mygamesonline.org/version";
            var webClient = new WebClient();
            string data = webClient.DownloadString(url);

            if (!String.IsNullOrEmpty(data))
            {
                string[] arr1 = data.Split(new[] { "TradeHubAnalyst:" }, StringSplitOptions.None);
                arr1 = arr1[1].Split(new[] { "</" }, StringSplitOptions.None);
                arr1 = arr1[0].Split(new[] { "." }, StringSplitOptions.None);
                int major1 = int.Parse(arr1[0]);
                decimal minor1 = 0;


                string[] arr2 = Properties.Resources.Version.Split(new[] { "." }, StringSplitOptions.None);
                int major2 = int.Parse(arr2[0]);
                decimal minor2 = 0;

                if (major1 == major2)
                {
                    StringBuilder minorString = new StringBuilder();
                    decimal i = 0;
                    foreach (char c in arr1[1])
                    {
                        if (c >= '0' && c <= '9')
                        {
                            minorString.Append(c);
                        }
                        else
                        {
                            switch (c)
                            {
                                case 'a':
                                    i = 0.1m;
                                    break;
                                case 'b':
                                    i = 0.2m;
                                    break;
                                case 'c':
                                    i = 0.3m;
                                    break;
                                case 'd':
                                    i = 0.4m;
                                    break;
                                case 'e':
                                    i = 0.5m;
                                    break;
                                case 'f':
                                    i = 0.6m;
                                    break;
                            }
                        }
                    }

                    minor1 = decimal.Parse(minorString.ToString());
                    minor1 += i;

                    minorString = new StringBuilder();
                    i = 0;
                    foreach (char c in arr2[1])
                    {
                        if (c >= '0' && c <= '9')
                        {
                            minorString.Append(c);
                        }
                        else
                        {
                            switch (c)
                            {
                                case 'a':
                                    i = 0.1m;
                                    break;
                                case 'b':
                                    i = 0.2m;
                                    break;
                                case 'c':
                                    i = 0.3m;
                                    break;
                                case 'd':
                                    i = 0.4m;
                                    break;
                                case 'e':
                                    i = 0.5m;
                                    break;
                                case 'f':
                                    i = 0.6m;
                                    break;
                            }
                        }
                    }
                    minor2 = decimal.Parse(minorString.ToString());

                    minor2 += i;
                }

                if (major1 > major2 || minor1 > minor2)
                {
                    return true;
                }
            }

            return false;
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

    }
}
