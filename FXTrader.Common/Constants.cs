using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace FXTrader.Common
{
    public static class Constants
    {
        public static int PriceTickIntervalInMilliseconds { get; private set; }

        /// <summary>
        /// Statistics will be sample every X milliseconds
        /// recommended 5000 or 10000 to avoid noise, if not set the system will go as fast as it can ie few milliseconds
        /// </summary>
        public static int RawPriceStatisticsReadIntervalInMilliseconds { get; private set; }

        public static int RawSpotPriceLoggingThrottleInMilliseconds { get; set; }

        public static int SalesMarkupInPips { get; set; }

        public static string[] AuthorizedClients { get; set; }

        public static bool LogRawPriceStatistics { get; set; }

        public static bool LogRawSpotPriceLogging { get; set; }

        public static bool LogPriceResponse { get; set; }

        public static bool LogPriceStatisticsResponse { get; set; }

        public static int PriceLongOperationTimeInMilliseconds { get; set; }

        public static int TradeLongOperationTimeInMilliseconds { get; set; }

        public static bool DisableThreadingConstructs { get; set; }

        static Constants()
        {
            PriceTickIntervalInMilliseconds = int.Parse(ConfigurationManager.AppSettings["PriceTickIntervalInMilliseconds"]);
            SalesMarkupInPips = int.Parse(ConfigurationManager.AppSettings["SalesMarkupInPips"]);
            var clients = (string)ConfigurationManager.AppSettings["AuthorizedClients"];
            AuthorizedClients = clients.Split(',');
            PriceLongOperationTimeInMilliseconds = int.Parse(ConfigurationManager.AppSettings["PriceLongOperationTimeInMilliseconds"]);
            TradeLongOperationTimeInMilliseconds = int.Parse(ConfigurationManager.AppSettings["TradeLongOperationTimeInMilliseconds"]);
            DisableThreadingConstructs = bool.Parse(ConfigurationManager.AppSettings["DisableThreadingConstructs"]);            

            // logging & tuning
            LogRawSpotPriceLogging = bool.Parse(ConfigurationManager.AppSettings["LogRawSpotPriceLogging"]);
            RawSpotPriceLoggingThrottleInMilliseconds = int.Parse(ConfigurationManager.AppSettings["RawSpotPriceLoggingThrottleInMilliseconds"]);    
            LogRawPriceStatistics = bool.Parse(ConfigurationManager.AppSettings["LogRawPriceStatistics"]);
            RawPriceStatisticsReadIntervalInMilliseconds = int.Parse(ConfigurationManager.AppSettings["RawPriceStatisticsReadIntervalInMilliseconds"]);        
            LogPriceResponse = bool.Parse(ConfigurationManager.AppSettings["LogPriceResponse"]);
            LogPriceStatisticsResponse = bool.Parse(ConfigurationManager.AppSettings["LogPriceStatisticsResponse"]);            
            
        }
        
        public static string Print()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- begin config --");
            sb.AppendLine("===========");
            sb.AppendLine("Simulation:");
            sb.AppendLine("===========");
            sb.AppendFormat("PriceTickIntervalInMilliseconds={0}\nPriceLongOperationTimeInMilliseconds={1}\nTradeLongOperationTimeInMilliseconds={2}\nDisableThreadingConstructs={3}\n",
                            Constants.PriceTickIntervalInMilliseconds,                             
                            Constants.PriceLongOperationTimeInMilliseconds,
                            Constants.TradeLongOperationTimeInMilliseconds,
                            Constants.DisableThreadingConstructs);
            sb.AppendLine("==================");
            sb.AppendLine("Logging & tuning:");
            sb.AppendLine("==================");
            sb.AppendFormat("LogRawSpotPriceLogging={0}\nRawSpotPriceLoggingThrottleInMilliseconds={1}\nLogRawPriceStatistics={2}\nRawPriceStatisticsReadIntervalInMilliseconds={3}\nLogPriceResponse={4}\nLogPriceStatisticsResponse={5}\n",
                            Constants.LogRawSpotPriceLogging,            
                            Constants.RawSpotPriceLoggingThrottleInMilliseconds,   
                            Constants.LogRawPriceStatistics, 
                            Constants.RawPriceStatisticsReadIntervalInMilliseconds,                                                         
                            Constants.LogPriceResponse, 
                            Constants.LogPriceStatisticsResponse);
            sb.AppendLine("==================");
            sb.AppendLine("Business:");
            sb.AppendLine("==================");
            sb.AppendFormat("SalesMarkupInPips={0}\nAuthorizedClients={1}\n",Constants.SalesMarkupInPips, string.Join(",", Constants.AuthorizedClients));
            sb.AppendLine("-- end config --");


            return sb.ToString();   
        }        
    }
}
