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
    public class SensorsHistoryController : BaseController
    {
        private DataContext db = new DataContext();

        // GET: api/SensorsHistory
        /// <summary>
        /// Gets historical record of sensor's measurements
        /// </summary>
        /// <remarks>Gets historical record of sensor's measurements
        /// In the "from" parameter you should spesify the date when you have last synced with the server. 
        /// The server will then 'slice' the datapoints for that day, and only provide you with ones that were produced later than the from date
        /// Timestamp on the day should be midnight of the day when recording FINISHES. It's the end of that day.
        /// All the records attached to that day sould be timestamped before the day!</remarks>
        /// <param name="dateTimeTicks">Date from which we start grabbing data, as ticks in UTC</param>
        /// <param name="number">How many days to take from that date</param>
        [Route("api/SensorsHistory/{dateTimeTicks}/{number}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SensorHistory>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<HttpResponseMessage> GetSensorsHistory(long dateTimeTicks, int number)
        {
            DateTimeOffset from = new DateTimeOffset(dateTimeTicks, TimeSpan.Zero); 
            List<SensorHistory> sensorsHistory =  await 
                db.SensorHistories.Where(Ch => Ch.Location.PersonId == _UserID 
                && Ch.TimeStamp > from)
                .OrderBy(Ch => Ch.TimeStamp)
                .Take(number).ToListAsync();

            //WE should deserialise ALL of them! 
            foreach(var ch in sensorsHistory)
            {
                ch.DeserialiseData(); 
            }

            if(sensorsHistory.Count > 0)
            {
                sensorsHistory[0].DeserialiseData();
                sensorsHistory[0] = sensorsHistory[0].Slice(from);
            }
            return Request.CreateResponse(HttpStatusCode.OK, sensorsHistory); 
        }

        // POST: api/SensorsHistory
        /// <summary>
        /// Accepts a list of SensorHistories you want to edit. 
        /// </summary>
        /// <remarks> This accepts delta updates. 
        /// So you can add a SensorsHistory that has only one new datapoint each, and it will just add it on top of what's already in the DB</remarks>
        /// <param name="shRecievedList">A list of sensorHistories that you want to add or edit.</param>
        /// <returns>Ok if all good, otherwise you will get an ErrorResponce</returns>

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(void))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ModelStateDictionary))]
        [SwaggerResponse(HttpStatusCode.Forbidden, "Happens when you try to edit someone else's stuff",Type = typeof(ErrorResponse<SensorHistory>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Refers to something that doesn;t exist", Type = typeof(ErrorResponse<SensorHistory>))]
        public async Task<HttpResponseMessage> PostSensorsHistory(List<SensorHistory> shRecievedList)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }

            List<Guid> sensorIDs = shRecievedList.Select(chR => chR.SensorID).ToList();

            //Get all relevant Controllables, they must both exist and belong to this user! 
            List<Sensor> usersRelays =
                await db.Sensors.Where(rel => sensorIDs.Contains(rel.ID) && rel.Device.Location.PersonId == _UserID)
                .ToListAsync();

            //If one of the submitted items reffers to a controllable that doesnt exist/belong to user, return error
            foreach (SensorHistory sHistory in shRecievedList)
            {
                if (!usersRelays.Any(rel => rel.ID == sHistory.SensorID))
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound,
                        new ErrorResponse<SensorHistory>("One of the SensorIDs does not exist", sHistory));
                }
            }

            //Get all the control histories that are being edited
            List<DateTimeOffset> timestamps = shRecievedList.Select(sensHist => sensHist.TimeStamp).ToList();
            List<SensorHistory> sensHistDbRawList = await db.SensorHistories.Where(sensHist => timestamps.Contains(sensHist.TimeStamp)
            && sensorIDs.Contains(sensHist.SensorID)).ToListAsync();
            List<Location> userLocations = await db.Location.Where(loc => loc.PersonId == _UserID).ToListAsync(); 

            foreach (var sensHistRecieved in shRecievedList)
            {
                SensorHistory SensHIstoryDB = sensHistDbRawList.FirstOrDefault(rHist => rHist.SensorID == sensHistRecieved.SensorID
                && rHist.TimeStamp == sensHistRecieved.TimeStamp);
                if (SensHIstoryDB == null) //create new
                {
                    if(false == userLocations.Any(loc => loc.ID == sensHistRecieved.LocationID))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound,
                      new ErrorResponse<SensorHistory>("Referenced location doesn't exist", sensHistRecieved));
                    }

                    sensHistRecieved.SerialiseData();
                    db.Entry(sensHistRecieved).State = EntityState.Added;
                }
                else
                {
                    if(SensHIstoryDB.LocationID != sensHistRecieved.LocationID)
                    {
                        return Request.CreateResponse(HttpStatusCode.Forbidden,
                       new ErrorResponse<SensorHistory>("You are not allowed to change location of SensorHistory", sensHistRecieved));
                    }
                    
                    SensHIstoryDB.DeserialiseData();
                    SensorHistory chMerged = SensorHistory.Merge(SensHIstoryDB, sensHistRecieved);
                    chMerged.SerialiseData();
                    SensHIstoryDB.RawData = chMerged.RawData;
                }
            }

            await db.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}