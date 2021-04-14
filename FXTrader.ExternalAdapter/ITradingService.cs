using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using FXTrader.Common;

namespace FXTrader.ExternalAdapter
{
    [ServiceContract(CallbackContract = typeof(ITradingStreamCallback))]
    public interface ITradingService
    {
        [OperationContract]
        void RequestPrice(PriceRequest request);

        [OperationContract]
        void RequestTrade(TradeRequest request);

        [OperationContract]
        void RequestPriceStatistics(PriceStatisticsRequest request);
    }
}
