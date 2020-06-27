using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TradeHubAnalyst.Models;

namespace TradeHubAnalyst.Libraries
{
    public class AsyncUpdateItemDatabase
    {

        private int maxAsync;
        private int previouslyDoneItems;

        private int startedItem;
        private int finishedItem;

        private int totalItems;
        private DateTime startTime;
        private CancellationToken token;
        private IProgress<DownloadProgressReportModel> progress;

        private bool finishedWithErrors = false;



        public async Task<bool> DoWork(IProgress<DownloadProgressReportModel> progressSent, CancellationToken cancellationToken)
        {
            DownloadProgressReportModel report;

            token = cancellationToken;
            progress = progressSent;

            string fileName = "invTypes.xls.bz2";

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            WebClient wb = new WebClient();
            wb.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)");
            wb.DownloadFile("https://www.fuzzwork.co.uk/dump/latest/invTypes.xls.bz2", fileName);

            var fileNameXls = Path.GetFileNameWithoutExtension(fileName);

            if (File.Exists(fileNameXls))
            {
                File.Delete(fileNameXls);
            }

            using (Stream fs = File.OpenRead(fileName), output = File.Create(fileNameXls), decompressor = new Ionic.BZip2.BZip2InputStream(fs))
            {
                byte[] buffer = new byte[2048];
                int n;
                while ((n = decompressor.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, n);
                }
            }

            File.Delete(fileName);

            List<List<string>> allItemList = new List<List<string>>();

            startTime = DateTime.Now;

            using (var stream = File.Open(fileNameXls, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {

                    var conf = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    };

                    var dataSet = reader.AsDataSet(conf);

                    var dataTable = dataSet.Tables[0];

                    int totalItems = dataTable.Rows.Count;

                    for (int i = 0; i < totalItems; i++)
                    {
                        List<string> singleRow = new List<string>();

                        singleRow.Add(dataTable.Rows[i][0].ToString());
                        singleRow.Add(dataTable.Rows[i][2].ToString());

                        string itemVolumeLocal = dataTable.Rows[i][5].ToString();
                        string itemVolumeInternational = itemVolumeLocal.Replace(',', '.');
                        singleRow.Add(itemVolumeInternational);

                        allItemList.Add(singleRow);

                        /* Trebalo pregaziti volumene zbog zareza
                        ItemModel newItem = SqliteDataAccess.LoadSingleItem(dataTable.Rows[i][0].ToString());
                        newItem.name = dataTable.Rows[i][2].ToString();
                        newItem.volume = Decimal.Parse(itemVolumeInternational, CultureInfo.InvariantCulture);
                        SqliteDataAccess.UpdateItems(newItem);
                        */

                        token.ThrowIfCancellationRequested();

                        int progressPercentage = Convert.ToInt32(((double)i / totalItems) * 100);

                        report = new DownloadProgressReportModel();

                        report.PercentageComplete = progressPercentage;
                        report.MessageRemaining = "Processing" + StaticMethods.EstimatedTime(startTime, i, totalItems);

                        progress.Report(report);
                    }
                }
            }

            File.Delete(fileNameXls);


            if (allItemList.Any())
            {

                ItemFiltersModel filters = SqliteDataAccess.LoadItemFilters();

                long currentTImestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                startTime = DateTime.Now;

                report = new DownloadProgressReportModel();

                report.PercentageComplete = 0;
                report.MessageRemaining = "Downloading data... ";

                progress.Report(report);

                previouslyDoneItems = 0;

                bool isFirstNew = true;

                totalItems = allItemList.Count();

                startedItem = 0;
                finishedItem = 0;
                maxAsync = 0;

                while (startedItem < totalItems)
                {

                    token.ThrowIfCancellationRequested();

                    List<string> item = allItemList[startedItem];

                    ItemModel existingItem = SqliteDataAccess.LoadSingleItem(item[0]);

                    long oldTimestamp;

                    if (existingItem == null)
                    {
                        ItemModel fullItem = new ItemModel();
                        fullItem.type_id = Int32.Parse(item[0]);
                        fullItem.name = item[1];
                        fullItem.volume = Decimal.Parse(item[2], CultureInfo.InvariantCulture);
                        fullItem.sell_price = 0;
                        fullItem.trade_volume = 0;
                        fullItem.updated_at = 0;
                        SqliteDataAccess.SaveItem(fullItem);

                        oldTimestamp = fullItem.updated_at + (filters.updated_item_max_age * 24 * 60 * 60);
                    }
                    else
                    {
                        oldTimestamp = existingItem.updated_at + (filters.updated_item_max_age * 24 * 60 * 60);
                    }


                    if (currentTImestamp > oldTimestamp)
                    {
                        if (isFirstNew)
                        {
                            previouslyDoneItems = startedItem;
                            startTime = DateTime.Now;
                            isFirstNew = false;
                        }

                        if (maxAsync < filters.max_async_tasks)
                        {
                            maxAsync++;
                            Task.Run(() => GetAndUpdateItem(item));
                        }

                        Thread.Sleep(100);

                    }
                    else
                    {
                        startedItem++;
                    }

                }
            }

            return finishedWithErrors;
        }

        private void GetAndUpdateItem(List<string> item)
        {
            startedItem++;

            try
            {
                WebClient client = new WebClient();

                string tradeVolumeSource = client.DownloadString("https://api.evemarketer.com/ec/marketstat?typeid=" + item[0]);

                client.Dispose();

                XDocument xmlDoc = XDocument.Parse(tradeVolumeSource);


                XElement buy = xmlDoc.Root.Element("marketstat").Element("type").Element("buy").Element("volume");
                long buyVolume = Int64.Parse(buy.Value);

                XElement sell = xmlDoc.Root.Element("marketstat").Element("type").Element("sell");
                long sellVolume = Int64.Parse(sell.Element("volume").Value);

                decimal sellPrice = Decimal.Parse(sell.Element("min").Value, CultureInfo.InvariantCulture);

                ItemModel fullItem = new ItemModel();
                fullItem.type_id = Int32.Parse(item[0]);
                fullItem.name = item[1];
                fullItem.volume = Decimal.Parse(item[2], CultureInfo.InvariantCulture);
                fullItem.sell_price = sellPrice;
                fullItem.trade_volume = buyVolume + sellVolume;
                fullItem.updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                SqliteDataAccess.UpdateItem(fullItem);

            }
            catch (Exception)
            {
                finishedWithErrors = true;
            }

            finishedItem++;

            if (!token.IsCancellationRequested)
            {
                int progressPercentage = Convert.ToInt32(((double)(finishedItem + previouslyDoneItems) / totalItems) * 100);

                DownloadProgressReportModel report = new DownloadProgressReportModel();

                report.PercentageComplete = progressPercentage;
                report.MessageRemaining = "Downloading data" + StaticMethods.EstimatedTime(startTime, finishedItem, (totalItems - previouslyDoneItems));

                progress.Report(report);
            }

            maxAsync--;
        }

    }
}
