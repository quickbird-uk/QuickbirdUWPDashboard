using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;

namespace GhAPIAzure.WebSockets
{
    public class AppConnection
    {
        public readonly Guid UserId;
        public readonly Guid AppConID = Guid.NewGuid();

        private readonly BroadcastContext _broadcastContext;
        public readonly Task GetDataTask;

        private WebSocket socket;

        private ConcurrentQueue<ArraySegment<byte>> SendQueue = new ConcurrentQueue<ArraySegment<byte>>(); 

        public AppConnection(AspNetWebSocketContext ctx, Guid userId, BroadcastContext broadcastContext)
        {
            UserId = userId;
            socket = ctx.WebSocket;
            GetDataTask = RecieveLoop();
            _broadcastContext = broadcastContext;
        }

        /// <summary>
        /// This point is entered by foreign threads
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task SendData(ArraySegment<byte> data)
        {
            await socket?.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task RecieveLoop()
        {
            //Set up a simple send / receive loop
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[10240]);

            while (true)
            {
                WebSocketReceiveResult retVal = await socket.ReceiveAsync(buffer, CancellationToken.None);

                if (retVal.CloseStatus != null)
                {
                    //new CancellationTokenSource(100).Token; 
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Goodbye", CancellationToken.None);
                    socket.Dispose();
                    socket = null; 
                    _broadcastContext.RemoveAppConnection(this);
                    return; 
                }
                else  // Broadcast to all peers! 
                {
                    //await socket.SendAsync(new ArraySegment<byte>(buffer.Array, 0, retVal.Count),
                    //    retVal.MessageType, retVal.EndOfMessage, CancellationToken.None);  
                                  
                    await _broadcastContext.Broadcast(new ArraySegment<byte>(buffer.Array, 0, retVal.Count), AppConID); 
                }
            }

        }

    }
}