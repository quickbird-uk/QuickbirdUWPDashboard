namespace GhAPIAzure.Controllers.Private
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using DbStructure.User;
    using Models;
    using Swashbuckle.Swagger.Annotations;

    [Authorize]
    public class PeopleController : BaseController
    {
        private readonly DataContext db = new DataContext();

        // GET: api/People
        /// <summary>gets account info</summary>
        /// <remarks>Gets account info, creates account if it's not there</remarks>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Person>))]
        public async Task<List<Person>> GetMe()
        {
            var user = await db.People.FirstOrDefaultAsync(P => P.ID == _UserID);

            if (user == null)
            {
                user = new Person {ID = _UserID};
                db.People.Add(user);
            }
            //user.UserName = _UserName;
            //user.TwitterHandle = _TwitterHandle;

            await db.SaveChangesAsync();

            var people = new List<Person>();
            people.Add(user);
            return people;
        }
    }
}
