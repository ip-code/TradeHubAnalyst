using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using TradeHubAnalyst.Libraries;
using TradeHubAnalyst.Models;
using TradeHubAnalyst.ViewModels;

namespace TradeHubAnalyst.Views
{
    public partial class StationTradingView : UserControl
    {
        private StationTradingViewModel viewModel = new StationTradingViewModel();
        private CancellationTokenSource ctSource;
        public bool isTaskRunning = false;
        private AsyncDownloadWebsites downloadWorker;
        private AsyncCalculateStationTrading calculateWorker;
        private Progress<DownloadProgressReportModel> progress;
        private long oldTime;

        public StationTradingView()
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private async void btnStartWorker_Click(object sender, RoutedEventArgs e)
        {
            if (!isTaskRunning)
            {
                oldTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                isTaskRunning = true;

                pbRefresh.Value = 0;
                tbStatus.Text = "Step 1/2: Initializing refresh...";
                btnRefresh.Content = "Cancel";

                viewModel.SaveFiltersUponStart();
                gridMain.IsEnabled = false;
                cbStations.IsEnabled = false;
                tbBrokersFee.IsEnabled = false;
                tbUserSalesTax.IsEnabled = false;

                ctSource = new CancellationTokenSource();

                downloadWorker = new AsyncDownloadWebsites();
                calculateWorker = new AsyncCalculateStationTrading();

                progress = new Progress<DownloadProgressReportModel>();
                progress.ProgressChanged += ReportProgress;

                try
                {
                    AsyncDownloadResultModel downloadedData = await Task.Run(() => downloadWorker.DoWork(progress, ctSource.Token));
                    ObservableCollection<CalculateStationTradeModel> trades = await Task.Run(() => calculateWorker.DoWork(progress, ctSource.Token, downloadedData.item_list));

                    updateTable(trades);

                    cbStations.IsEnabled = true;
                    tbBrokersFee.IsEnabled = true;
                    tbUserSalesTax.IsEnabled = true;

                    pbRefresh.Value = 0;
                    btnRefresh.Content = "Refresh data";

                    if (downloadedData.finished_with_errors)
                    {
                        tbStatus.Text = "Trades refreshed with some errors on " + DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
                    }
                    else
                    {
                        tbStatus.Text = "Trades refreshed on " + DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
                    }
                }
                catch (OperationCanceledException)
                {
                    gridMain.IsEnabled = true;
                    btnRefresh.IsEnabled = true;
                    cbStations.IsEnabled = true;
                    tbBrokersFee.IsEnabled = true;
                    tbUserSalesTax.IsEnabled = true;

                    pbRefresh.Value = 0;
                    btnRefresh.Content = "Refresh data";
                    tbStatus.Text = "Refresh cancelled. To begin, please refresh data.";
                }

                isTaskRunning = false;
            }
            else
            {
                isTaskRunning = false;
                btnRefresh.IsEnabled = false;
                ctSource.Cancel();
            }
        }

        private void ReportProgress(object sender, DownloadProgressReportModel e)
        {
            pbRefresh.Value = e.PercentageComplete;

            if (DateTimeOffset.Now.ToUnixTimeSeconds() > (oldTime + 2))
            {
                oldTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                tbStatus.Text = e.MessageRemaining;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void gridMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        public void updateTable(ObservableCollection<CalculateStationTradeModel> trades)
        {
            if (!gridMain.IsEnabled)
            {
                gridMain.IsEnabled = true;
            }

            List<CalculateStationTradeModel> SortedList = trades.OrderByDescending(o => o.spread_percent).ToList();

            while (SortedList.Count < 23)
            {
                SortedList.Add(new CalculateStationTradeModel());
            }

            viewModel.Trades = new ObservableCollection<CalculateStationTradeModel>(SortedList as List<CalculateStationTradeModel>);
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            DataGrid dg = sender as DataGrid;
            ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(dg.ItemsSource);

            if (e.Column.SortMemberPath == "item")
            {
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
                e.Column.SortDirection = direction;

                if (direction == ListSortDirection.Ascending)
                {
                    lcv.CustomSort = new StationTradingDataGridComparer("item", 0);
                }
                else
                {
                    lcv.CustomSort = new StationTradingDataGridComparer("item", 1);
                }
            }
            else if (e.Column.SortMemberPath == "buy_price")
            {
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Descending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                e.Column.SortDirection = direction;

                if (direction == ListSortDirection.Ascending)
                {
                    lcv.CustomSort = new StationTradingDataGridComparer("buy_price", 0);
                }
                else
                {
                    lcv.CustomSort = new StationTradingDataGridComparer("buy_price", 1);
                }
            }
            else if (e.Column.SortMemberPath == "sell_price")
            {
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Descending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                e.Column.SortDirection = direction;

                if (direction == ListSortDirection.Ascending)
                {
                    lcv.CustomSort = new StationTradingDataGridComparer("sell_price", 0);
                }
                else
                {
                    lcv.CustomSort = new StationTradingDataGridComparer("sell_price", 1);
                }
            }
            else if (e.Column.SortMemberPath == "spread_isk")
            {
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Descending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                e.Column.SortDirection = direction;

                if (direction == ListSortDirection.Ascending)
                {
                    lcv.CustomSort = new StationTradingDataGridComparer("spread_isk", 0);
                }
                else
                {
                    lcv.CustomSort = new StationTradingDataGridComparer("spread_isk", 1);
                }
            }
            else if (e.Column.SortMemberPath == "spread_percent")
            {
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Descending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                e.Column.SortDirection = direction;

                if (direction == ListSortDirection.Ascending)
                {
                    lcv.CustomSort = new StationTradingDataGridComparer("spread_percent", 0);
                }
                else
                {
                    lcv.CustomSort = new StationTradingDataGridComparer("spread_percent", 1);
                }
            }
        }
    }
}