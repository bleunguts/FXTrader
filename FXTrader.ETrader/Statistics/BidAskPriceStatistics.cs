using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FXTrader.ETrader.Statistics
{
    public struct BidAskPriceStatistics
    {
        public decimal AverageBidPrice { get; private set; }
        public decimal AverageAskPrice { get; private set; }
        public decimal MinBidPrice { get; private  set; }
        public decimal MinAskPrice { get; private set; }
        public decimal MaxBidPrice { get; private set; }
        public decimal MaxAskPrice { get; private set; }

        public long Count { get; private set; }

        public BidAskPriceStatistics(decimal sumbid, decimal sumask, decimal minbid, decimal minask, decimal maxbid, decimal maxask, long count)  
            : this()
        {
            MinBidPrice = minbid;
            MinAskPrice = minask;
            MaxBidPrice = maxbid;
            MaxAskPrice = maxask;
            Count = count;

            if (count == 0)
            {
                AverageBidPrice = 0;
                AverageAskPrice = 0;
            }
            else
            {
                AverageBidPrice = sumbid / count;
                AverageAskPrice = sumask / count;
            }
        }
     
        private string PrintBidPriceStats()
        {
            return string.Format("Avg={0} Min={1}, Max={2}", AverageBidPrice.ToString("#.00000"), MinBidPrice.ToString("#.00000"), MaxBidPrice.ToString("#.00000"));
        }

        private string PrintAskPriceStats()
        {
            return string.Format("Avg={0} Min={1}, Max={2}", AverageAskPrice.ToString("#.00000"), MinAskPrice.ToString("#.00000"), MaxAskPrice.ToString("#.00000"));
        }

        public override string ToString()
        {
            return string.Format("[Bid] {0}\t[Ask] {1}", PrintBidPriceStats(), PrintAskPriceStats()) ;
        } 
    }
}
