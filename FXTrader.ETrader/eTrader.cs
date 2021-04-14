using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FXTrader.Common;
using System.Reactive.Linq;
using System.Reflection;
using log4net;
using FXTrader.ETrader.Config;
using System.Threading.Tasks;
using FXTrader.ETrader.Statistics;
using System.Reactive.Concurrency;
using System.Threading;

namespace FXTrader.ETrader
{
    public class eTrader : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConfigService _config;
        private readonly RawSpotPriceService _rawSpotPriceService;
        private readonly FXPricer.FXPricer _fxPricer;     

        public eTrader()
        {
            _config = ConfigService.GetTradingConfig();            
            _rawSpotPriceService = new RawSpotPriceService(_config);
            _fxPricer = new FXPricer.FXPricer();            
        }

        public void Start()
        {
            _rawSpotPriceService.StartStreaming();
        }

        public IObservable<PriceResponse> GetPriceResponseStream(PriceRequest request)
        {
            return _rawSpotPriceService.SpotPriceObservable
                .Where(price => price.CurrencyPair.Equals(request.CurrencyPair))
                .Select(spotPrice => Price(request, spotPrice, _config.SalesMarkupInPips));            
        }

        // This is where we call into the analytics/pricing libraries to compute how client price
        private PriceResponse Price(PriceRequest request, SpotPrice spotPrice, int spreadInPips)
        {
            if (Constants.LogPriceResponse) Logger.DebugFormat("Pricing Request {0}, {1}, {2}", request.CurrencyPair, spotPrice, spreadInPips);

            var price = _fxPricer.Price(request.CurrencyPair,spotPrice, spreadInPips);
            return new SpreadedPriceResponse() { SpotPrice = price, SpreadInPips = spreadInPips, ClientName = request.ClientName };
        }
  
        public TradeResponse ExecuteTrade(TradeRequest request)
        {
            decimal purchasePrice;
            decimal totalPrice;
            _fxPricer.PriceTrade(request.ProductType, request.BuySell, request.Quantity, request.Price, out purchasePrice, out totalPrice);

            var response = new TradeResponse { ClientName = request.ClientName, TotalPrice = totalPrice, BuySell = request.BuySell, Quantity = request.Quantity, TransactionPrice = purchasePrice, TransactionPriceId = request.Price.PriceId, PriceTimestamp = request.PriceTimestamp };

            /// this is where we would pass the trade to a TradeCaptureService
            if (Constants.TradeLongOperationTimeInMilliseconds > 0) Thread.Sleep(Constants.TradeLongOperationTimeInMilliseconds);
            return response;
        }

        public IObservable<PriceStatisticsResponse> GetPriceStatisticsStream(PriceStatisticsRequest request, IObservable<PriceResponse> primaryStream)
        {
            return MovingStatisticsComposer.GetPriceStatisticsObservable(primaryStream.Select(p => p.SpotPrice))
                    .ObserveOn(NewThreadScheduler.Default)
                    .Sample(TimeSpan.FromMilliseconds(Constants.RawPriceStatisticsReadIntervalInMilliseconds))
                    .Select(stats => ProcessPriceStatistics(request.ClientName, request.BuySell, stats));
        }
        
        private PriceStatisticsResponse ProcessPriceStatistics(string clientName, BuySell buySell, BidAskPriceStatistics stats)
        {
            return new PriceStatisticsResponse
            {
                ClientName = clientName,
                Statistics = new PriceStatistics
                {
                    AveragePrice = buySell == BuySell.Buy ? stats.AverageAskPrice : stats.AverageBidPrice,
                    MinPrice = buySell == BuySell.Buy ? stats.MinAskPrice : stats.MinBidPrice,
                    MaxPrice = buySell == BuySell.Buy ? stats.MaxAskPrice : stats.MaxBidPrice
                }
            };
        }

        // This is where we connect to an authentication service to authenticate the client
        public void AuthorizeClient(string clientName)
        {
            if (!_config.AuthorizedClients.Contains(clientName))
            {
                Logger.ErrorFormat("Authentication failed for client: {0}.", clientName);
                throw new Exception("Client is not authorized to access FXTrader.");
            }
        }        

        public void Dispose()
        {
            _rawSpotPriceService.Dispose();
        }  
    }
}
