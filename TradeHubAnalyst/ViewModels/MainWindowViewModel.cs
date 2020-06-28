using System.ComponentModel;
using System.Threading;
using System.Windows;
using TradeHubAnalyst.Libraries;

namespace TradeHubAnalyst.ViewModels
{
    internal class MainWindowViewModel
    {
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // maybe some kind of popup?
        }

        public void CheckVersion()
        {
            bool isOutdated = StaticMethods.hasNewVersion();

            if (isOutdated)
            {
                string newVersion = StaticMethods.getNewVersion();
                string newDownloadLink = StaticMethods.getNewDownloadLink();

                Thread.Sleep(1000);

                MessageBoxResult result = MessageBox.Show("Version " + newVersion + " is available!\nWould you like to download?", "Update!", MessageBoxButton.YesNo);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        System.Diagnostics.Process.Start(newDownloadLink);
                        break;

                    case MessageBoxResult.No:
                        break;
                }
            }
        }
    }
}