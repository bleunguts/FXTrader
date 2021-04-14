using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using FXTrader.Common;

namespace FXTrader.ExternalAdapter
{       
    public interface ITradingStreamCallback
    {        
        [OperationContract(IsOneWay = true)]
        void PriceUpdated(PriceResponse response);

        [OperationContract(IsOneWay = true)]
        void TradeExecuted(TradeResponse response);

        [OperationContract(IsOneWay = true)]
        void PriceStatisticsUpdated(PriceStatisticsResponse response);
    }
}
