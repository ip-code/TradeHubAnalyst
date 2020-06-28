using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using TradeHubAnalyst.Libraries;
using TradeHubAnalyst.Models;

namespace TradeHubAnalyst.ViewModels
{
    public partial class DatabaseViewModel : INotifyPropertyChanged
    {
        //private ObservableCollection<StationModel> stationCollection;
        private List<StationModel> stations;

        private string max_volume;
        private string max_price;
        private string min_trade_volume;

        public event PropertyChangedEventHandler PropertyChanged;

        private string newStationId = "";
        private string newStationname = "";
        private bool ignore_zero = false;
        private string filtered_items;
        private string max_age;
        private bool popupMaxAge = false;
        private bool popupStationId = false;
        private bool popupMaxVolume = false;
        private bool popupMaxPrice = false;
        private bool popupMinTadeVolume = false;

        private string max_async_tasks;
        private bool popupMaxAsyncTasks = false;

        public DatabaseViewModel()
        {
            LoadAllItemFilters();
        }

        private void LoadAllStations()
        {
            stations = SqliteDataAccess.LoadStations();

            bool isEmpty = !stations.Any();
            if (isEmpty)
            {
                StationModel seedStations = new StationModel();

                seedStations.type_id = 60003760;
                seedStations.name = "Jita IV - Moon 4 - Caldari Navy Assembly Plant";
                SqliteDataAccess.SaveStation(seedStations);

                seedStations.type_id = 60008494;
                seedStations.name = "Amarr VIII (Oris) - Emperor Family Academy";
                SqliteDataAccess.SaveStation(seedStations);

                seedStations.type_id = 60011866;
                seedStations.name = "Dodixie IX - Moon 20 - Federation Navy Assembly Plant";
                SqliteDataAccess.SaveStation(seedStations);

                seedStations.type_id = 60004588;
                seedStations.name = "Rens VI - Moon 8 - Brutor Tribe Treasury";
                SqliteDataAccess.SaveStation(seedStations);

                seedStations.type_id = 60005686;
                seedStations.name = "Hek VIII - Moon 12 - Boundless Creation Factory";
                SqliteDataAccess.SaveStation(seedStations);

                stations = SqliteDataAccess.LoadStations();
            }
        }

        private void LoadAllItemFilters()
        {
            ItemFiltersModel filters = SqliteDataAccess.LoadItemFilters();
            max_volume = filters.max_volume.ToString(CultureInfo.InvariantCulture);
            max_price = filters.max_price.ToString(CultureInfo.InvariantCulture);
            min_trade_volume = filters.min_trade_volume.ToString();
            max_async_tasks = filters.max_async_tasks.ToString();

            if (filters.ignore_zero > 0)
            {
                ignore_zero = true;
            }

            filtered_items = filters.filtered_items.ToString();

            max_age = filters.updated_item_max_age.ToString();
        }

        public string ItemCount
        {
            get
            {
                return "Total Items: " + SqliteDataAccess.CountItems().ToString();
            }
        }

        public string MaxAge
        {
            get
            {
                return max_age;
            }

            set
            {
                max_age = Regex.Replace(value, "[^0-9]+", string.Empty);
                OnPropertyChanged("MaxAge");

                if (!Equals(value, max_age) && !popupMaxAge)
                {
                    popupMaxAge = true;

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += Worker_PopupTimeout;
                    worker.RunWorkerAsync("PopupMaxAge");

                    OnPropertyChanged("PopupMaxAge");
                }
            }
        }

        public bool PopupMaxAge
        {
            get
            {
                return popupMaxAge;
            }
        }

        public List<StationModel> Stations
        {
            get
            {
                LoadAllStations();
                return stations;
            }
        }

        public string NewStationId
        {
            get
            {
                return newStationId;
            }

            set
            {
                newStationId = Regex.Replace(value, "[^0-9]+", string.Empty);
                OnPropertyChanged("NewStationId");

                if (!Equals(value, newStationId) && !popupStationId)
                {
                    popupStationId = true;

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += Worker_PopupTimeout;
                    worker.RunWorkerAsync("PopupStationId");

                    OnPropertyChanged("PopupStationId");
                }
            }
        }

        public bool PopupStationId
        {
            get
            {
                return popupStationId;
            }
        }

        public string NewStationName
        {
            get
            {
                return newStationname;
            }

            set
            {
                newStationname = Regex.Replace(value, "[^\x0d\x0a\x20-\x7e\t]", string.Empty);
                newStationname = newStationname.Trim();
                OnPropertyChanged("NewStationName");
            }
        }

        public string MaxVolume
        {
            get
            {
                return max_volume;
            }

            set
            {
                max_volume = Regex.Replace(value, "[^0-9.]+", string.Empty);
                OnPropertyChanged("MaxVolume");

                if (!Equals(value, max_volume) && !popupMaxVolume)
                {
                    popupMaxVolume = true;

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += Worker_PopupTimeout;
                    worker.RunWorkerAsync("PopupMaxVolume");

                    OnPropertyChanged("PopupMaxVolume");
                }
            }
        }

        public bool PopupMaxVolume
        {
            get
            {
                return popupMaxVolume;
            }
        }

        public string MaxPrice
        {
            get
            {
                return max_price;
            }

            set
            {
                max_price = Regex.Replace(value, "[^0-9.]+", string.Empty);
                OnPropertyChanged("MaxPrice");

                if (!Equals(value, max_price) && !popupMaxPrice)
                {
                    popupMaxPrice = true;

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += Worker_PopupTimeout;
                    worker.RunWorkerAsync("PopupMaxPrice");

                    OnPropertyChanged("PopupMaxPrice");
                }
            }
        }

