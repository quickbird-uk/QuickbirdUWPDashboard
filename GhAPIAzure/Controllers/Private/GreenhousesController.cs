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
using DatabasePOCOs.User;
using GhAPIAzure.Models;
using System.Security.Claims;
using EntityFramework.Extensions;
using EFExtensions; 

namespace GhAPIAzure.Controllers
{
    [Authorize]
    public class GreenhousesController : BaseController
    {
        private DataContext db = new DataContext();

        // GET: api/Greenhouses
        public IQueryable<Greenhouse> GetGreenhouses()
        {
            return db.Greenhouses.Where(Gh => Gh.PersonId == _UserID);
        }

        // POST: api/Greenhouses
        [ResponseType(typeof(List<Greenhouse>))]
        public async Task<IHttpActionResult> PostGreenhouses(List<Greenhouse> gHousesRecieved)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //Check Ownership
            if(gHousesRecieved.Any(gh => gh.PersonId != _UserID))
            {
                return Unauthorized(); 
            }
            Guid[] IDs = gHousesRecieved.Select(gh => gh.ID).ToArray();
            List<Greenhouse> gHousesDb =  await db.Greenhouses.Where(gh => IDs.Contains(gh.ID)).ToListAsync(); 

            foreach(Greenhouse gHouseRecieved in gHousesRecieved)
            {
                var match = gHousesDb.FirstOrDefault(gh => gh.ID == gHouseRecieved.ID);
                if (match != null)
                    db.Entry(match).CurrentValues.SetValues(gHouseRecieved);
                else
                    db.Entry(gHouseRecieved).State = EntityState.Added;
            }

            await db.SaveChangesAsync(); 
            return Ok(gHousesRecieved);
        }
    }
}