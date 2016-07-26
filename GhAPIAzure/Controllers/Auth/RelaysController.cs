using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using DbStructure.User;
using GhAPIAzure.Models;
using System.Security.Claims;
using EntityFramework.Extensions;
using EFExtensions;
using DbStructure;
using Swashbuckle.Swagger.Annotations;
using System.Web.Http.ModelBinding;

namespace GhAPIAzure.Controllers
{

    [Authorize]
    public class RelaysController : BaseController
    {
        private DataContext db = new DataContext();

        // GET: api/Relays
        /// <summary>
        /// Gets All relays that belong to this user
        /// </summary>
        /// <remarks> Gets all the relays that belong to the user that's currently logged in. 
        /// Ownership is determined through connection to a devvice, that in installed in a 
        /// location that belongs to the user. </remarks>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Relay>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<HttpResponseMessage> GetRelays()
        {
            List<Relay> devices = await db.Relays.Where(rel => rel.Device.Location.PersonId == _UserID).ToListAsync();
            return Request.CreateResponse(HttpStatusCode.OK, devices); 
        }

        // POST: api/Relays
        /// <summary>
        /// Accepts a list of Relays you want to create or edit
        /// </summary>
        /// <remarks> This accepts a list of Relays. It will create one if it does not exist, or edit one if it already exists.</remarks>
        /// <param name="relRecivedList">The list of Relays</param>
        /// <returns>Ok if all good, otherwise you will get an ErrorResponce</returns>

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Relay>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ModelStateDictionary))]
        [SwaggerResponse(HttpStatusCode.Forbidden, "Happens when you try to edit someone else's stuff",Type = typeof(ErrorResponse<Relay>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Happens when you refer to items that do not exist", Type = typeof(ErrorResponse<Relay>))]
        public async Task<HttpResponseMessage> PostRelays(List<Relay> relRecivedList)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }
            var duplicate = relRecivedList.FirstOrDefault(p => relRecivedList.Any(q => (p != q && p.ID == q.ID))); 
            if (null != duplicate)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden, new ErrorResponse<Relay>(
                        "You submitted a list with duplicates", duplicate)); 
            }
            
            //Check for illegal locations
            List<Guid> DeviceIDs = relRecivedList.Select(dev => dev.DeviceID).ToList();
            List<Device> devicesDb = await db.Devices.Where(dev => DeviceIDs.Contains(dev.ID))
                .Include(dev => dev.Location).ToListAsync();

            foreach (Relay relRecieved in relRecivedList)
            {
                Device device = devicesDb.FirstOrDefault(dev => dev.ID == relRecieved.DeviceID);
                if (device == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, new ErrorResponse<Relay>(
                        "the device you referenced does not exist. Create a location first", relRecieved));
                else if (device.Location.PersonId != _UserID)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new ErrorResponse<Relay>(
                        "You can't attach a relay to someone else's device", relRecieved));
                }
            }
             
            List<Guid> relayIDs = relRecivedList.Select(dev => dev.ID).ToList();
            List<Relay> relaysDbList = await db.Relays.Where(rel => relayIDs.Contains(rel.ID)).ToListAsync(); 
            for(int i =0; i < relRecivedList.Count; i++)
            {
                var MatchDB = relaysDbList.FirstOrDefault(rel => rel.ID == relRecivedList[i].ID); 


                if(MatchDB != null)
                { 
                    if(MatchDB.DeviceID != relRecivedList[i].DeviceID)
                        return Request.CreateResponse(HttpStatusCode.Forbidden, new ErrorResponse<Relay>(
                        "You can't move the relay to a different device", relRecivedList[i]));
                    else if (MatchDB.RelayTypeID!= relRecivedList[i].RelayTypeID)
                        return Request.CreateResponse(HttpStatusCode.Forbidden, new ErrorResponse<Relay>(
                        "You can't change the relay type", relRecivedList[i]));
                    else
                        db.Entry(MatchDB).CurrentValues.SetValues(relRecivedList[i]); 
                }
                else
                {
                    db.Entry(relRecivedList[i]).State = EntityState.Added; 
                }         
            }

            await db.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.OK, relRecivedList);
        }
    }
}