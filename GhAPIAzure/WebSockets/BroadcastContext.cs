using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebSockets;
using System.Collections.Concurrent; 

namespace GhAPIAzure.WebSockets
{
    public class BroadcastContext
    {
        //Lock this list!
        private List<AppConnection> appConnections = new List<AppConnection>();

        public readonly Guid UserId;


        public BroadcastContext(Guid userId)
        {
            UserId = userId;
        }

        public Task AddAppContext(AspNetWebSocketContext ctx, Guid userId)
        {
            AppConnection freshConnection = new AppConnection(ctx, userId, this);
            lock(appConnections) {
                appConnections.Add(freshConnection);
            }
            return freshConnection.GetDataTask; 
        }

        /// <summary>
        /// This left you broadcast to all clients except one. 
        /// </summary>
        /// <param name="data">The data in question. We assume it's in UTF8 format</param>
        /// <param name="exceptFor">Lets you omit a client from a broadcast. Set Guid.Empty if you want to broadacast to all</param>
        /// <returns></returns>
        public async Task Broadcast(ArraySegment<byte> data, Guid exceptFor)
        {
            Task[] tasks;
            lock (appConnections)
            {
                tasks = new Task[appConnections.Count];
                for (int i = 0; i < appConnections.Count; i++)
                {
                    if (appConnections[i].AppConID != exceptFor)
                        tasks[i] = (appConnections[i].SendData(data)); //FireAnd forget at the moment. This is how it should be! 
                    else
                        tasks[i] = Task.CompletedTask; 
                }
            }
            await Task.WhenAll(tasks);
        }

        public void RemoveAppConnection(AppConnection connection)
        {
            lock(appConnections)
            {
                appConnections.Remove(connection);
            }

            //TODO: remove itself from SocketManager
        }
    }
}
