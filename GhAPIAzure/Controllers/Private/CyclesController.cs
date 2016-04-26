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

namespace GhAPIAzure.Controllers
{
    [Authorize]
    public class CyclesController: BaseController
    {
        private DataContext db = new DataContext();

        // GET: api/Greenhouses
        public IQueryable<CropCycle> GetCycles()
        {
            return db.CropCycles.Where(CC => CC.Greenhouse.PersonId == _UserID);
        }

        // POST: api/Greenhouses
        [ResponseType(typeof(List<CropCycle>))]
        public async Task<IHttpActionResult> PostCycles(List<CropCycle> cCyclesRecieved)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Guid[] IDs = cCyclesRecieved.Select(gh => gh.ID).ToArray();
            List<CropCycle> cCyclesDb = await db.CropCycles
                .Where(CC => IDs.Contains(CC.ID))
                .Include(CC => CC.Greenhouse)
                .Include(CC => CC.CropType)
                .ToListAsync();

            //Check ownership
            if (cCyclesDb.Any(CC => CC.Greenhouse.PersonId != _UserID))
            {
                return Unauthorized();
            }

            foreach (CropCycle cCycleRecieved in cCyclesRecieved)
            {
                var match = cCyclesDb.FirstOrDefault(CC => CC.ID == cCycleRecieved.ID);
                if (match != null)
                {
                    db.Entry(match).CurrentValues.SetValues(cCycleRecieved);
                    //Check if someone else is using this crop, and if so approve it automatically 
                    if (match.CropType.CreatedBy != _UserID && match.CropType.Approved == false) 
                    {
                        match.CropType.Approved = true;
                        match.UpdatedAt = DateTimeOffset.Now; 
                    }
                }
                else
                    db.Entry(cCycleRecieved).State = EntityState.Added;
            }

            await db.SaveChangesAsync();
            return Ok(cCyclesRecieved);
        }
    }



}
