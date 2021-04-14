using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FXTrader.Common;
using System.Threading;

namespace FXTrader.FXPricer
{
    public class FXPricer
    {     
        public void PriceTrade(ProductType productType, BuySell buySell, int quantity, SpotPrice price, out decimal purchasePrice, out decimal totalPrice)
        {
            purchasePrice = buySell == BuySell.Buy ? price.AskPrice : price.BidPrice;
            totalPrice = purchasePrice * quantity;
        }

        public SpotPrice Price(string ccyPair, SpotPrice spotPrice, int spreadInPips)
        {
            // potential complex calculations go here... 
            // this may be a long running operation and may need to talk to external services, run on grids, or use lots of threads.
            if (Constants.PriceLongOperationTimeInMilliseconds > 0) Thread.Sleep(TimeSpan.FromMilliseconds(Constants.PriceLongOperationTimeInMilliseconds));

            var spreadInDecimal = spreadInPips / 10000M;

            var rawSpread = spotPrice.AskPrice - spotPrice.BidPrice;
            var rawdifference = rawSpread / 2;
            var rawMid = spotPrice.BidPrice + rawdifference;
            var finalSpread = rawSpread + spreadInDecimal;
            var finaldifference = finalSpread / 2;

            var bidPrice = rawMid - finaldifference;
            var askPrice = rawMid + finaldifference;

            return new SpotPrice(ccyPair, bidPrice, askPrice);                        
        }
    }
}
