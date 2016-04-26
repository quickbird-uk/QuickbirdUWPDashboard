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
using DatabasePOCOs;

namespace GhAPIAzure.Controllers
{
    
    public class CropTypesController: BaseController
    {
        private DataContext db = new DataContext();

        [AllowAnonymous]
        // GET: api/Greenhouses
        public IQueryable<CropType> GetCropTypes()
        {
            return db.CropTypes;
        }

        [Authorize]
        // POST: api/Greenhouses
        [ResponseType(typeof(List<CropType>))]
        public async Task<IHttpActionResult> PostCropTypes(List<CropType> cTypesRecieved)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (cTypesRecieved.Any(ct => ct.Name.Length > 240 || String.IsNullOrWhiteSpace(ct.Name)))
            {
                return BadRequest();
            }

            string[] names = cTypesRecieved.Select(ct => ct.Name).ToArray();
            List<CropType> cTypesDb = await db.CropTypes.Where(ct => names.Contains(ct.Name)).ToListAsync();

            List<CropType> toCreate = new List<CropType>();

            for (int i = 0; i < cTypesRecieved.Count; i++)
            {
                var match = cTypesDb.FirstOrDefault(CC => string.Compare(CC.Name, cTypesRecieved[i].Name, true) == 0);
                if (match != null)
                {
                    //Check if client is trying to modify existing values
                    if (cTypesRecieved[i].Approved != match.Approved
                        || cTypesRecieved[i].CreatedBy != match.CreatedBy
                        || cTypesRecieved[i].CreatedAt != match.CreatedAt)
                    { return Unauthorized(); }
                    //Do nothing, item exists, but display values from the DB
                    else { cTypesRecieved[i] = match; }            
                }
                else
                {
                    toCreate.Add(cTypesRecieved[i]);
                }
            }

            //Check Ownership
            if (toCreate.Any(ct => ct.CreatedBy != _UserID)
                || toCreate.Any(ct => ct.Approved == true))
            {
                return Unauthorized();
            }
            else
            {
                db.CropTypes.AddRange(toCreate); 

                await db.SaveChangesAsync();
                return Ok(cTypesRecieved);
            }
        }
    }



}
