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
        /// <param name="linuxTime">Date from which we start grabbing data, as lunux ticks in UTC</param>
        /// <param name="number">How many days to take from that date</param>
        [Route("api/SensorsHistory/{linuxTime}/{number}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SensorHistory>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<HttpResponseMessage> GetSensorsHistory(long linuxTime, int number)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTimeOffset afterDate = new DateTimeOffset(epoch + TimeSpan.FromSeconds(linuxTime), TimeSpan.Zero);

           // System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            //timer.Start(); 

            List<SensorHistory> sHistories =  await 
                db.SensorHistories.Where(sHist => sHist.Location.PersonId == _UserID 
                && sHist.TimeStamp > afterDate)
                .OrderBy(sHist => sHist.TimeStamp)
                .Take(number).ToListAsync();

            //timer.Stop(); 

            //WE should deserialise ALL of them! 
            foreach(var ch in sHistories)
            {
                ch.DeserialiseData(); 
            }


            //Slice all the time histories that were collected on the same date
            for (int i = 0; i < sHistories.Count; i++)
            {
                if ((sHistories[i].TimeStamp.UtcDateTime - afterDate.UtcDateTime) < TimeSpan.FromDays(1))
                    sHistories[i] = sHistories[i].Slice(afterDate); 
                 
            }
            return Request.CreateResponse(HttpStatusCode.OK, sHistories); 
        }

        // POST: api/SensorsHistory
        /// <summary>
        /// Accepts a list of SensorHistories you want to edit. 
        /// </summary>
        /// <remarks> This accepts delta updates. 
        /// So you can add a SensorsHistory that has only one new datapoint each, and it will just add it on top of what's already in the DB
        /// UpdatedAt will be overwritten with the time of upload, even if no changes were made to the item</remarks>
        /// <param name="shRecievedList">A list of sensorHistories that you want to add or edit.</param>
        /// <returns>Ok if all good, otherwise you will get an ErrorResponce</returns>

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(void))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ModelStateDictionary))]
        [SwaggerResponse(HttpStatusCode.Forbidden, "Happens when you try to edit someone else's stuff",Type = typeof(ErrorResponse<SensorHistory>))]
        public async Task<HttpResponseMessage> PostSensorsHistory(List<SensorHistory> shRecievedList)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }

            List<Guid> sensorIDs = shRecievedList.Select(chR => chR.SensorID).ToList();

            //Get all relevant sensors, they must both exist and belong to this user! 
            List<Sensor> userSensors =
                await db.Sensors.Where(rel => sensorIDs.Contains(rel.ID) && rel.Device.Location.PersonId == _UserID)
                .ToListAsync();

            //If one of the submitted items reffers to a sensor that doesn't exist/belong to user, return error
            foreach (SensorHistory sHistory in shRecievedList)
            {
                if (userSensors.Any(rel => rel.ID == sHistory.SensorID) == false)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest,
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
                SensorHistory SensHistoryDB = sensHistDbRawList.FirstOrDefault(rHist => rHist.SensorID == sensHistRecieved.SensorID
                && rHist.TimeStamp == sensHistRecieved.TimeStamp);

                if (SensHistoryDB == null) //create new
                {
                    if(false == userLocations.Any(loc => loc.ID == sensHistRecieved.LocationID))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest,
                      new ErrorResponse<SensorHistory>("Referenced location doesn't exist", sensHistRecieved));
                    }

                    sensHistRecieved.SerialiseData();
                    db.Entry(sensHistRecieved).State = EntityState.Added;
                }
                else
                {
                    if(SensHistoryDB.LocationID != sensHistRecieved.LocationID)
                    {
                        return Request.CreateResponse(HttpStatusCode.Forbidden,
                       new ErrorResponse<SensorHistory>("You are not allowed to change location of SensorHistory", sensHistRecieved));
                    }
                    
                    SensHistoryDB.DeserialiseData();
                    SensorHistory chMerged = SensorHistory.Merge(SensHistoryDB, sensHistRecieved);

                    SensHistoryDB.Data = chMerged.Data; 
                    SensHistoryDB.SerialiseData(); 
                }
            }

            await db.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}