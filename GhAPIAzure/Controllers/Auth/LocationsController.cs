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
    public class LocationsController : BaseController
    {
        private DataContext db = new DataContext();

        // GET: api/Locations
        /// <summary>
        /// Get greenhosues that belong to the user
        /// </summary>
        /// <remarks>Get greenhosues that belong to the user</remarks>
        public IQueryable<Location> GetLocations()
        {
            return db.Location.Where(Gh => Gh.PersonId == _UserID);
        }

        // POST: api/Locations
        /// <summary>
        /// Creates or Updates Locations as nessesary. 
        /// </summary>
        /// <remarks> Attempting to edit Locations that do not belong to the user will result in Unauthorised responce. 
        /// No changes will be made</remarks>
        /// <param name="gHousesRecieved">lsit of greenhouses that you want to update or create</param>
        /// <returns></returns>
        [ResponseType(typeof(List<Location>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ModelStateDictionary))]
        [SwaggerResponse(HttpStatusCode.Forbidden, "Happens when you try to edit someone else's stuff", Type = typeof(ErrorResponse<Location>))]
        public async Task<HttpResponseMessage> PostLocations(List<Location> gHousesRecieved)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }
            //Check Ownership
            if(gHousesRecieved.Any(gh => gh.PersonId != _UserID))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                        new ErrorResponse<Location>("You are accessing a location that doesn't belong to you", null));
            }
            Guid[] IDs = gHousesRecieved.Select(gh => gh.ID).ToArray();
            List<Location> gHousesDb =  await db.Location.Where(gh => IDs.Contains(gh.ID)).ToListAsync(); 

            foreach(Location gHouseRecieved in gHousesRecieved)
            {
                var matchDb = gHousesDb.FirstOrDefault(gh => gh.ID == gHouseRecieved.ID);
                if (matchDb != null)
                    db.Entry(matchDb).CurrentValues.SetValues(gHouseRecieved);
                else
                    db.Entry(gHouseRecieved).State = EntityState.Added;
            }

            await db.SaveChangesAsync(); 
            return Request.CreateResponse(HttpStatusCode.OK, gHousesRecieved);
        }
    }
}