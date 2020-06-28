using System.Collections.Generic;

namespace TradeHubAnalyst.Models
{
    public class AsyncDownloadResultModel
    {
        public List<FormattedTradesModel> item_list { get; set; }

        public bool finished_with_errors { get; set; }
    }
}