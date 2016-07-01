using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Collections.Concurrent;
using System.Web.WebSockets;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using DbStructure;
using Swashbuckle.Swagger.Annotations;

namespace GhAPIAzure.Controllers.Auth
{
    public class WebsocketController : BaseController
    {

        //public static ConcurrentDictionary<Guid, ConcurrentBag<int>> UserCollection = new ConcurrentDictionary<Guid, ConcurrentBag<int>>();


        // GET: api/SensorsHistory
        /// <summary>
        /// Connect to this endpoint using websockets to get a realtime data feed. Non-web-scoket connections will be rejected
        /// </summary>
        /// <remarks>This endpoint is used for braodcasts between apps of the same user. For example if user Bob is logged in with 3 apps,
        /// app №1 can broadcast readings and the other apps will receive them.</remarks>
        [SwaggerResponse(HttpStatusCode.SwitchingProtocols)]
        [SwaggerResponse(HttpStatusCode.Forbidden, "Happens when you make a request that is not a web-socket request", Type = typeof(ErrorResponse<string>))]
        public HttpResponseMessage Get(string username)
        {
            if (HttpContext.Current.IsWebSocketRequest)
            {
                HttpContext.Current.AcceptWebSocketRequest(WebSocketLoop);
                return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
            }
            else
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ErrorResponse<string>("Not a web-socket request", "Not a web-socket request")); 
        }


        private static async Task WebSocketLoop(AspNetWebSocketContext ctx)
        {
            WebSocket socket = ctx.WebSocket;
            //Send the 'Hello' Message

            await socket.SendAsync(new ArraySegment<byte>(
                Encoding.UTF8.GetBytes("Connected to the Echo server!")),
                WebSocketMessageType.Text, true, CancellationToken.None);

            //Set up a simple send / receive loop
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            while (true)
            {
                var retVal = await socket.ReceiveAsync(buffer, CancellationToken.None);

                if (retVal.CloseStatus != null)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Goodbye", CancellationToken.None);

                }
                else //echo them message
                {
                    await socket.SendAsync(
                        new ArraySegment<byte>(buffer.Array, 0, retVal.Count),
                        retVal.MessageType,
                        retVal.EndOfMessage,
                        CancellationToken.None);
                }
            }

        }



        //private  ChatWebSocketHandler 
        //{
        //    private string _username;

        //    public ChatWebSocketHandler(AspNetWebSocketContext ctx)
        //    {
        //        _username = username;
        //    }

        //    public override void OnOpen()
        //    {
        //        _chatClients.Add(this);
        //    }

        //    public override void OnMessage(string message)
        //    {
        //        _chatClients.Broadcast(_username + ": " + message);
        //    }
        //}


    }
}
