using System.Threading.Tasks;
using System.Windows;
using TradeHubAnalyst.ViewModels;

namespace TradeHubAnalyst
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MainWindowViewModel viewModel = new MainWindowViewModel();

            Title = "TradeHubAnalyst " + Properties.Resources.Version;
            InitializeComponent();

            Task.Run(() => viewModel.CheckVersion());

            Closing += viewModel.OnWindowClosing;
        }
    }
}
