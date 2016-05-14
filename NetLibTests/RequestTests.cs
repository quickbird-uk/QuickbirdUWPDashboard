namespace NetLibTests
{
    using System.Threading.Tasks;
    using Agronomist.Models;
    using NetLib;
    using Xunit;

    public class RequestTests
    {
        public RequestTests()
        {
            const string token =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdGFibGVfc2lkIjoic2lkOjdhN2EwZDZlOWRjNWU0MDA2NGIwOWU0M2Y1ODM0N2EwIiwic3ViIjoic2lkOmQ5NDZiMWNiYzY2M2Y1NTI4MTdkMjQ1NGJhMDljZmQ3IiwiaWRwIjoidHdpdHRlciIsInZlciI6IjMiLCJpc3MiOiJodHRwczovL2doYXBpNDZhenVyZS5henVyZXdlYnNpdGVzLm5ldC8iLCJhdWQiOiJodHRwczovL2doYXBpNDZhenVyZS5henVyZXdlYnNpdGVzLm5ldC8iLCJleHAiOjE0NjUxMDAxNDIsIm5iZiI6MTQ2MjUwODE0Mn0.0abeyMVXIPpnTP1Llt0Ct6QItGCPjZUQPSmkL7hQ5gc";
            _cred = Creds.FromUserIdAndToken("fakeuser", token);
        }

        private readonly Creds _cred;

        /// <summary>
        ///     Requests that don't requires authorisation and therefore should always work.
        /// </summary>
        [Theory]
        [InlineData("Parameters")]
        [InlineData("Placements")]
        [InlineData("RelayTypes")]
        [InlineData("Parameters")]
        [InlineData("SensorTypes")]
        [InlineData("Subsystems")]
        [InlineData("CropTypes")]
        public async Task NoAuthGetTest(string tableName)
        {
            var response = await Request.RequestTable(MainDbContext.ApiUrl, tableName);
            //The Json response is always surrounded by [].
            Assert.StartsWith("[", response);
            Assert.EndsWith("]", response);
        }

        /// <summary>
        ///     Tests for accessing table that require auth reurning the correct error.
        /// </summary>
        [Theory]
        [InlineData("CropCycles")]
        [InlineData("Devices")]
        [InlineData("Locations")]
        [InlineData("Relays")]
        [InlineData("Sensors")]
        [InlineData("People")]
        public async Task UnauthorisedGetTest(string tableName)
        {
            const string expected = "Unauthorized (401)";

            var response = await Request.RequestTable(MainDbContext.ApiUrl, tableName);
            Assert.Contains(expected, response);
        }

        /// <summary>
        ///     Tests for accessing table that require auth reurning the correct error.
        /// </summary>
        [Theory]
        [InlineData("CropCycles")]
        [InlineData("Devices")]
        [InlineData("Locations")]
        [InlineData("Relays")]
        [InlineData("Sensors")]
        public async Task AuthorisedGetTest(string tableName)
        {
            var response = await Request.RequestTable(MainDbContext.ApiUrl, tableName, _cred);
            //The Json response is always surrounded by [].
            Assert.StartsWith("[", response);
            Assert.EndsWith("]", response);
        }

        /// <summary>
        ///     People is the only table that does not return a list, so different response.
        /// </summary>
        [Fact]
        public async Task PeopleGetTest()
        {
            var response = await Request.RequestTable(MainDbContext.ApiUrl, "People", _cred);
            //The Json response is always surrounded by [].
            Assert.StartsWith("{\"ID\":", response);
        }
    }
}