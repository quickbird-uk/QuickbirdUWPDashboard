namespace GhAPIAzure.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using DbStructure.Global;
    using Models;

    [AllowAnonymous]
    public class SensorTypesController : ApiController
    {
        private readonly DataContext db = new DataContext();

        // GET: api/ParamsAtPlaces
        /// <summary>No Auth, Shared</summary>
        /// <returns></returns>
        public IQueryable<SensorType> GetParamsAtPlaces() { return db.SensorTypes; }
    }
}
