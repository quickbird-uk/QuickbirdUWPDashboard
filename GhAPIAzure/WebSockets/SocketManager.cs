using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;

namespace GhAPIAzure.WebSockets
{
    public static class SocketManager
    {

        private static Dictionary<Guid, BroadcastContext> UsersCollection = new Dictionary<Guid, BroadcastContext>();


        //TODO: addSonnection method
        public static Task AddConnection(AspNetWebSocketContext ctx, Guid userId)
        {
            Task returnTask; 
            lock(UsersCollection)
            {
                BroadcastContext broadcastContext;  
                if(UsersCollection.TryGetValue(userId, out broadcastContext))
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