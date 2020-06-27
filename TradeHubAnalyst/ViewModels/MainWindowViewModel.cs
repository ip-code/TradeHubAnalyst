using System.ComponentModel;
using System.Threading;
using System.Windows;
using TradeHubAnalyst.Libraries;

namespace TradeHubAnalyst.ViewModels
{
    class MainWindowViewModel
    {
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {

            // maybe some kind of popup?
        }

        public void CheckVersion()
        {
            bool isOutdated = StaticMethods.checkVersion();

            if (isOutdated)
            {
                Thread.Sleep(1000);

                MessageBoxResult result = MessageBox.Show("New version is available!\nWould you like to download?", "Update!", MessageBoxButton.YesNo);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        System.Diagnostics.Process.Start("http://bretonis.mygamesonline.org");
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
        }
    }
}
