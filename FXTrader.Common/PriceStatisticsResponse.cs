using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FXTrader.Common
{
    [DataContract]
    public class PriceStatisticsResponse
    {
        [DataMember]
        public PriceStatistics Statistics { set; get; }

        [DataMember]
        public string ClientName { get; set; }

        public override string ToString()
        {
            return string.Format("Statistics={0}, ClientName={1}", Statistics, ClientName);
        }

    }
}
