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
using GhAPIAzure.Models;
using Swashbuckle.Swagger.Annotations;
using DatabasePOCOs.Global;

namespace GhAPIAzure.Controllers
{
    [AllowAnonymous]
    public class ControlTypesController : ApiController
    {
        private Models.DbContext db = new Models.DbContext();

        // GET: api/ControlTypes
        [SwaggerOperation("GetAll")]
        public IQueryable<ControlType> GetControlTypes()
        {
            return db.ControlTypes;
        }
    }
}