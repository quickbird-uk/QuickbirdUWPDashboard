using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using DbStructure;
using Swashbuckle.Swagger.Annotations;
using GhAPIAzure.WebSockets;

namespace GhAPIAzure.Controllers.Auth
{
    public class WebsocketController : BaseController
    {

        

        // GET: api/SensorsHistory
        /// <summary>
        /// Connect to this endpoint using websockets to get a realtime data feed. Non-web-scoket connections will be rejected
        /// </summary>
        /// <remarks>This endpoint is used for braodcasts between apps of the same user. For example if user Bob is logged in with 3 apps,
        /// app №1 can broadcast readings and the other apps will receive them.</remarks>
        [Authorize]
        [SwaggerResponse(HttpStatusCode.SwitchingProtocols)]
        [SwaggerResponse(HttpStatusCode.Forbidden, "Happens when you make a request that is not a web-socket request", Type = typeof(ErrorResponse<string>))]
        public HttpResponseMessage Get()
        {
            if (HttpContext.Current.IsWebSocketRequest)
            {
                
                HttpContext.Current.AcceptWebSocketRequest((AspNetWebSocketContext ctx) => SocketManager.AddConnection(ctx, _UserID));
                return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
            }
            else
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ErrorResponse<string>("Not a web-socket request", "Not a web-socket request")); 
        }




    }
}
