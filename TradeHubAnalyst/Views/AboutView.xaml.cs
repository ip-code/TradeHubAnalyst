﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TradeHubAnalyst.Libraries;

namespace TradeHubAnalyst.Views
{
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
            bool isOutdated = StaticMethods.hasNewVersion();

            if (isOutdated)
            {
                string newVersion = StaticMethods.getNewVersion();
                string newDownloadLink = StaticMethods.getNewDownloadLink();

                hlVersion.IsEnabled = true;
                hlVersion.NavigateUri = new Uri(newDownloadLink);
                hlVersion.TextDecorations = TextDecorations.Underline;
                tbVerDescription.Text = "Version " + newVersion + " is available for download!";
            }

            tbVersion.Text = Properties.Resources.Version;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}