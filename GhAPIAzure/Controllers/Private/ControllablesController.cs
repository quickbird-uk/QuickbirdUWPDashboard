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
    public class ControllablesController : BaseController
    {
        private DataContext db = new DataContext();

        // GET: api/Greenhouses
        public IQueryable<Controllable> GetControllables()
        {
            return db.Controllables.Where(Ctr => Ctr.Greenhouse.PersonId == _UserID);
        }

        // POST: api/Greenhouses
        [ResponseType(typeof(List<Controllable>))]
        public async Task<IHttpActionResult> PostControllables(List<Controllable> ccsRecieved)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            Guid[] IDs = ccsRecieved.Select(gh => gh.ID).ToArray();
            List<Controllable> ccsDb =  await db.Controllables.Where(Ctr => IDs.Contains(Ctr.ID))
                .Include(Ctr => Ctr.Greenhouse).ToListAsync();

            //Check Ownership
            if (ccsDb.Any(Ctr => Ctr.Greenhouse.PersonId != _UserID))
            {
                return Unauthorized();
            }

            foreach (Controllable ccRecieved in ccsRecieved)
            {
                var match = ccsDb.FirstOrDefault(ctr => ctr.ID == ctr.ID);
                if (match != null)
                    db.Entry(match).CurrentValues.SetValues(ccRecieved);
                else
                    db.Entry(ccRecieved).State = EntityState.Added;
            }

            await db.SaveChangesAsync(); 
            return Ok(ccsRecieved);
        }
    }
}