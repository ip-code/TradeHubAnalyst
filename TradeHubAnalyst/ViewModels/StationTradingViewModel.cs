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
    public partial class StationTradingViewModel : INotifyPropertyChanged
    {
        ObservableCollection<CalculateStationTradeModel> trades;
        private int comboBoxSelectedId;
        public event PropertyChangedEventHandler PropertyChanged;
        private string user_brokers_fee;
        private string user_sales_tax;
        private bool popupUserBrokersFee = false;
        private bool popupUserSalesTax = false;

        public List<ComboBoxStationModel> ComboBoxStations { get; }

        public StationTradingViewModel()
        {
            trades = StaticMethods.LoadEmptyStationTrades(23);

            ItemFiltersModel filters = SqliteDataAccess.LoadItemFilters();

            user_brokers_fee = filters.user_brokers_fee.ToString(CultureInfo.InvariantCulture); ;
            user_sales_tax = filters.user_sales_tax.ToString(CultureInfo.InvariantCulture); ;

            List<StationModel> stations = SqliteDataAccess.LoadStations();

            ComboBoxStations = new List<ComboBoxStationModel>();

            for (var i = 0; i < stations.Count(); i++)
            {
                ComboBoxStationModel newStation = new ComboBoxStationModel();
                newStation.id = stations[i].id;
                newStation.name = stations[i].name;
                ComboBoxStations.Add(newStation);

                if (i == 0)
                {
                    comboBoxSelectedId = 0;
                }

                if (filters.selected_station_trading_station_id == newStation.id)
                {
                    comboBoxSelectedId = newStation.id;
                }
            }

        }

        public void SaveFiltersUponStart()
        {
            if (string.IsNullOrEmpty(user_brokers_fee))
            {
                user_brokers_fee = "5";
                OnPropertyChanged("UserBrokersFee");
            }

            if (string.IsNullOrEmpty(user_sales_tax))
            {
                user_sales_tax = "5";
                OnPropertyChanged("UserSalesTax");
            }

            ItemFiltersModel newFilters = SqliteDataAccess.LoadItemFilters();
            newFilters.selected_station_trading_station_id = comboBoxSelectedId;
            newFilters.user_brokers_fee = decimal.Parse(user_brokers_fee, CultureInfo.InvariantCulture);
            newFilters.user_sales_tax = decimal.Parse(user_sales_tax, CultureInfo.InvariantCulture);
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

        public ObservableCollection<CalculateStationTradeModel> Trades
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

        public string UserBrokersFee
        {
            get
            {
                return user_brokers_fee;
            }

            set
            {
                user_brokers_fee = Regex.Replace(value, "[^0-9.]+", string.Empty);
                OnPropertyChanged("UserBrokersFee");

                if (!Equals(value, user_brokers_fee) && !popupUserBrokersFee)
                {
                    popupUserBrokersFee = true;

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += Worker_PopupTimeout;
                    worker.RunWorkerAsync("PopupUserBrokersFee");

                    OnPropertyChanged("PopupUserBrokersFee");
                }
            }
        }

        public bool PopupUserBrokersFee
        {
            get
            {
                return popupUserBrokersFee;
            }
        }

        public string UserSalesTax
        {
            get
            {
                return user_sales_tax;
            }

            set
            {
                user_sales_tax = Regex.Replace(value, "[^0-9.]+", string.Empty);
                OnPropertyChanged("UserSalesTax");

                if (!Equals(value, user_sales_tax) && !popupUserSalesTax)
                {
                    popupUserSalesTax = true;

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += Worker_PopupTimeout;
                    worker.RunWorkerAsync("PopupUserSalesTax");

                    OnPropertyChanged("PopupUserSalesTax");
                }
            }
        }

        public bool PopupUserSalesTax
        {
            get
            {
                return popupUserSalesTax;
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
                case "PopupUserBrokersFee":
                    popupUserBrokersFee = false;
                    break;

                case "PopupUserSalesTax":
                    popupUserSalesTax = false;
                    break;

                default:
                    break;
            }

            OnPropertyChanged(popup);
        }
    }
}
