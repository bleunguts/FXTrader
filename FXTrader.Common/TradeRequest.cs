using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace FXTrader.Common
{
    [DataContract]
    public partial class TradeRequest
    {
        [DataMember]
        public ProductType ProductType { set; get; }

        [DataMember]
        public SpotPrice Price { set; get; }

        [DataMember]
        public int Quantity { set; get; }

        [DataMember]
        public BuySell BuySell { set; get; }

        [DataMember]
        public string ClientName { set; get; }

        [DataMember]
        public long PriceTimestamp { set; get; }

        public override string ToString()
        {
            return string.Format("TradeRequest: {0} {1} {2} {3}.", ProductType, BuySell, Quantity, Price);
        }
    }
}