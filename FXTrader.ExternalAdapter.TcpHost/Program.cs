using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using FXTrader.ExternalAdapter;
using log4net.Config;
using log4net;
using System.Reflection;
using FXTrader.Common;

namespace FXTrader.ExternalAdapter.TcpHost
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            Logger.Info(Constants.Print());
            using (ServiceHost host = new ServiceHost(typeof(FXTrader.ExternalAdapter.TradingService)))
            {
                host.Open();

                Console.WriteLine("Service up and running at:");
                foreach (var ea in host.Description.Endpoints)
                {
                    Console.WriteLine(ea.Address);
                }

                Console.ReadLine();
                host.Close();
            }

        }
    }
}
