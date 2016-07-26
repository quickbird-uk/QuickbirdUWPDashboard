namespace GhAPIAzure.WebSockets
{
    using System;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.WebSockets;

    public class AppConnection : IDisposable
    {
        private readonly BroadcastContext _broadcastContext;
        public readonly Guid AppConID = Guid.NewGuid();
        public readonly Task GetDataTask;
        public readonly Guid UserId;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        private WebSocket socket;


        public AppConnection(AspNetWebSocketContext ctx, Guid userId, BroadcastContext broadcastContext)
        {
            UserId = userId;
            socket = ctx.WebSocket;
            _broadcastContext = broadcastContext;
            _cancellation.Token.Register(SendCloseAndDispose);
            GetDataTask = RecieveLoop();
        }


        public async void SendCloseAndDispose()
        {
            var timeout = new CancellationTokenSource(500);
            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Goodbye", timeout.Token);
            }
            catch
            {
            }
            timeout.Dispose();
            Dispose();
        }

        //TODO: detect multiple threads trying to send
        /// <summary>This point is entered by foreign threads</summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task SendData(ArraySegment<byte> data)
        {
            try
            {
                var Timeout = new CancellationTokenSource(700);
                await socket?.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
                if (Timeout.IsCancellationRequested)
                {
                    _cancellation.Cancel();
                }
                else
                    Timeout.Dispose();
            }
            catch
            { //if there is an error, setup the socket for disposal 
                _cancellation.Cancel();
            }
        }

        private async Task RecieveLoop()
        {
            //Set up a simple send / receive loop
            var buffer = new ArraySegment<byte>(new byte[10240]);

            while (true)
            {
                WebSocketReceiveResult retVal = null;
                try
                {
                    retVal = await socket.ReceiveAsync(buffer, _cancellation.Token);
                }
                catch
                {
                    _cancellation.Cancel();
                }

                if (retVal?.CloseStatus != null)
                {
                    _cancellation.Cancel();
                }
                else // Broadcast to all peers! 
                {
                    await _broadcastContext.Broadcast(new ArraySegment<byte>(buffer.Array, 0, retVal.Count), AppConID);
                }
            }
        }

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    _broadcastContext?.RemoveAppConnection(this);
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                socket?.Dispose();
                socket = null;
                disposedValue = true;
            }
        }


        ~AppConnection()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            //uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
