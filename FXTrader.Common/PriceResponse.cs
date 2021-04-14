using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FXTrader.Common
{
    [KnownType(typeof(SpreadedPriceResponse))]
    [DataContract]
    public partial class PriceResponse
    {
        [DataMember]
        public SpotPrice SpotPrice { set; get; }
        
        [DataMember]
        public string ClientName { get; set; }

        public override string ToString()
        {
            return string.Format("SpotPrice={0}, ClientName={1}", SpotPrice, ClientName);
        }        
    }
}
