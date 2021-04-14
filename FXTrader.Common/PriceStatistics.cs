using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FXTrader.Common
{
    [DataContract]
    public class PriceStatistics
    {
        [DataMember]
        public decimal AveragePrice { get; set; }

        [DataMember]
        public decimal MinPrice { get; set; }
        
        [DataMember]
        public decimal MaxPrice { get; set; }

        public override string ToString()
        {
            return string.Format("Avg: {0}, Min: {1}, Max: {2}", AveragePrice.ToString("#.00000"), MinPrice.ToString("#.00000"), MaxPrice.ToString("#.00000"));
        }     
    }
}
