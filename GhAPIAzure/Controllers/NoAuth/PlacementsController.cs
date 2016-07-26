namespace GhAPIAzure.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using DbStructure.Global;
    using Models;

    [AllowAnonymous]
    public class PlacementsController : ApiController
    {
        private readonly DataContext db = new DataContext();

        // GET: api/PlacementTypes
        /// <summary>No Auth, Shared</summary>
        /// <returns></returns>
        public IQueryable<Placement> GetPlacementsTypes() { return db.Placements; }
    }
}
