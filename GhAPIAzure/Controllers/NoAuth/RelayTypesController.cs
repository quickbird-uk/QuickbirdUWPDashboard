namespace GhAPIAzure.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using DbStructure.Global;
    using Models;

    [AllowAnonymous]
    public class RelayTypesController : ApiController
    {
        private readonly DataContext db = new DataContext();

        // GET: api/ControlTypes
        /// <summary>No Auth, Shared</summary>
        /// <returns></returns>
        public IQueryable<RelayType> GetControlTypes() { return db.RelayTypes; }
    }
}
