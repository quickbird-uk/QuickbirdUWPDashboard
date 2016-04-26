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
    public class ParametersController : ApiController
    {
        private Models.DataContext db = new Models.DataContext();

        // GET: api/Parameters
        [SwaggerOperation("GetAll")]
        public IQueryable<Parameter> GetParameters()
        {
            return db.Parameters;
        }
    }
}