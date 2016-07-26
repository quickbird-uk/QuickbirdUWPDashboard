namespace GhAPIAzure.WebSockets
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.WebSockets;

    public static class SocketManager
    {
        private static readonly Dictionary<Guid, BroadcastContext> UsersCollection =
            new Dictionary<Guid, BroadcastContext>();


        //TODO: addSonnection method
        public static Task AddConnection(AspNetWebSocketContext ctx, Guid userId)
        {
            Task returnTask;
            lock (UsersCollection)
            {
                BroadcastContext broadcastContext;
                if (UsersCollection.TryGetValue(userId, out broadcastContext))
                {
                    returnTask = broadcastContext.AddAppContext(ctx, userId);
                }
                else
                {
                    broadcastContext = new BroadcastContext(userId);
                    returnTask = broadcastContext.AddAppContext(ctx, userId);
                    UsersCollection.Add(userId, broadcastContext);
                }
            }

            return returnTask;
        }

        public static void RemoveEmptyBroadcastContext(BroadcastContext broadcastContext)
        {
            lock (UsersCollection)
            {
                UsersCollection.Remove(broadcastContext.UserId);
            }
        }
    }
}
