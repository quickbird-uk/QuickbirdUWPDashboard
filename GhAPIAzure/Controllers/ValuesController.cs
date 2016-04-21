using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace GhAPIAzure.Controllers
{
    public class ValuesController : ApiController
    {
        [Authorize]
        // GET api/values
        [SwaggerOperation("GetAll")]
        public IEnumerable<string> Get()
        {
            
            ClaimsIdentity identity = (ClaimsIdentity)User.Identity;
            var type = identity.AuthenticationType; 
            
            //var owner = identity.FindFirst(ClaimTypes.NameIdentifier ).Value;
            var exp = identity.Claims;
            List<string> result = new List<string>(new string[] {type });

            foreach (var res in exp)
            {
                result.Add(res.Type);
                result.Add(res.Value);
            }
            
            return result;
        }

        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }
    }
}
