using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FXTrader.Common
{
    [DataContract]    
    public class SpreadedPriceResponse : PriceResponse
    {
        public int SpreadInPips { get; set; }
    }
}
