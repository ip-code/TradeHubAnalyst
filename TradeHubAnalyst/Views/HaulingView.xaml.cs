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
    public partial class HaulingView : UserControl
    {
        private CancellationTokenSource ctSource;
        public bool isTaskRunning = false;
        private HaulingViewModel viewModel = new HaulingViewModel();
        private AsyncDownloadWebsites downloadWorker;
        private AsyncCalculateHauling calculateWorker;
        private Progress<DownloadProgressReportModel> progress;
        private long oldTime;

        public HaulingView()
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
                tbUserargoCapacity.IsEnabled = false;
                tbUserAvailableMoney.IsEnabled = false;

                ctSource = new CancellationTokenSource();

                downloadWorker = new AsyncDownloadWebsites();
                calculateWorker = new AsyncCalculateHauling();

                progress = new Progress<DownloadProgressReportModel>();
                progress.ProgressChanged += ReportProgress;

                try
                {
                    AsyncDownloadResultModel downloadedData = await Task.Run(() => downloadWorker.DoWork(progress, ctSource.Token));
                    ObservableCollection<CalculateTradeModel> trades = await Task.Run(() => calculateWorker.DoWork(progress, ctSource.Token, downloadedData.item_list));

                    updateTable(trades);

                    cbStations.IsEnabled = true;
                    tbUserargoCapacity.IsEnabled = true;
                    tbUserAvailableMoney.IsEnabled = true;

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
                    tbUserargoCapacity.IsEnabled = true;
                    tbUserAvailableMoney.IsEnabled = true;

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

        public void updateTable(ObservableCollection<CalculateTradeModel> trades)
        {
            if (!gridMain.IsEnabled)
            {
                gridMain.IsEnabled = true;
            }

            List<CalculateTradeModel> SortedList = trades.OrderByDescending(o => o.Profit).ToList();
            SortedList = SortedList.OrderByDescending(o => o.To).ToList();
            SortedList = SortedList.OrderByDescending(o => o.From).ToList();

            while (SortedList.Count < 23)
            {
                SortedList.Add(new CalculateTradeModel());
            }

            viewModel.Trades = new ObservableCollection<CalculateTradeModel>(SortedList as List<CalculateTradeModel>);
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            DataGrid dg = sender as DataGrid;
            ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(dg.ItemsSource);

            if (e.Column.SortMemberPath == "Item")
            {
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
                e.Column.SortDirection = direction;

                if (direction == ListSortDirection.Ascending)
                {
                    lcv.CustomSort = new HaulingDataGridComparer("Item", 0);
                }
                else
                {
                    lcv.CustomSort = new HaulingDataGridComparer("Item", 1);
                }
            }
            else if (e.Column.SortMemberPath == "From")
            {
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
                e.Column.SortDirection = direction;

                if (direction == ListSortDirection.Ascending)
                {
                    lcv.CustomSort = new HaulingDataGridComparer("From", 0);
                }
                else
                {
                    lcv.CustomSort = new HaulingDataGridComparer("From", 1);
                }
            }
            else if (e.Column.SortMemberPath == "To")
            {
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
                e.Column.SortDirection = direction;

                if (direction == ListSortDirection.Ascending)
                {
                    lcv.CustomSort = new HaulingDataGridComparer("To", 0);
                }
                else
                {
                    lcv.CustomSort = new HaulingDataGridComparer("To", 1);
                }
            }
            else if (e.Column.SortMemberPath == "NumItems")
            {
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Descending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                e.Column.SortDirection = direction;

                if (direction == ListSortDirection.Ascending)
                {
                    lcv.CustomSort = new HaulingDataGridComparer("NumItems", 0);
                }
                else
                {
                    lcv.CustomSort = new HaulingDataGridComparer("NumItems", 1);
                }
            }
            else if (e.Column.SortMemberPath == "Price")
            {
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Descending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                e.Column.SortDirection = direction;

                if (direction == ListSortDirection.Ascending)
                {
                    lcv.CustomSort = new HaulingDataGridComparer("Price", 0);
                }
                else
                {
                    lcv.CustomSort = new HaulingDataGridComparer("Price", 1);
                }
            }
            else if (e.Column.SortMemberPath == "Profit")
            {
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Descending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                e.Column.SortDirection = direction;

                if (direction == ListSortDirection.Ascending)
                {
                    lcv.CustomSort = new HaulingDataGridComparer("Profit", 0);
                }
                else
                {
                    lcv.CustomSort = new HaulingDataGridComparer("Profit", 1);
                }
            }
            else if (e.Column.SortMemberPath == "ROI")
            {
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Descending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                e.Column.SortDirection = direction;

                if (direction == ListSortDirection.Ascending)
                {
                    lcv.CustomSort = new HaulingDataGridComparer("ROI", 0);
                }
                else
                {
                    lcv.CustomSort = new HaulingDataGridComparer("ROI", 1);
                }
            }
        }
    }
}