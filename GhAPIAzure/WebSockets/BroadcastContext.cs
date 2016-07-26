namespace GhAPIAzure.WebSockets
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.WebSockets;

    public class BroadcastContext
    {
        public readonly Guid UserId;
        //Lock this list!
        private readonly List<AppConnection> appConnections = new List<AppConnection>();


        public BroadcastContext(Guid userId) { UserId = userId; }

        public Task AddAppContext(AspNetWebSocketContext ctx, Guid userId)
        {
            var freshConnection = new AppConnection(ctx, userId, this);
            lock (appConnections)
            {
                appConnections.Add(freshConnection);
            }
            return freshConnection.GetDataTask;
        }

        /// <summary>This left you broadcast to all clients except one.</summary>
        /// <param name="data">The data in question. We assume it's in UTF8 format</param>
        /// <param name="exceptFor">Lets you omit a client from a broadcast. Set Guid.Empty if you want to
        /// broadacast to all</param>
        /// <returns></returns>
        public async Task Broadcast(ArraySegment<byte> data, Guid exceptFor)
        {
            var tasks = new List<Task>();

            lock (appConnections)
            {
                for (var i = 0; i < appConnections.Count; i++)
                {
                    if (appConnections[i].AppConID != exceptFor)
                        tasks.Add(appConnections[i].SendData(data));
                }
            }
            await Task.WhenAll(tasks);
        }

        public void RemoveAppConnection(AppConnection connection)
        {
            lock (appConnections)
            {
                appConnections.Remove(connection);
            }

            //TODO: remove itself from SocketManager
        }
    }
}
