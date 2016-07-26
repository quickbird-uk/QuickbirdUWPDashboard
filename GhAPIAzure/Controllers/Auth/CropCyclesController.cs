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
using Swashbuckle.Swagger.Annotations;
using DbStructure;
using System.Web.Http.ModelBinding;

namespace GhAPIAzure.Controllers
{
    [Authorize]
    public class CropCyclesController: BaseController
    {
        private DataContext db = new DataContext();

        // GET: api/Greenhouses
        /// <summary>
        /// Gets all crop cycles in all grenehosues for this user
        /// </summary>
        /// <remarks></remarks>
        public IQueryable<CropCycle> GetCycles()
        {
            return db.CropCycles.Where(CC => CC.Location.PersonId == _UserID);
        }

        // POST: api/Greenhouses
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CropCycle>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ModelStateDictionary))]
        [SwaggerResponse(HttpStatusCode.Forbidden, "Happens when you try to edit someone else's stuff", Type = typeof(ErrorResponse<CropCycle>))]
        public async Task<HttpResponseMessage> PostCropCycles(List<CropCycle> cCyclesRecieved)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }

            Guid[] IDs = cCyclesRecieved.Select(cc => cc.ID).ToArray();
            Guid[] ghIDs = cCyclesRecieved.Select(gh => gh.LocationID).ToArray();

            var Greenhouses = await db.Location.Where(gh => ghIDs.Contains(gh.ID))
                .Include(gh => gh.CropCycles).ToListAsync();
            List<CropType> cropTypes = await db.CropTypes.ToListAsync(); 
            List<CropCycle> cCyclesDb = Greenhouses.SelectMany(gh => gh.CropCycles).ToList();

            var missingGh = cCyclesRecieved.FirstOrDefault(cc => ! Greenhouses.Any(gh => gh.ID == cc.LocationID)); 
            if(missingGh != null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ErrorResponse<CropCycle>(
                    "This cycle spesifies invalid greenhosue ID", missingGh));
            }
            var missingCrop = cCyclesRecieved.FirstOrDefault(cc => !cropTypes.Any(ct => ct.Name == cc.CropTypeName));
            if (missingCrop != null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ErrorResponse<CropCycle>(
                    "This cycle spesifies invalid CropType", missingCrop));
            }
            //Check ownership
            var invalidCycle = cCyclesDb.FirstOrDefault(CC => CC.Location.PersonId != _UserID); 
            if (null != invalidCycle)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden, new ErrorResponse<CropCycle>(
                    "Can't create a cycle on a greenhosue that does not belong to you", invalidCycle));
            }

            foreach (CropCycle cCycleRecieved in cCyclesRecieved)
            {
                var match = cCyclesDb.FirstOrDefault(CC => CC.ID == cCycleRecieved.ID);
                if (match != null)
                {
                    db.Entry(match).CurrentValues.SetValues(cCycleRecieved);

                }
                else
                {
                    db.Entry(cCycleRecieved).State = EntityState.Added;
                }
                ////Check if someone else is using this crop, and if so approve it automatically 
                var selectedCrop = cropTypes.First(ct => ct.Name == cCycleRecieved.CropTypeName);
                if (selectedCrop.CreatedBy != _UserID && selectedCrop.Approved == false)
                {
                    selectedCrop.Approved = true;
                }
            }

            await db.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.OK, cCyclesRecieved);
        }
    }



}
