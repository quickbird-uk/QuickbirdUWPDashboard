namespace GhAPIAzure.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using DbStructure.Global;
    using Models;

    [AllowAnonymous]
    public class SubsystemsController : ApiController
    {
        private readonly DataContext db = new DataContext();

        // GET: api/Subsystems
        /// <summary>No Auth, Shared</summary>
        /// <returns></returns>
        public IQueryable<Subsystem> GetSubsystems() { return db.Subsystems; }
    }
}
