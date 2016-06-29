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
using System.Data.Entity.Migrations;
using System.Security.Claims;
using Swashbuckle.Swagger.Annotations;

namespace GhAPIAzure.Controllers.Private
{
    [Authorize]
    public class PeopleController : BaseController
    {
        private DataContext db = new DataContext();

        // GET: api/People
        /// <summary>
        /// gets account info
        /// </summary>
        /// <remarks>Gets account info, creates account if it's not there</remarks>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Person>))]
        public async Task<List<Person>> GetMe()
        {

            var user = await db.People.FirstOrDefaultAsync(P => P.ID == _UserID);

            if (user == null)
            {
                user = new Person { ID = _UserID };
                db.People.Add(user);
            }
            //user.UserName = _UserName;
            //user.TwitterHandle = _TwitterHandle;

            await db.SaveChangesAsync();

            List<Person> people = new List<Person>();
            people.Add(user);
            return people; 
        }

    }
}