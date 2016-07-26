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
    public class SubsystemsController : ApiController
    {
        private Models.DataContext db = new Models.DataContext();

        // GET: api/Subsystems
        /// <summary>
        /// No Auth, Shared
        /// </summary>
        /// <returns></returns>
        public IQueryable<Subsystem> GetSubsystems()
        {
            return db.Subsystems;
        }
    }
}