using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FXTrader.Common;
using System.Reactive.Concurrency;

namespace ExternalSimulator.PriceGenerator
{
    public class SpotPriceGenerator
    {
        // only supports one currency pair 
        private readonly string[] currencyPairsPriced = new[] { "GBPUSD" };
        private readonly int RawSpreadInPips = 2;
        private readonly TimeSpan PriceTickInterval = TimeSpan.FromMilliseconds(Constants.PriceTickIntervalInMilliseconds);

        private IDisposable _disposableStream;
        private Random _random;       

        public IConnectableObservable<SpotPrice> PriceStream { private set; get; }

        public SpotPriceGenerator()
        {
            _random = new Random();
        }

        public void StartStreaming()
        {
            if (Constants.DisableThreadingConstructs)
            {
                PriceStream = Observable.Interval(PriceTickInterval)
                                    .Select<long, SpotPrice>(l => GenerateSpotPrice(currencyPairsPriced[0]))
                                    .Publish();
            }
            #region Threading considerations
            else
            {
                PriceStream = Observable.Interval(PriceTickInterval)
                                        .ObserveOn(NewThreadScheduler.Default)
                                        .Select<long, SpotPrice>(l => GenerateSpotPrice(currencyPairsPriced[0]))
                                        .Publish();                
            }
            #endregion

            _disposableStream = PriceStream.Connect();
        }

        /// <summary>
        /// Only for GBPUSD
        /// </summary>
        /// <param name="currencyPair">GBPUSD</param>
        /// <returns>random spot price 1.6420 +- 10 pips</returns>
        private SpotPrice GenerateSpotPrice(string currencyPair)
        {
            // get a random midpoint - fluctuate by 10 pips
            var rawMid = (decimal)(1.6420 + _random.NextDouble() / 100);            

            // apply spread
            var difference = (RawSpreadInPips / 10000M) / 2M;                       
            var bidPrice = rawMid - difference;
            var askPrice = rawMid + difference;
            return new SpotPrice(currencyPair, bidPrice, askPrice);            
        }

        public void StopStreaming()
        {
            _disposableStream.Dispose();
        }      
    }
}
