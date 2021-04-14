using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ServiceModel;
using System.Collections.ObjectModel;
using log4net.Config;
using log4net;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bilbos.ETrader
{   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, FXWorld.ITradingServiceCallback
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<string> _productTypes = new List<string>() { "GBPUSD", "EURUSD", "AUDUSD", "EURCHF" };
        private readonly List<string> _buysell = new List<string>() { "Buy", "Sell" };
        private readonly ObservableCollection<string> _priceList = new ObservableCollection<string>();        

        public List<string> ProductTypes { get { return _productTypes; } }
        public List<string> BuySell { get { return _buysell; } }

        public ObservableCollection<string> PriceList { get { return _priceList; } }

        private FXWorld.TradingServiceClient _tradingServiceProxy;

        public MainWindow()
        {
            InitializeComponent();
            this.cboCurrency.DataContext = _productTypes;
            this.cboBuySell.DataContext = _buysell;
            this.lstPrices.DataContext = _priceList;            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {                                                
            UpdateStatus("Ready");

            Connect();            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Disconnect();
        }      

        private void Connect()
        {
            try
            {
                _tradingServiceProxy = new FXWorld.TradingServiceClient(new InstanceContext(this), "NetTcpBinding_ITradingService");
                _tradingServiceProxy.Open();

                Dispatcher.Invoke(new Action(() => UpdateStatus("Connected to server.")));
            }
            catch (Exception exp)
            {
                MessageBox.Show(string.Format("Failed to connect to server, 'Reason:{0}'", exp.Message, "Server Error"));
            }
        }

        private void Reconnect()
        {
            Dispatcher.Invoke(new Action(() => UpdateStatus(string.Format("Abrupt disconnection detected, reconnecting... "))));
            Connect();
        }

        private void Disconnect()
        {
            try
            {
                Dispatcher.Invoke(new Action(() => UpdateStatus(string.Format("Disconnected."))));
                _tradingServiceProxy.Close();
            }
            catch
            {
            }
        }
   
        private void btnSubscribe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currencyPair = this.cboCurrency.Text;
                var priceRequest = new FXWorld.PriceRequest() { ProductType = FXWorld.ProductType.Spot, CurrencyPair = currencyPair, ClientName = txtClientName.Text };

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        _tradingServiceProxy.RequestPrice(priceRequest);
                        Dispatcher.Invoke(new Action(() => UpdateStatus(string.Format("Subscribed to price stream for: {0}.", currencyPair))));
                    }
                    catch (ObjectDisposedException)
                    {
                        Reconnect();
                    }
                });
            }
            catch (ObjectDisposedException)
            {
                Reconnect();
            }
            catch (Exception exp)
            {
                MessageBox.Show(string.Format("Failed to connect to server, 'Reason:{0}'", exp.Message, "Server Error"));
            }
        }

        private void btnUnsubscribe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Disconnect();
            }
            catch (Exception)
            {                
            }
        }        

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var timestamped = (PriceTimestamped)lblLatestPrice.Tag;     
                var currentPrice = timestamped.SpotPrice;
                var quantity = int.Parse(this.txtQuantity.Text);
                var buySell = (FXWorld.BuySell)Enum.Parse(typeof(FXWorld.BuySell), cboBuySell.Text);
                var clientName = this.txtClientName.Text;
                var request = new FXWorld.TradeRequest() { Price = currentPrice, Quantity = quantity, BuySell = buySell, ProductType = FXWorld.ProductType.Spot, ClientName= clientName, PriceTimestamp = timestamped.Timestamp};

                Logger.InfoFormat("TradeExecuteRequest @ PriceId={0} ", request.Price.PriceId);
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                       _tradingServiceProxy.RequestTrade(request);
                       Dispatcher.Invoke(new Action(() => UpdateStatus(string.Format("Trade executed, {0} {1} @ {2}", buySell.ToString(), quantity, buySell == FXWorld.BuySell.Buy ? currentPrice.AskPrice : currentPrice.BidPrice))));
                    }
                    catch (ObjectDisposedException)
                    {
                        Reconnect();
                    }
                });
            }
            catch (ObjectDisposedException)
            {
                Reconnect();
            }
            catch (Exception exp)
            {
                MessageBox.Show(string.Format("Cannot execute trade, Reason:'{0}'", exp.Message, "Error"));
            }            
        }

        private void btnPriceStatitics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currencyPair = this.cboCurrency.Text;
                var clientName = this.txtClientName.Text;
                var buySell = (FXWorld.BuySell)Enum.Parse(typeof(FXWorld.BuySell), cboBuySell.Text);
                var request = new FXWorld.PriceStatisticsRequest() { BuySell = buySell, ClientName = clientName, ProductType = FXWorld.ProductType.Spot, CurrencyPair = currencyPair};

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        _tradingServiceProxy.RequestPriceStatistics(request);
                        Dispatcher.Invoke(new Action(() => UpdateStatus(string.Format("Stats requested, {0} {1}", buySell.ToString(), currencyPair))));
                    }                    
                    catch (ObjectDisposedException)
                    {
                        Reconnect();
                    }
                });
            }
            catch (ObjectDisposedException)
            {
                Reconnect();
            }
            catch (Exception exp)
            {
                MessageBox.Show(string.Format("Cannot subscribe to price statistics, Reason:'{0}'", exp.Message, "Error"));
            }
        }

        #region callbacks

        public void PriceUpdated(FXWorld.PriceResponse response)
        {
            try
            {
                var latestPrice = FormatPrice(response.SpotPrice);
                var timestampedResponse = new PriceTimestamped { Timestamp = Stopwatch.GetTimestamp(), SpotPrice = response.SpotPrice }; 
                Dispatcher.BeginInvoke(new Action(() => {             
                    UpdatePriceStreamLabel(response.SpotPrice.CurrencyPair);                    
                    lblLatestPrice.Content = latestPrice;
                    lblLatestPrice.Tag = timestampedResponse;
                    _priceList.Add(latestPrice);
                    lstPrices.ScrollIntoView(lstPrices.Items[lstPrices.Items.Count - 1]);
                }));

                Logger.InfoFormat("PriceReceived: {0}", Print(response.SpotPrice));
            }
            catch (Exception exp)
            {
                MessageBox.Show(string.Format("Cannot interpret PriceResponse, Reason:'{0}'", exp.Message, "Error"));
            }
        } 

        public void TradeExecuted(FXWorld.TradeResponse response)
        {
            try
            {
                var timestamp = Stopwatch.GetTimestamp();
                Logger.InfoFormat("TradeExecuteResponse @ PriceId={0}, Latency={1} ms ", response.TransactionPriceId, CalculateLatency(response.PriceTimestamp, timestamp));

                Dispatcher.BeginInvoke(new Action(() => {    
                UpdateStatus(string.Format("Trade executed successfully. {0} {1} @ {2} with total price @ {3}", response.BuySell, response.Quantity, response.TransactionPrice.ToString("#.0000"), response.TotalPrice.ToString("#.0000")));                
                }));      
            }
            catch (Exception exp)
            {
                MessageBox.Show(string.Format("Cannot interpret trade response, Reason:'{0}'", exp.Message, "Error"));
            }
        }

        private double CalculateLatency(long start, long stop)
        {
            var latency = (stop - start) / (double) Stopwatch.Frequency;
            return Math.Round(latency,3);
        }

        public void PriceStatisticsUpdated(FXWorld.PriceStatisticsResponse response)
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() => {  
                this.lblAvg.Content = response.Statistics.AveragePrice.ToString("#.0000");
                this.lblMin.Content = response.Statistics.MinPrice.ToString("#.0000");
                this.lblMax.Content = response.Statistics.MaxPrice.ToString("#.0000");
                    }));      
            }
            catch (Exception exp)
            {
                MessageBox.Show(string.Format("Cannot interpret stats response, Reason:'{0}'", exp.Message, "Error"));
            }
        }        

        #endregion

        #region ui updates
        private void UpdateStatus(string statusText)
        {
            this.txtStatus.Text = statusText;
        }

        private void UpdatePriceStreamLabel(string productType)
        {         
            if (lblPriceStream.Content != null && lblPriceStream.Content.ToString().Equals(productType))
                return;

            lblPriceStream.Content = productType;            
        }

        private void btnClearPriceHistory_Click(object sender, RoutedEventArgs e)
        {
            _priceList.Clear();
        }
        #endregion     
        
        #region price formatters
        private string Print(FXWorld.SpotPrice price)
        {
            return string.Format("ccyPair={0}, Price={1}/{2}, PriceId={3}", price.CurrencyPair, price.BidPrice.ToString("#.00000"), price.AskPrice.ToString("#.00000"), price.PriceId);
        }

        private string FormatPrice(FXWorld.SpotPrice price)
        {
            return string.Format("{0}/{1}", price.BidPrice.ToString("#.00000"), price.AskPrice.ToString("#.00000"));
        }

        private FXWorld.SpotPrice ToSpotPrice(string currentPrice)
        {
            var prices = currentPrice.Split('/');
            var bidPrice = decimal.Parse(prices[0]);
            var askPrice = decimal.Parse(prices[1]);

            return new FXWorld.SpotPrice { CurrencyPair = (string)this.cboCurrency.SelectedValue, BidPrice = bidPrice, AskPrice = askPrice };
        }   
        #endregion          
          
        struct PriceTimestamped 
        {
            public long Timestamp { get; set; }            
            public FXWorld.SpotPrice SpotPrice { get; set; }
        }
    }
}
