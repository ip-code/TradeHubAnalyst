using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeHubAnalyst.Models;

namespace TradeHubAnalyst.Libraries
{
    public class AsyncDownloadWebsites
    {
        private IProgress<DownloadProgressReportModel> progress;
        private int startedItem;
        private int finishedItem;
        private int maxAsync;
        private int totalItems;
        private long startTime;
        private CancellationToken token;
        private bool finishedWithErrors = false;

        public async Task<AsyncDownloadResultModel> DoWork(IProgress<DownloadProgressReportModel> progressSent, CancellationToken cancellationToken)
        {
            List<FormattedTradesModel> resultData = new List<FormattedTradesModel>();

            List<string[]> filteredItems = StaticMethods.GetFilteredItems();

            progress = progressSent;

            token = cancellationToken;

            ItemFiltersModel filters = SqliteDataAccess.LoadItemFilters();

            totalItems = filteredItems.Count;

            startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            startedItem = 0;
            finishedItem = 0;
            maxAsync = 0;

            List<Task<FormattedTradesModel>> tasks = new List<Task<FormattedTradesModel>>();

            while (startedItem < totalItems)
            {
                token.ThrowIfCancellationRequested();

                if (maxAsync < filters.max_async_tasks)
                {
                    maxAsync++;
                    tasks.Add(Task.Run(() => downloadSite(filteredItems[startedItem])));
                }

                Thread.Sleep(10);
            }

            FormattedTradesModel[] results = await Task.WhenAll(tasks);

            foreach (FormattedTradesModel model in results)
            {
                if (model.has_data)
                {
                    resultData.Add(model);
                }
            }

            AsyncDownloadResultModel result = new AsyncDownloadResultModel();
            result.item_list = resultData;
            result.finished_with_errors = finishedWithErrors;

            return result;
        }

        private FormattedTradesModel downloadSite(string[] item)
        {
            startedItem++;

            FormattedTradesModel result = new FormattedTradesModel();
            result.type_id = Convert.ToInt32(item[0]);
            result.has_data = false;

            try
            {
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;

                var options = new ChromeOptions();
                options.AddArgument("headless");
                options.AddArgument("--no-sandbox");
                options.Proxy = null;

                IWebDriver driver = new ChromeDriver(service, options, TimeSpan.FromMinutes(5));

                driver.Url = "https://evemarketer.com/types/" + item[0];

                new WebDriverWait(driver, TimeSpan.FromSeconds(60)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementWithText(By.TagName("h2"), "No Type Selected"));

                if (!string.IsNullOrEmpty(driver.PageSource))
                {
                    ParseDownloadedString parseDownload = new ParseDownloadedString();
                    result = parseDownload.Trades(item[0], driver.PageSource);
                }

                driver.Quit();
            }
            catch (Exception)
            {
                finishedWithErrors = true;
            }

            finishedItem++;

            if (!token.IsCancellationRequested)
            {
                int progressPercentage = Convert.ToInt32(((double)finishedItem / totalItems) * 100);

                DownloadProgressReportModel report = new DownloadProgressReportModel();

                report.PercentageComplete = progressPercentage;
                report.MessageRemaining = "Step 1/2: Downloading data" + StaticMethods.EstimatedTime(startTime, finishedItem, totalItems);

                progress.Report(report);
            }

            maxAsync--;

            return result;
        }
    }
}