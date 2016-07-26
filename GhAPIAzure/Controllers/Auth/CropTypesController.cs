namespace GhAPIAzure.Controllers
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.ModelBinding;
    using DbStructure;
    using Models;
    using Swashbuckle.Swagger.Annotations;

    public class CropTypesController : BaseController
    {
        private readonly DataContext db = new DataContext();

        /// <summary>No Auth</summary>
        /// <returns></returns>
        [AllowAnonymous]
        // GET: api/Greenhouses
        public IQueryable<CropType> GetCropTypes()
        {
            return db.CropTypes;
        }


        // POST: api/Greenhouses
        /// <summary>Special. Creates a new Crop</summary>
        /// <remarks>Creates a new crop. Once created, crops cannot be edited by users. Crops are created by a
        /// certain user. If you are creating a new crop, CreatedBY should be set to the ID of your user.
        /// Approved flag indicates wether this crop should be shown publicly or should only be displayed to
        /// the user that created it Users cannot directly edit the approved status of a crop</remarks>
        /// <param name="cTypesRecieved"></param>
        [Authorize]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CropType>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ModelStateDictionary))]
        [SwaggerResponse(HttpStatusCode.Forbidden, "Actions you are not allowed to do",
             Type = typeof(ErrorResponse<CropType>))]
        public async Task<HttpResponseMessage> PostCropTypes(List<CropType> cTypesRecieved)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }
            var invalidName =
                cTypesRecieved.FirstOrDefault(ct => ct.Name.Length > 240 || string.IsNullOrWhiteSpace(ct.Name));
            if (invalidName != null)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    new ErrorResponse<CropType>("Name is invalid", invalidName));
                ;
            }

            var names = cTypesRecieved.Select(ct => ct.Name).ToArray();
            var cTypesDb = await db.CropTypes.Where(ct => names.Contains(ct.Name)).ToListAsync();

            var toCreate = new List<CropType>();

            for (var i = 0; i < cTypesRecieved.Count; i++)
            {
                var match = cTypesDb.FirstOrDefault(CC => string.Compare(CC.Name, cTypesRecieved[i].Name, true) == 0);
                if (match != null)
                {
                    //Check if client is trying to modify existing values
                    if (cTypesRecieved[i].Approved && match.Approved == false
                        //we don;t let users upgrade the status, but if their version is not approved, we let it slide
                        || cTypesRecieved[i].CreatedBy != match.CreatedBy ||
                        cTypesRecieved[i].CreatedAt != match.CreatedAt)
                    {
                        return Request.CreateResponse(HttpStatusCode.Forbidden,
                            new ErrorResponse<CropType>(
                                "You are not allowed to edit creation time and approval of crops", cTypesRecieved[i]));
                    }
                    //Do nothing, item exists, but display values from the DB
                    cTypesRecieved[i] = match;
                }
                else
                {
                    toCreate.Add(cTypesRecieved[i]);
                }
            }

            //Check Ownership
            if (toCreate.Any(ct => ct.CreatedBy != _UserID) || toCreate.Any(ct => ct.Approved))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    new ErrorResponse<CropType>(
                        "All new crops should have approved set ot false, and CreatedBy  should be your user", null));
            }
            db.CropTypes.AddRange(toCreate);

            await db.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.OK, cTypesRecieved);
        }
    }
}
