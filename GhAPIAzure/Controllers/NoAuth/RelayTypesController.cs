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
using DbStructure.Global;

namespace GhAPIAzure.Controllers
{
    [AllowAnonymous]
    public class RelayTypesController : ApiController
    {
        private Models.DataContext db = new Models.DataContext();

        // GET: api/ControlTypes
        /// <summary>
        /// No Auth, Shared
        /// </summary>
        /// <returns></returns>
        public IQueryable<RelayType> GetControlTypes()
        {
            return db.RelayTypes;
        }
    }
}