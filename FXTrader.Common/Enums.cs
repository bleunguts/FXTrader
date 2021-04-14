using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FXTrader.Common
{
    [DataContract]
    public enum BuySell
    {
        [EnumMember]
        Buy,
        [EnumMember]
        Sell
    }

    [DataContract]
    public enum ProductType
    {
        [EnumMember]
        Spot
        // Outright
        // Swap
        // Options
    }


}
