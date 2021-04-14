using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using FXTrader.Common;
using FXTrader.ETrader.Config;

namespace FXTrader.ETrader.Statistics
{
    class MovingStatisticsComposer
    {               
        public static IObservable<BidAskPriceStatistics> GetPriceStatisticsObservable(IObservable<SpotPrice> spotPriceStream)
        {           
            return spotPriceStream.Scan(new { bidsum = .0M, 
                                        asksum = .0M, 
                                        bidmin = decimal.MaxValue, 
                                        askmin = decimal.MaxValue, 
                                        bidmax = decimal.MinValue, 
                                        askmax = decimal.MinValue, 
                                        count = 0L },
                                    (agg, p) => new { bidsum = agg.bidsum + p.BidPrice, 
                                        asksum = agg.asksum + p.AskPrice, 
                                        bidmin = Math.Min(agg.bidmin, p.BidPrice), 
                                        askmin = Math.Min(agg.askmin, p.AskPrice), 
                                        bidmax = Math.Max(agg.bidmax, p.BidPrice), 
                                        askmax = Math.Max(agg.askmax, p.AskPrice), 
                                        count = agg.count + 1 })
                .Select(agg => new BidAskPriceStatistics(agg.bidsum, agg.asksum, agg.bidmin, agg.askmin, agg.bidmax, agg.askmax, agg.count));
        }

        void whatsWrongwithThis(IObservable<SpotPrice> SpotPriceStream)
        {
            /*
            SpotPriceStatistics = SpotPriceStream.Window(10)
               .Select(window => new
               {
                   AverageBidPriceStream = window.Average(p => p.BidPrice),
                   AverageAskPriceStream = window.Average(p => p.AskPrice),
                   MinBidPriceStream = window.Min(p => p.BidPrice),
                   MinAskPriceStream = window.Min(p => p.AskPrice),
                   MaxBidPriceStream = window.Max(p => p.BidPrice),
                   MaxAskPriceStream = window.Max(p => p.AskPrice)
               })
               .SelectMany(x => Observable.When<PriceStatistics>(
                                    x.AverageBidPriceStream
                                   .And(x.AverageAskPriceStream)
                                   .And(x.MinBidPriceStream)
                                   .And(x.MinAskPriceStream)
                                   .And(x.MaxBidPriceStream)
                                   .And(x.MaxAskPriceStream)
                                   .Then((avgBid, avgAsk, minBid, minAsk, maxBid, maxAsk) => new PriceStatistics(avgBid, avgAsk, minBid, minAsk, maxBid, maxAsk, 10))
                                   ));
             */
        }

        void otherOptimizedMethods(IObservable<SpotPrice> SpotPriceStream)
        {
            var min = decimal.MaxValue;
            var lowFeed = SpotPriceStream
                .Where(p => p.BidPrice < min)
                .Do(p => min = Math.Min(min, p.BidPrice))
                .Select(p => "New LO: " + p);

            var max = decimal.MinValue;
            var highFeed = SpotPriceStream
                .Where(p => p.BidPrice > max)
                .Do(p => max = Math.Max(max, p.BidPrice))
                .Select(p => "New HI: " + p);     
        }        
    }
}
