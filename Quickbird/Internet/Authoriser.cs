using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quickbird.Internet
{
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;

    public static class Authoriser
    {
        public static bool Login(string email, string password)
        {
            var client = new HttpClient(new HttpBaseProtocolFilter {AllowUI = false});
        }
    }
}
