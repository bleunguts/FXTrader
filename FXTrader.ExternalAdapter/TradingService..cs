using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using FXTrader.ETrader;
using FXTrader.Common;
using System.Timers;
using log4net.Config;
using log4net;
using System.Reflection;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace FXTrader.ExternalAdapter
{    
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class TradingService : ITradingService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ClientSessions _clientSessions;        

        private readonly eTrader _eTrader;       

        public TradingService()
        {
            try
            {
                _clientSessions = new ClientSessions();

                _eTrader = new eTrader();
                _eTrader.Start();
            }
            catch (Exception exp)
            {
                Logger.ErrorFormat("Failed to start, Reason: '{0}'", exp.Message);
            }
        }

        /// <summary>
        /// client should call this method to start subscribing to PriceRequest events
        /// </summary>
        public void RequestPrice(PriceRequest request)
        {
            Logger.InfoFormat("PriceRequest {0} - {1}, client = {2}.", request.ProductType, request.CurrencyPair, request.ClientName);
            _eTrader.AuthorizeClient(request.ClientName);

            SetupCallbackChannel(request.ClientName);       
            
            Logger.InfoFormat("PriceStream channel setup for {0}, streaming {1} - {2} to client.", request.ClientName, request.ProductType, request.CurrencyPair);
            
            IDisposable disposable;
            var primaryStream = _eTrader.GetPriceResponseStream(request);
            if (Constants.DisableThreadingConstructs)
            {
                disposable = primaryStream
                                .Subscribe(priceResponse => PriceUpdated(priceResponse),
                                            e => Logger.WarnFormat("PriceResponse stream error, Reason:'{0}'", e.Message));
            }
            #region Threading Considerations
            else
            {
                disposable = primaryStream
                                    .ObserveOn(NewThreadScheduler.Default)
                                    .Subscribe(priceResponse => PriceUpdated(priceResponse),
                                                e => Logger.WarnFormat("PriceResponse stream error, Reason:'{0}'", e.Message));
            }
            #endregion
            _clientSessions.AddStream(request.ClientName, disposable, primaryStream);
        }

        private void SetupCallbackChannel(string clientName)
        {
            ITradingStreamCallback channel = OperationContext.Current.GetCallbackChannel<ITradingStreamCallback>();
            _clientSessions.AddChannel(clientName, channel);                   
        }     
                     
        public void RequestTrade(TradeRequest request)
        {
            Logger.InfoFormat("TradeRequest sent: {0}", request.ToString());
            _clientSessions.ValidateForSecondaryRequests(request.ClientName);
                      
            Task.Factory.StartNew( () => 
                {
                    var result = _eTrader.ExecuteTrade(request);                  
                    TradeExecuted(result);     
                });
        }


        public void RequestPriceStatistics(PriceStatisticsRequest request)
        {
            try
            {
                Logger.InfoFormat("PriceStatisticsRequest {0} - {1}, client = {2}.", request.ProductType, request.CurrencyPair, request.ClientName);
                _clientSessions.ValidateForSecondaryRequests(request.ClientName);
                var primaryStream = _clientSessions.GetPrimaryStream(request.ClientName);

                IDisposable disposable;
                if (Constants.DisableThreadingConstructs)
                {
                    disposable = _eTrader.GetPriceStatisticsStream(request, primaryStream)
                                            .Subscribe(stats => PriceStatisticsUpdated(stats),
                                                     e => Logger.WarnFormat("PriceStatisticsResponse stream error, Reason:'{0}'", e.Message));
                }
                #region Threading Considerations
                else
                {
                    disposable = _eTrader.GetPriceStatisticsStream(request, primaryStream)
                                                .ObserveOn(NewThreadScheduler.Default)
                                                .Subscribe(stats => PriceStatisticsUpdated(stats),
                                                            e => Logger.WarnFormat("PriceStatisticsResponse stream error, Reason:'{0}'", e.Message));
                }
                #endregion
                _clientSessions.AddStream(request.ClientName, disposable);
            }
            catch (Exception exp)
            {
                Logger.WarnFormat("PriceStatisticsRequest interpreteation failed to {0}, Reason: '{1}'", request.ClientName, exp.Message);
            }
        }

        #region callbacks to client

        /// <summary>
        /// Called when eTrader has updated price for this client
        /// </summary>
        public void PriceUpdated(PriceResponse response)
        {
            lock (_clientSessions)
            {
                try
                {
                    var clientChannel = _clientSessions.GetChannelForCallback(response.ClientName);
                    clientChannel.PriceUpdated(response);
                    if(Constants.LogPriceResponse) Logger.InfoFormat("PriceResponse sent: {0}", response);
                }
                catch (CommunicationObjectAbortedException)
                {
                    lock (_clientSessions)
                    {
                        _clientSessions.DestroyClientSession(response.ClientName);
                    }
                }
                catch (CommunicationObjectFaultedException)
                {
                    lock (_clientSessions)
                    {
                        _clientSessions.DestroyClientSession(response.ClientName);
                    }
                }
                catch (ObjectDisposedException)
                {
                    lock (_clientSessions)
                    {
                        _clientSessions.DestroyClientSession(response.ClientName);
                    }
                }
                catch (Exception exp)
                {
                    Logger.WarnFormat("PriceResponse send failed to {0}, Reason: '{1}'", response.ClientName, exp.Message);
                }
            }
        }
    
        /// <summary>
        /// Called when eTrader has executed trade for this client
        /// </summary>
        /// <param name="response"></param>
        public void TradeExecuted(TradeResponse response)
        {
            lock (_clientSessions)
            {
                try
                {
                    ITradingStreamCallback clientChannel = _clientSessions.GetChannelForCallback(response.ClientName); 
                    clientChannel.TradeExecuted(response);
                    Logger.InfoFormat("TradeResponse sent: {0}", response);
                }
                catch (CommunicationObjectAbortedException)
                {
                    lock (_clientSessions)
                    {
                        _clientSessions.DestroyClientSession(response.ClientName);
                    }
                }
                catch (CommunicationObjectFaultedException)
                {
                    lock (_clientSessions)
                    {
                        _clientSessions.DestroyClientSession(response.ClientName);
                    }
                }
                catch (ObjectDisposedException)
                {
                    lock (_clientSessions)
                    {
                        _clientSessions.DestroyClientSession(response.ClientName);
                    }
                }
                catch (Exception exp)
                {
                    Logger.WarnFormat("TradeResponse send failed to {0}, Reason: '{1}'", response.ClientName, exp.Message);
                }
            }
        }

        public void PriceStatisticsUpdated(PriceStatisticsResponse response)
        {
            lock (_clientSessions)
            {
                try
                {
                    ITradingStreamCallback clientChannel = _clientSessions.GetChannelForCallback(response.ClientName);
                    clientChannel.PriceStatisticsUpdated(response);
                    if (Constants.LogPriceStatisticsResponse) Logger.InfoFormat("PriceStatisticsResponse sent: {0}", response);
                }
                catch (CommunicationObjectAbortedException)
                {
                    lock (_clientSessions)
                    {
                        _clientSessions.DestroyClientSession(response.ClientName);
                    }
                }
                catch (CommunicationObjectFaultedException)
                {
                    lock (_clientSessions)
                    {
                        _clientSessions.DestroyClientSession(response.ClientName);
                    }
                }
                catch (ObjectDisposedException)
                {
                    lock (_clientSessions)
                    {
                        _clientSessions.DestroyClientSession(response.ClientName);
                    }
                }           
                catch (Exception exp)
                {
                    Logger.WarnFormat("PriceStatisticsResponse send failed to {0}, Reason: '{1}'", response.ClientName, exp.Message);
                }
            }
        }

        #endregion

     
    }
}
