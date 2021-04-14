using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using log4net;
using System.Reflection;

namespace FXTrader.ExternalAdapter
{
    public class ClientSessions
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ConcurrentDictionary<string, ClientSession> _clientSessions;            

        public ClientSessions()
        {
            _clientSessions = new ConcurrentDictionary<string, ClientSession>();
        }

        public void AddStream(string clientName, IDisposable disposable, IObservable<Common.PriceResponse> primaryStream = null)
        {
            ClientSession session;
            ValidateForSecondaryRequests(clientName);

            if (_clientSessions.TryGetValue(clientName, out session))
            {
                if (primaryStream != null)
                {
                    session.PrimaryStream = primaryStream;
                }
                session.AddStream(disposable);
            }        
        }        

        public void AddChannel(string clientName, ITradingStreamCallback newchannel)
        {
            ClientSession session;
            if (_clientSessions.TryGetValue(clientName, out session))
            {
                ForcefullyRemoveClientSession(clientName, session);
            }

            session = new ClientSession(newchannel);
            if (!_clientSessions.TryAdd(clientName, session))
            {
                throw new Exception(string.Format("Cannot add client: {0} to channels, possible threading issue.", clientName));                
            }
            Logger.InfoFormat("Client channel successfully created for {0}.", clientName);                 
        }        

        private void ForcefullyRemoveClientSession(string clientName, ClientSession session)
        {
            Logger.InfoFormat("Client exists attempting to remove and kill session... {0}", clientName);
            session.Kill();
            if (!_clientSessions.TryRemove(clientName, out session))
            {
                DestroyAllSessions();
                Logger.WarnFormat("Cannot remove client sesssions on second attempt, forcefully destroyed all sessions!");
            }
            else
            {
                Logger.InfoFormat("Client {0} removed successfully.", clientName);
            }
        }

        public void ValidateForSecondaryRequests(string clientName)
        {
            if (!_clientSessions.ContainsKey(clientName))
            {
                Logger.ErrorFormat("No client streams detected, PriceStream channels must be setup with clients before trading/secondary requests can be placed.");
                throw new Exception("Price streams have not been requested to initiate a session");
            };
        }

        internal IObservable<Common.PriceResponse> GetPrimaryStream(string clientName)
        {
            IObservable<Common.PriceResponse> primaryStream;
            ClientSession session;

            lock (_clientSessions)
            {
                if (!_clientSessions.TryGetValue(clientName, out session))
                {                    
                    throw new Exception(string.Format("Cannot find clientName = {0} in channels, forcefully destroyed all sessions!", clientName));
                }
                primaryStream = session.PrimaryStream;
            }
            return primaryStream;
        }

        internal ITradingStreamCallback GetChannelForCallback(string clientName)
        {
            ITradingStreamCallback callback;
            ClientSession session;

            lock (_clientSessions)
            {
                if (!_clientSessions.TryGetValue(clientName, out session))
                {
                    DestroyAllSessions();
                    throw new Exception(string.Format("Cannot find clientName = {0} in channels, forcefully destroyed all sessions!", clientName));
                }
                callback = session.ClientCallback;
            }
            return callback;
        }

        internal void DestroyClientSession(string clientName)
        {
            ClientSession session;
            if (_clientSessions.TryRemove(clientName, out session))
            {
                session.Kill();
                Logger.WarnFormat("Client {0} was forcefully removed from channel.", clientName);
            }
            else
            {
                DestroyAllSessions();
                Logger.WarnFormat("Client {0} could not be removed from channel, forcefully destroyed all sessions!", clientName);                
            }
        }

        private void DestroyAllSessions()
        {
            foreach (var session in _clientSessions.Values)
            {
                session.Kill();                
            }
            _clientSessions.Clear();
        } 
    }
}
