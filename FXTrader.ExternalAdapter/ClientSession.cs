using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FXTrader.ExternalAdapter
{
    public class ClientSession
    {      
        public ITradingStreamCallback ClientCallback { get; private set; }
        
        public IObservable<Common.PriceResponse> PrimaryStream { get; set; }
        public List<IDisposable> DisposableStreams { get; private set; }

        public ClientSession(IDisposable disposable)
        {
            DisposableStreams = new List<IDisposable> { disposable };
        }

        public ClientSession(ITradingStreamCallback newchannel)
        {
            DisposableStreams = new List<IDisposable>();
            ClientCallback = newchannel;
        }
 
        public void AddStream(IDisposable disposableStream)
        {
            DisposableStreams.Add(disposableStream);
        }

        internal void Kill()
        {
            foreach (var d in DisposableStreams)
            {                
                d.Dispose();
            }
        }        
    }
}
