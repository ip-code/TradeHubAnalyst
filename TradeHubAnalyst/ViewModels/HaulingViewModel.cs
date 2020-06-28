using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using TradeHubAnalyst.Libraries;
using TradeHubAnalyst.Models;

namespace TradeHubAnalyst.ViewModels
{
    public partial class HaulingViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<CalculateTradeModel> trades;
        private int comboBoxSelectedId;

        public event PropertyChangedEventHandler PropertyChanged;

        private string user_cargo_capacity;
        private string user_available_money;
        private bool popupUserCargoCapacity = false;
        private bool popupUserAvailableMoney = false;

        public List<ComboBoxStationModel> ComboBoxStations { get; }

        public HaulingViewModel()
        {
            trades = StaticMethods.LoadEmptyTrades(23);

            ItemFiltersModel filters = SqliteDataAccess.LoadItemFilters();

            if (filters == null)
            {
                filters = StaticMethods.SaveDefaultItemFilterModel();
            }

            user_cargo_capacity = filters.user_cargo_capacity.ToString(CultureInfo.InvariantCulture); ;
            user_available_money = filters.user_available_money.ToString(CultureInfo.InvariantCulture); ;

            List<StationModel> stations = SqliteDataAccess.LoadStations();

            ComboBoxStations = new List<ComboBoxStationModel>();
            ComboBoxStations.Add(new ComboBoxStationModel { id = 0, name = "All stations" });
            comboBoxSelectedId = 0;

            for (var i = 0; i < stations.Count(); i++)
            {
                ComboBoxStationModel newStation = new ComboBoxStationModel();
                newStation.id = stations[i].id;
                newStation.name = stations[i].name;
                ComboBoxStations.Add(newStation);

                if (filters.selected_hauling_station_id == newStation.id)
                {
                    comboBoxSelectedId = newStation.id;
                }
            }
        }

        public void SaveFiltersUponStart()
        {
            if (string.IsNullOrEmpty(user_cargo_capacity))
            {
                user_cargo_capacity = "0";
                OnPropertyChanged("UserCargoCapacity");
            }

            if (string.IsNullOrEmpty(user_available_money))
            {
                user_available_money = "0";
                OnPropertyChanged("UserAvailableMoney");
            }

            ItemFiltersModel newFilters = SqliteDataAccess.LoadItemFilters();
            newFilters.selected_hauling_station_id = comboBoxSelectedId;
            newFilters.user_cargo_capacity = decimal.Parse(user_cargo_capacity, CultureInfo.InvariantCulture);
            newFilters.user_available_money = decimal.Parse(user_available_money, CultureInfo.InvariantCulture);
            SqliteDataAccess.UpdateItemFilters(newFilters);
        }

        public int ComboBoxSelected
        {
            get
            {
                return comboBoxSelectedId;
            }

            set
            {
                comboBoxSelectedId = value;
            }
        }

        public ObservableCollection<CalculateTradeModel> Trades
        {
            get
            {
                return trades;
            }

            set
            {
                trades = value;
                OnPropertyChanged("Trades");
            }
        }

        public string UserCargoCapacity
        {
            get
            {
                return user_cargo_capacity;
            }

            set
            {
                user_cargo_capacity = Regex.Replace(value, "[^0-9.]+", string.Empty);
                OnPropertyChanged("UserCargoCapacity");

                if (!Equals(value, user_cargo_capacity) && !popupUserCargoCapacity)
                {
                    popupUserCargoCapacity = true;

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += Worker_PopupTimeout;
                    worker.RunWorkerAsync("PopupUserCargoCapacity");

                    OnPropertyChanged("PopupUserCargoCapacity");
                }
            }
        }

        public bool PopupUserCargoCapacity
        {
            get
            {
                return popupUserCargoCapacity;
            }
        }

        public string UserAvailableMoney
        {
            get
            {
                return user_available_money;
            }

            set
            {
                user_available_money = Regex.Replace(value, "[^0-9.]+", string.Empty);
                OnPropertyChanged("UserAvailableMoney");

                if (!Equals(value, user_available_money) && !popupUserAvailableMoney)
                {
                    popupUserAvailableMoney = true;

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += Worker_PopupTimeout;
                    worker.RunWorkerAsync("PopupUserAvailableMoney");

                    OnPropertyChanged("PopupUserAvailableMoney");
                }
            }
        }

        public bool PopupUserAvailableMoney
        {
            get
            {
                return popupUserAvailableMoney;
            }
        }

        private void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        private void Worker_PopupTimeout(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);

            string popup = e.Argument as string;

            switch (popup)
            {
                case "PopupUserCargoCapacity":
                    popupUserCargoCapacity = false;
                    break;

                case "PopupUserAvailableMoney":
                    popupUserAvailableMoney = false;
                    break;

                default:
                    break;
            }

            OnPropertyChanged(popup);
        }
    }
}