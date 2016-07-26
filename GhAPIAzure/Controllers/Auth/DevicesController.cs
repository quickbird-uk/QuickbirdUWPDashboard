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
    public class DevicesController : BaseController
    {
        private DataContext db = new DataContext();

        // GET: api/ControlHistory
        /// <summary>
        /// Gets All devices that belong to this user
        /// </summary>
        /// <remarks> Gets all the devices that belong to the user that's currently logged in. 
        /// Ownership is determined through connection to a location htat belongs to the user. </remarks>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Device>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<HttpResponseMessage> GetDevices()
        {
            List<Device> devices = await db.Devices.Where(dev => dev.Location.PersonId == _UserID).ToListAsync();
            return Request.CreateResponse(HttpStatusCode.OK, devices); 
        }

        // POST: api/Greenhouses
        /// <summary>
        /// Accepts a list of devices you want to create or edit
        /// </summary>
        /// <remarks> This accepts a list of devices. It will create one if it does not exist, or edit one if it already exists.</remarks>
        /// <param name="devRecievedList">The list of devices</param>
        /// <returns>Ok if all good, otherwise you will get an ErrorResponce</returns>

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Device>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ModelStateDictionary))]
        [SwaggerResponse(HttpStatusCode.Forbidden, "Happens when you try to edit someone else's stuff",Type = typeof(ErrorResponse<Device>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Happens when you refer to items that do not exist", Type = typeof(ErrorResponse<Device>))]
        public async Task<HttpResponseMessage> PostControlHistory(List<Device> devRecievedList)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }
            var duplicate = devRecievedList.FirstOrDefault(p => devRecievedList.Any(q => (p != q && p.ID == q.ID))); 
            if (null != duplicate)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden, new ErrorResponse<Device>(
                        "You submitted a list with duplicates", duplicate)); 
            }
            
            //Check for illegal locations
            List<Guid> locationIDs = devRecievedList.Select(dev => dev.LocationID).ToList();
            List<Location> locationsDb = await db.Location.Where(loc => locationIDs.Contains(loc.ID)).ToListAsync();

            foreach (Device devRecieved in devRecievedList)
            {
                Location location = locationsDb.FirstOrDefault(loc => loc.ID == devRecieved.LocationID);
                if (location == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, new ErrorResponse<Device>(
                        "the location you referenced does not exist. Create a location first", devRecieved));
                else if (location.PersonId != _UserID)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new ErrorResponse<Device>(
                        "You can't refer your device to someone else's location", devRecieved));
                }
            }
             
            List<Guid> deviceIDs = devRecievedList.Select(dev => dev.ID).ToList();
            List<Device> devicesDbList = await db.Devices.Where(dev => deviceIDs.Contains(dev.ID)).ToListAsync(); 
            for(int i =0; i < devRecievedList.Count; i++)
            {
                var MatchDB = devicesDbList.FirstOrDefault(dev => dev.ID == devRecievedList[i].ID); 


                if(MatchDB != null)
                { 
                    if(MatchDB.SerialNumber != devRecievedList[i].SerialNumber)
                        return Request.CreateResponse(HttpStatusCode.Forbidden, new ErrorResponse<Device>(
                        "You can't change the device's serial number", devRecievedList[i]));
                    else
                        db.Entry(MatchDB).CurrentValues.SetValues(devRecievedList[i]); 
                }
                else
                {
                    db.Entry(devRecievedList[i]).State = EntityState.Added; 
                }         
            }

            await db.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.OK, devRecievedList);
        }
    }
}