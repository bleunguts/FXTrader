using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalSimulator.PriceGenerator;
using FXTrader.Common;
using log4net;
using System.Reflection;
using System.Reactive.Linq;
using FXTrader.ETrader.Statistics;
using FXTrader.ETrader.Config;
using System.Reactive.Concurrency;

namespace FXTrader.ETrader
{
    /// <summary>
    /// In reality this would subscribe to a live price feed either bank's own price feed computer, or a 3rd party like Reuters etc.
    /// 
    /// In this example we will simulate price ticks using the SpotPriceGenerator simulator
    /// </summary>
    class RawSpotPriceService : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly SpotPriceGenerator _spotPriceGenerator;
        private ConfigService _configService;        

        public IObservable<SpotPrice> SpotPriceObservable 
        {             
            get { return _spotPriceGenerator.PriceStream; } 
        }

        public RawSpotPriceService(ConfigService configService)
        {
            _configService = configService;
            _spotPriceGenerator = new SpotPriceGenerator();
        }

        public void StartStreaming()
        {
            _spotPriceGenerator.StartStreaming();

            if (Constants.LogRawSpotPriceLogging) RawSpotPriceLoggingSubscription();

            if (Constants.LogRawPriceStatistics) PriceStatisticsSubscription();           
        }   

        private void RawSpotPriceLoggingSubscription()
        {
            if (Constants.DisableThreadingConstructs)
            {
                SpotPriceObservable
                 .Throttle(TimeSpan.FromMilliseconds(Constants.RawSpotPriceLoggingThrottleInMilliseconds), NewThreadScheduler.Default)
                 .Subscribe(price => Record(price), e => Logger.WarnFormat("Record error, Reason:'{0}'", e.Message));
            }
            #region Threading considerations
            else
            {
                SpotPriceObservable
                    .Throttle(TimeSpan.FromMilliseconds(Constants.RawSpotPriceLoggingThrottleInMilliseconds), NewThreadScheduler.Default)
                    .ObserveOn(NewThreadScheduler.Default)
                    .Subscribe(price => Record(price), e => Logger.WarnFormat("Record error, Reason:'{0}'", e.Message));
            }
            #endregion
        }

        private void PriceStatisticsSubscription()
        {
            if (Constants.DisableThreadingConstructs)
            {
                MovingStatisticsComposer.GetPriceStatisticsObservable(SpotPriceObservable)
                .Sample(TimeSpan.FromMilliseconds(Constants.RawPriceStatisticsReadIntervalInMilliseconds))
                .Subscribe(stats => Record(stats), e => Logger.WarnFormat("Stats stream error, Reason:'{0}'", e.Message));
            }
            #region Threading considerations
            else
            {
                MovingStatisticsComposer.GetPriceStatisticsObservable(SpotPriceObservable)
                    .ObserveOn(NewThreadScheduler.Default)
                    .Sample(TimeSpan.FromMilliseconds(Constants.RawPriceStatisticsReadIntervalInMilliseconds))
                    .Subscribe(stats => Record(stats), e => Logger.WarnFormat("Stats stream error, Reason:'{0}'", e.Message));
            }
            #endregion
        }

        private void Record(SpotPrice price)
        {
            // log the unspreaded client spot price
            Logger.InfoFormat("Spot Price Received {0}", price);
        }

        private void Record(BidAskPriceStatistics stats)
        {
            Logger.InfoFormat("Stats: {0}", stats);
        }

        public void Dispose()
        {
            _spotPriceGenerator.StopStreaming();
        }
    }
}
