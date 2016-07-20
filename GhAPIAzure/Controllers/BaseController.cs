using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using DbStructure.User;
using GhAPIAzure.Models;
using System.Security.Claims;

namespace GhAPIAzure.Controllers
{
    public class BaseController : ApiController
    {

        public ulong _TwitterID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Guid _UserID { get
            {
                var user = User;
                var Identity = user.Identity;
                var claimsIdentity = ((ClaimsIdentity)Identity);

                var claims = claimsIdentity.Claims;

                string claimString = claims.FirstOrDefault(c => c.Type.Contains("stable_sid")).Value;
                string guidString = claimString.Split(':').Last();

                return Guid.Parse(guidString);
            } }

        public string _UserName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string _TwitterHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
