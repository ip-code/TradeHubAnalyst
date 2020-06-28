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
        private bool finishedWithErrors = false;
        private CancellationToken token;
        private int finishedItem;
        private int maxAsync;
        private int totalItems;
        private IProgress<DownloadProgressReportModel> progress;
        private long startTime;
        private int startedItem;

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

            startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

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

                        token.ThrowIfCancellationRequested();

                        int progressPercentage = Convert.ToInt32(((double)i / totalItems) * 100);

                        report = new DownloadProgressReportModel();

                        report.PercentageComplete = progressPercentage;
                        report.MessageRemaining = "Step 2/4: Processing new item list" + StaticMethods.EstimatedTime(startTime, i, totalItems);

                        progress.Report(report);
                    }
                }
            }

            File.Delete(fileNameXls);

            if (allItemList.Any())
            {
                List<ItemModel> itemsDB = SqliteDataAccess.LoadItems();

                List<ItemModel> todoItems = new List<ItemModel>();

                ItemFiltersModel filters = SqliteDataAccess.LoadItemFilters();

                long currentTImestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

                startedItem = 0;
                totalItems = allItemList.Count;
                startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                foreach (List<string> newItem in allItemList)
                {
                    token.ThrowIfCancellationRequested();

                    bool isNewItem = true;

                    foreach (ItemModel item in itemsDB)
                    {
                        if (Int32.Parse(newItem[0]) == item.type_id)
                        {
                            isNewItem = false;

                            long oldTimestamp = item.updated_at + (filters.updated_item_max_age * 24 * 60 * 60);

                            if (currentTImestamp > oldTimestamp)
                            {
                                ItemModel newTodoItem = new ItemModel();
                                newTodoItem.type_id = item.type_id;
                                newTodoItem.name = newItem[1];
                                newTodoItem.volume = Decimal.Parse(newItem[2], CultureInfo.InvariantCulture);

                                todoItems.Add(newTodoItem);
                            }
                        }
                    }

                    if (isNewItem)
                    {
                        ItemModel newTodoItem = new ItemModel();
                        newTodoItem.type_id = Int32.Parse(newItem[0]);
                        newTodoItem.name = newItem[1];
                        newTodoItem.volume = Decimal.Parse(newItem[2], CultureInfo.InvariantCulture);
                        newTodoItem.sell_price = 0;
                        newTodoItem.trade_volume = 0;
                        newTodoItem.updated_at = 0;
                        SqliteDataAccess.SaveItem(newTodoItem);

                        todoItems.Add(newTodoItem);
                    }

                    report = new DownloadProgressReportModel();

                    report.PercentageComplete = Convert.ToInt32(((double)startedItem / totalItems) * 100);
                    report.MessageRemaining = "Step 3/4: Checking items in database" + StaticMethods.EstimatedTime(startTime, startedItem, totalItems);

                    progress.Report(report);

                    startedItem++;
                }

                startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                totalItems = todoItems.Count();

                startedItem = 0;
                finishedItem = 0;
                maxAsync = 0;

                while (startedItem < totalItems)
                {
                    token.ThrowIfCancellationRequested();

                    if (maxAsync < filters.max_async_tasks)
                    {
                        maxAsync++;
                        Task.Run(() => GetAndUpdateItem(todoItems[startedItem]));
                    }

                    Thread.Sleep(100);
                }
            }

            return finishedWithErrors;
        }

        private void GetAndUpdateItem(ItemModel item)
        {
            startedItem++;

            try
            {
                WebClient client = new WebClient();

                string tradeVolumeSource = client.DownloadString("https://api.evemarketer.com/ec/marketstat?typeid=" + item.type_id.ToString());

                client.Dispose();

                XDocument xmlDoc = XDocument.Parse(tradeVolumeSource);

                XElement buy = xmlDoc.Root.Element("marketstat").Element("type").Element("buy").Element("volume");
                long buyVolume = Int64.Parse(buy.Value);

                XElement sell = xmlDoc.Root.Element("marketstat").Element("type").Element("sell");
                long sellVolume = Int64.Parse(sell.Element("volume").Value);

                decimal sellPrice = Decimal.Parse(sell.Element("min").Value, CultureInfo.InvariantCulture);

                ItemModel fullItem = new ItemModel();
                fullItem.type_id = item.type_id;
                fullItem.name = item.name;
                fullItem.volume = item.volume;
                fullItem.sell_price = sellPrice;
                fullItem.trade_volume = buyVolume + sellVolume;
                fullItem.updated_at = DateTimeOffset.Now.ToUnixTimeSeconds();

                SqliteDataAccess.UpdateItem(fullItem);
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
                report.MessageRemaining = "Step 4/4: Downloading new data" + StaticMethods.EstimatedTime(startTime, finishedItem, totalItems);

                progress.Report(report);
            }

            maxAsync--;
        }
    }
}