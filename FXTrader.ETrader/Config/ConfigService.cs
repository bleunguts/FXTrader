using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FXTrader.Common;

namespace FXTrader.ETrader.Config
{
    class ConfigService
    {
        /// <summary>
        /// Do not set this -> system isnt designed to handle more than one ccy
        /// </summary>
        public string[] eTraderSupportedCurrencyPairs { get; private set; }

        /// <summary>
        /// Authorized list of clients
        /// </summary>
        public string[] AuthorizedClients { get; private set; }

        /// <summary>
        /// Spread applied for sales commision
        /// </summary>
        public int SalesMarkupInPips { get; private set; }          

        /// <summary>
        /// Get static data
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// In reality this will fetch trading config data from another service in the bank, database, from another system
        /// remember these configs can change at anytime
        /// </remarks>
        public static ConfigService GetTradingConfig()
        {            
            return new ConfigService
            { 
                eTraderSupportedCurrencyPairs = new[] { "GBPUSD" },
                AuthorizedClients = Constants.AuthorizedClients,
                SalesMarkupInPips = Constants.SalesMarkupInPips
            };
        }        
    }
}
