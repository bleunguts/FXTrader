using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace FXTrader.Common
{
    [DataContract]
    public partial class PriceRequest
    {
        [DataMember]
        public ProductType ProductType { set; get; }

        [DataMember]
        public string CurrencyPair { set; get; }

        [DataMember]
        public string ClientName { set; get; }

        public override string ToString()
        {
            return string.Format("product={0}, ccyPair={1}, client={2}.", ProductType, CurrencyPair, ClientName);
        }
    }

}