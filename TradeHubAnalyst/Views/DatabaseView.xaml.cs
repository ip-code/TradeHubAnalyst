using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TradeHubAnalyst.Libraries;
using TradeHubAnalyst.Models;
using TradeHubAnalyst.ViewModels;

namespace TradeHubAnalyst.Views
{

    public partial class DatabaseView : UserControl
    {
        CancellationTokenSource ctSource;
        public bool isTaskRunning = false;
        private DatabaseViewModel viewModel = new DatabaseViewModel();
        private int stationClickedId;
        private Boolean isStationInEditing = false;
        Progress<DownloadProgressReportModel> progress;
        AsyncUpdateItemDatabase worker;
        long oldTime;
        public DatabaseView()
        {
            DataContext = viewModel;
            InitializeComponent();
            btnRefresh.Focus();
        }

        private async void btnStartWorker_Click(object sender, RoutedEventArgs e)
        {
            if (!isTaskRunning)
            {
                oldTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                isTaskRunning = true;

                pbRefresh.Value = 0;
                btnRefresh.Content = "Cancel";
                tbStatus.Text = "Downloading item list...";
                tbPbMessage.Visibility = Visibility.Hidden;
                pbRefresh.Visibility = Visibility.Visible;
                tbMaxAge.IsEnabled = false;

                ctSource = new CancellationTokenSource();

                progress = new Progress<DownloadProgressReportModel>();
                progress.ProgressChanged += ReportProgress;

                worker = new AsyncUpdateItemDatabase();

                try
                {
                    bool finishedWithErrors = await Task.Run(() => worker.DoWork(progress, ctSource.Token));

                    tbMaxAge.IsEnabled = true;

                    pbRefresh.Value = 0;
                    btnRefresh.Content = "Update items";

                    tbPbMessage.Visibility = Visibility.Visible;
                    pbRefresh.Visibility = Visibility.Hidden;

                    tbStatus.Text = "Total Items: " + SqliteDataAccess.CountItems().ToString();

                    if (finishedWithErrors)
                    {
                        tbPbMessage.Text = "Finished with errors! To correct errors, click on the button again!";
                    }
                    else
                    {
                        tbPbMessage.Text = "If you would like to update item database, please click on the button.";
                    }

                }
                catch (OperationCanceledException)
                {
                    btnRefresh.IsEnabled = true;
                    tbMaxAge.IsEnabled = true;

                    pbRefresh.Value = 0;
                    btnRefresh.Content = "Update items";

                    tbPbMessage.Visibility = Visibility.Visible;
                    pbRefresh.Visibility = Visibility.Hidden;

                    tbStatus.Text = "Total Items: " + SqliteDataAccess.CountItems().ToString();
                    tbPbMessage.Text = "Item update cancelled! To resume or retry, click the button.";
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

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > (oldTime + 2))
            {
                oldTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                tbStatus.Text = e.MessageRemaining;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void btnSaveMaxAge_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SaveMaxAge();
            tbSaveMaxAgeMessage.Opacity = 1;


            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork2;
            worker.RunWorkerAsync();

        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            isStationInEditing = true;
            btnSaveStation.Content = "Save";
            StationModel stationClicked = (StationModel)((Button)e.Source).DataContext;
            viewModel.EditStationButtonClick(stationClicked);
            stationClickedId = stationClicked.id;
            tbStationId.Text = stationClicked.type_id.ToString();
            tbStationName.Text = stationClicked.name.ToString();

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            StationModel stationClicked = (StationModel)((Button)e.Source).DataContext;
            viewModel.DeleteStationButtonClick(stationClicked);
            gridMain.ItemsSource = null;
            gridMain.ItemsSource = viewModel.Stations;
        }

        private void btnSaveStation_Click(object sender, RoutedEventArgs e)
        {
            if (isStationInEditing)
            {
                StationModel stationClicked = new StationModel();
                stationClicked.id = stationClickedId;
                stationClicked.type_id = Int32.Parse(tbStationId.Text);
                stationClicked.name = tbStationName.Text;

                viewModel.EditStationButtonClick(stationClicked);
                isStationInEditing = false;
                btnSaveStation.Content = "Add new";
            }
            else
            {
                StationModel stationClicked = new StationModel();
                stationClicked.type_id = Int32.Parse(tbStationId.Text);
                stationClicked.name = tbStationName.Text;

                viewModel.SaveStationButtonClick(stationClicked);
            }

            tbStationId.Text = "";
            tbStationName.Text = "";

        }

        private void btnSaveFilters_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SaveFilters();
            tbSaveFiltersMessage.Opacity = 1;


            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerAsync();

        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
            int i;

            for (i = 100; i > -1; i--)
            {
                Thread.Sleep(3);
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        tbSaveFiltersMessage.Opacity = ((double)i / 100);
                    });
                }
                catch (Exception) { }

            }
        }

        private void Worker_DoWork2(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
            int i;

            for (i = 100; i > -1; i--)
            {
                Thread.Sleep(3);
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        tbSaveMaxAgeMessage.Opacity = ((double)i / 100);
                    });
                }
                catch (Exception) { }

            }
        }



    }
}
