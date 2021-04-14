using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FXTrader.Common
{
    [DataContract]
    public partial class TradeResponse
    {
        [DataMember]
        public string ClientName { get; set; }

        [DataMember]        
        public BuySell BuySell { get; set; }

        [DataMember]
        public int Quantity { get; set; }

        [DataMember]
        public decimal TransactionPrice { get; set; }

        [DataMember]
        public Guid TransactionPriceId { get; set; }

        [DataMember]
        public decimal TotalPrice { get; set; }

        [DataMember]
        public long PriceTimestamp { set; get; }
    }
}
