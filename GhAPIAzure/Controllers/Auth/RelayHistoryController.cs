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
    public class RelayHistoryController : BaseController
    {
        private DataContext db = new DataContext();

        // GET: api/RelayHistory
        /// <summary>
        /// Gets historical record of what relays were doing.
        /// </summary>
        /// <remarks>Gets historical record of what relays were doing. 
        /// In the "from" parameter you should spesify the date when you have last synced with the server. 
        /// The server will then 'slice' the datapoints for that day, and only provide you with ones that were produced later than the from date
        /// Timestamp on the day should be midnight of the day when recording FINISHES. It's the end of that day.
        /// All the records attached to that day sould be timestamped before the day!</remarks>
        /// <param name="dateTimeTicks">Date from which we start grabbing data, as ticks in UTC</param>
        /// <param name="number">How many days to take from that date</param>
        [Route("api/RelayHistory/{dateTimeTicks}/{number}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<RelayHistory>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<HttpResponseMessage> GetRelayHistory(long dateTimeTicks, int number)
        {
            DateTimeOffset from = new DateTimeOffset(dateTimeTicks, TimeSpan.Zero); 
            List<RelayHistory> RelayHistory =  await 
                db.RelayHistories.Where(Ch => Ch.Location.PersonId == _UserID 
                && Ch.TimeStamp > from)
                .OrderBy(Ch => Ch.TimeStamp)
                .Take(number).ToListAsync();

            //WE should deserialise ALL of them! 
            foreach(var ch in RelayHistory)
            {
                ch.DeserialiseData(); 
            }

            if(RelayHistory.Count > 0)
            {
                RelayHistory[0].DeserialiseData();
                RelayHistory[0] = RelayHistory[0].Slice(from);
            }
            return Request.CreateResponse(HttpStatusCode.OK, RelayHistory); 
        }

        // POST: api/RelayHistory
        /// <summary>
        /// Accepts a list of RelayHistories you want to edit. 
        /// </summary>
        /// <remarks> This accepts delta updates. 
        /// So you can add a RelayHistory that has only one new datapoint each, and it will just add it on top of what's already in the DB</remarks>
        /// <param name="rhRecievedList">A list of RelayHistories that you want to add or edit.</param>
        /// <returns>Ok if all good, otherwise you will get an ErrorResponce</returns>

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(void))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ModelStateDictionary))]
        [SwaggerResponse(HttpStatusCode.Forbidden, "Happens when you try to edit someone else's stuff",Type = typeof(ErrorResponse<RelayHistory>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Refers to something that doesn;t exist", Type = typeof(ErrorResponse<RelayHistory>))]
        public async Task<HttpResponseMessage> PostRelayHistory(List<RelayHistory> rhRecievedList)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }

            List<Guid> relayIDs = rhRecievedList.Select(chR => chR.RelayID).ToList();

            //Get all relevant RelayHistories, they must both exist and belong to this user! 
            List<Relay> usersRelays =
                await db.Relays.Where(rel => relayIDs.Contains(rel.ID) && rel.Device.Location.PersonId == _UserID)
                .ToListAsync();

            //If one of the submitted items reffers to a Relay that doesnt exist/belong to user, return error
            foreach (RelayHistory rHistory in rhRecievedList)
            {
                if (!usersRelays.Any(rel => rel.ID == rHistory.RelayID))
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound,
                        new ErrorResponse<RelayHistory>("One of the Relays does not exist", rHistory));
                }
            }

            //Get all the RelayHistories that are being edited
            List<DateTimeOffset> timestamps = rhRecievedList.Select(relHist => relHist.TimeStamp).ToList();
            List<RelayHistory> relHistDbRawList = await db.RelayHistories.Where(relHist => timestamps.Contains(relHist.TimeStamp)
            && relayIDs.Contains(relHist.RelayID)).ToListAsync();
            List<Location> userLocations = await db.Location.Where(loc => loc.PersonId == _UserID).ToListAsync(); 

            foreach (var relHistRecieved in rhRecievedList)
            {
                var relHistDB = relHistDbRawList.FirstOrDefault(rHist => rHist.RelayID == relHistRecieved.RelayID
                && rHist.TimeStamp == relHistRecieved.TimeStamp);
                if (relHistDB == null) //create new
                {
                    if(false == userLocations.Any(loc => loc.ID == relHistRecieved.LocationID))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound,
                      new ErrorResponse<RelayHistory>("Referenced location doesn't exist", relHistRecieved));
                    }

                    relHistRecieved.SerialiseData();
                    db.Entry(relHistRecieved).State = EntityState.Added;
                }
                else
                {
                    if(relHistDB.LocationID != relHistRecieved.LocationID)
                    {
                        return Request.CreateResponse(HttpStatusCode.Forbidden,
                       new ErrorResponse<RelayHistory>("You are not allowed to change location of dataHistory", relHistRecieved));
                    }
                    
                    relHistDB.DeserialiseData();
                    RelayHistory chMerged = RelayHistory.Merge(relHistDB, relHistRecieved);
                    chMerged.SerialiseData();
                    relHistDB.RawData = chMerged.RawData;
                }
            }

            await db.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}