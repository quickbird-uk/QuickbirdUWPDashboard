namespace GhAPIAzure.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using DbStructure.Global;
    using Models;

    [AllowAnonymous]
    public class ParametersController : ApiController
    {
        private readonly DataContext db = new DataContext();

        // GET: api/Parameters
        /// <summary>No Auth, Shared</summary>
        /// <returns></returns>
        public IQueryable<Parameter> GetParameters() { return db.Parameters; }
    }
}
