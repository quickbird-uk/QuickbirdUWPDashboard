namespace GhAPIAzure.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Web.Http;

    public class BaseController : ApiController
    {
        public string _TwitterHandle { get { throw new NotImplementedException(); } }

        public ulong _TwitterID { get { throw new NotImplementedException(); } }

        public Guid _UserID
        {
            get
            {
                var user = User;
                var Identity = user.Identity;
                var claimsIdentity = (ClaimsIdentity) Identity;

                var claims = claimsIdentity.Claims;

                var claimString = claims.FirstOrDefault(c => c.Type.Contains("stable_sid")).Value;
                var guidString = claimString.Split(':').Last();

                return Guid.Parse(guidString);
            }
        }

        public string _UserName { get { throw new NotImplementedException(); } }
    }
}