        public bool PopupMaxPrice
        {
            get
            {
                return popupMaxPrice;
            }
        }

        public string MinTradeVolume
        {
            get
            {
                return min_trade_volume;
            }

            set
            {
                min_trade_volume = Regex.Replace(value, "[^0-9]+", string.Empty);
                OnPropertyChanged("MinTradeVolume");

                if (!Equals(value, min_trade_volume) && !popupMinTadeVolume)
                {
                    popupMinTadeVolume = true;

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += Worker_PopupTimeout;
                    worker.RunWorkerAsync("PopupMinTradeVolume");

                    OnPropertyChanged("PopupMinTradeVolume");
                }
            }
        }

        public bool PopupMinTradeVolume
        {
            get
            {
                return popupMinTadeVolume;
            }
        }

        public string MaxAsyncTasks
        {
            get
            {
                return max_async_tasks;
            }

            set
            {
                max_async_tasks = Regex.Replace(value, "[^0-9]+", string.Empty);
                OnPropertyChanged("MaxAsyncTasks");

                if (!Equals(value, max_async_tasks) && !popupMaxAsyncTasks)
                {
                    popupMaxAsyncTasks = true;

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += Worker_PopupTimeout;
                    worker.RunWorkerAsync("PopupMaxAsyncTasks");

                    OnPropertyChanged("PopupMaxAsyncTasks");
                }
            }
        }

        public bool PopupMaxAsyncTasks
        {
            get
            {
                return popupMaxAsyncTasks;
            }
        }

        public bool IgnoreZeroValues
        {
            get
            {
                return ignore_zero;
            }

            set
            {
                ignore_zero = value;
                OnPropertyChanged("IgnoreZeroValues");
            }
        }

        public string FilteredItemCount
        {
            get
            {
                return "Total Filtered Items: " + filtered_items;
            }
        }

        public void SaveMaxAge()
        {
            if (string.IsNullOrEmpty(max_age))
            {
                max_age = "0";
                OnPropertyChanged("MaxAge");
            }

            ItemFiltersModel filters = SqliteDataAccess.LoadItemFilters();
            filters.updated_item_max_age = Int32.Parse(max_age);
            SqliteDataAccess.UpdateItemFilters(filters);
        }

        public void SaveFilters()
        {
            if (string.IsNullOrEmpty(max_volume))
            {
                max_volume = "0";
                OnPropertyChanged("MaxVolume");
            }

            if (string.IsNullOrEmpty(max_price))
            {
                max_price = "0";
                OnPropertyChanged("MaxPrice");
            }

            if (string.IsNullOrEmpty(min_trade_volume))
            {
                min_trade_volume = "0";
                OnPropertyChanged("MinTradeVolume");
            }

            if (string.IsNullOrEmpty(max_async_tasks))
            {
                max_async_tasks = "5";
                OnPropertyChanged("MaxAsyncTasks");
            }

            ItemFiltersModel filters = SqliteDataAccess.LoadItemFilters();
            filters.max_volume = Decimal.Parse(max_volume, CultureInfo.InvariantCulture);
            filters.max_price = Decimal.Parse(max_price, CultureInfo.InvariantCulture);
            filters.min_trade_volume = Int32.Parse(min_trade_volume);
            filters.max_async_tasks = Int32.Parse(max_async_tasks);

            if (ignore_zero)
            {
                filters.ignore_zero = 1;
            }
            else
            {
                filters.ignore_zero = 0;
            }

            filtered_items = CountFilteredItems(filters);

            filters.filtered_items = Int32.Parse(filtered_items);

            SqliteDataAccess.UpdateItemFilters(filters);

            OnPropertyChanged("FilteredItemCount");
        }

        private string CountFilteredItems(ItemFiltersModel filters)
        {
            List<ItemModel> items = SqliteDataAccess.LoadItems();

            int validItems = 0;

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

                if (filters.max_volume > 0)
                {
                    if (item.volume < filters.max_volume)
                    {
                        isFilterVolumePassed = true;
                    }
                }
                else
                {
                    isFilterVolumePassed = true;
                }

                if (filters.max_price > 0)
                {
                    if (item.sell_price < filters.max_price)
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
                    validItems++;
                }
            }

            return validItems.ToString();
        }

        private void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        public void DeleteStationButtonClick(StationModel stationClicked)
        {
            SqliteDataAccess.DeleteStation(stationClicked);
            OnPropertyChanged("Stations");
        }

        public void EditStationButtonClick(StationModel stationClicked)
        {
            SqliteDataAccess.UpdateStation(stationClicked);
            OnPropertyChanged("Stations");
        }

        public void SaveStationButtonClick(StationModel stationClicked)
        {
            SqliteDataAccess.SaveStation(stationClicked);
            OnPropertyChanged("Stations");
        }

        private void Worker_PopupTimeout(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);

            string popup = e.Argument as string;

            switch (popup)
            {
                case "PopupMaxAge":
                    popupMaxAge = false;
                    break;

                case "PopupStationId":
                    popupStationId = false;
                    break;

                case "PopupMaxVolume":
                    popupMaxVolume = false;
                    break;

                case "PopupMaxPrice":
                    popupMaxPrice = false;
                    break;

                case "PopupMinTradeVolume":
                    popupMinTadeVolume = false;
                    break;

                case "PopupMaxAsyncTasks":
                    popupMaxAsyncTasks = false;
                    break;

                default:
                    break;
            }

            OnPropertyChanged(popup);
        }
    }
}