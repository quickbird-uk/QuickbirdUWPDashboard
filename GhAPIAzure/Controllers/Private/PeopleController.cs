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
using System.Data.Entity.Migrations;
using System.Security.Claims;

namespace GhAPIAzure.Controllers.Private
{
    [Authorize]
    public class PeopleController : BaseController
    {
        private DataContext db = new DataContext();

        // GET: api/People
        [ResponseType(typeof(Person))]
        public async Task<Person> GetMe()
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

            return user;
        }

    }
}