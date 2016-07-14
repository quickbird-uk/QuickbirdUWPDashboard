namespace GhAPIAzure.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using DbStructure.User;
    using Models;
    using DbStructure;
    using Swashbuckle.Swagger.Annotations;
    using System.Web.Http.ModelBinding;

    [Authorize]
    public class SensorsHistoryController : BaseController
    {
        private readonly DataContext _db = new DataContext();

        // GET: api/SensorsHistory
        /// <summary>Gets historical record of sensor's measurements</summary>
        /// <remarks>Gets historical data after the specified datetime. </remarks>
        /// <param name="deviceId">Id of sensor to get histories for.</param>
        /// <param name="unixTime">Only returns results with a timestamp greater than this.</param>
        /// <param name="maxDays">Sets a maximum amount of days to return, multiply by number of sensors.</param>
        [Route("api/SensorsHistory/{deviceId}/{unixTime}/{maxDays}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SensorHistory>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<HttpResponseMessage> GetSensorsHistory(Guid deviceId, long unixTime, int maxDays)
        {
            //System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            //timer.Start(); 

            var afterTime = DateTimeOffset.FromUnixTimeSeconds(unixTime);
            var includingAndAfterDay = afterTime.Date;
            var daySpan = TimeSpan.FromDays(maxDays);

            var device = await _db.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.ID == deviceId);


            var sensorHistories =
                await
                    _db.SensorHistories.AsNoTracking()
                        .Include(sh => sh.Sensor)
                        .Where(
                            sHist =>
                                sHist.Location.PersonId == _UserID && sHist.TimeStamp >= includingAndAfterDay &&
                                sHist.Sensor.DeviceID == deviceId)
                        .OrderBy(sHist => sHist.TimeStamp)
                        .TakeWhile(sh => sh.TimeStamp.Date - includingAndAfterDay < daySpan)
                        .ToListAsync();
 
            for (var i = 0; i < sensorHistories.Count; i++)
            {
                var hist = sensorHistories[i];
                if (hist.TimeStamp.Date == includingAndAfterDay)
                {
                    sensorHistories[i] = hist.Slice(afterTime);
                }
                else
                {
                    // Items are sorted so there should be no more on the first day
                    break;
                }
            }

            // The Json converter uses the deserialised data, ignores the raw blob stored in the DB.
            foreach (var history in sensorHistories)
            {
                history.DeserialiseData();
            }

            //timer.Stop(); 
            return Request.CreateResponse(HttpStatusCode.OK, sensorHistories);
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
                await _db.Sensors.Where(rel => sensorIDs.Contains(rel.ID) && rel.Device.Location.PersonId == _UserID)
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
            List<SensorHistory> sensHistDbRawList = await _db.SensorHistories.Where(sensHist => timestamps.Contains(sensHist.TimeStamp)
            && sensorIDs.Contains(sensHist.SensorID)).ToListAsync();
            List<Location> userLocations = await _db.Location.Where(loc => loc.PersonId == _UserID).ToListAsync(); 

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
                    sensHistRecieved.UploadedAt = DateTimeOffset.Now; 
                    _db.Entry(sensHistRecieved).State = EntityState.Added;
                }
                else
                {
                    if(SensHistoryDB.LocationID != sensHistRecieved.LocationID)
                    {
                        return Request.CreateResponse(HttpStatusCode.Forbidden,
                       new ErrorResponse<SensorHistory>("You are not allowed to change location of SensorHistory", sensHistRecieved));
                    }



                    //Check if changed were made by comparing raw bytes data
                    sensHistRecieved.SerialiseData();


                    if (Compare(sensHistRecieved.RawData, SensHistoryDB.RawData) == false)
                    {
                        SensHistoryDB.DeserialiseData();
                        SensorHistory chMerged = SensorHistory.Merge(SensHistoryDB, sensHistRecieved);
                        SensHistoryDB.Data = chMerged.Data;
                        SensHistoryDB.SerialiseData();
                        SensHistoryDB.UploadedAt = DateTimeOffset.Now; 
                    }
                }
            }

            await _db.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private bool Compare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;

            return true;
        }
    }
}