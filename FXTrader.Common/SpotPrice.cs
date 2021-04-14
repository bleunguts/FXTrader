using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FXTrader.Common
{
    [DataContract]
    public struct SpotPrice
    {
        [DataMember]
        public decimal BidPrice { private set; get; }

        [DataMember]
        public decimal AskPrice { private set; get; }

        [DataMember]
        public string CurrencyPair { private set; get; }

        [DataMember]
        public Guid PriceId { private set; get; }

        public SpotPrice(string currencyPair, decimal bidPrice, decimal askPrice)
            : this()
        {
            CurrencyPair = currencyPair;
            BidPrice = bidPrice;
            AskPrice = askPrice;
            PriceId = Guid.NewGuid();
        }
        
        public override string ToString()
        {
            return string.Format("ccyPair={0}, Price={1}/{2}, PriceId={3}", CurrencyPair, BidPrice.ToString("#.00000"), AskPrice.ToString("#.00000"), PriceId);
        }
    }
}
