using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FXTrader.Common
{
    [DataContract]
    public class PriceStatisticsRequest
    {
        [DataMember]
        public ProductType ProductType { set; get; }

        [DataMember]
        public string CurrencyPair { set; get; }

        [DataMember]
        public BuySell BuySell { set; get; }

        [DataMember]
        public string ClientName { set; get; }

        public override string ToString()
        {
            return string.Format("product={0}, ccyPair={1}, BuySell={2}, client={3}.", ProductType, CurrencyPair, BuySell, ClientName);
        }
    }
}
