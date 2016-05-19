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
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdGFibGVfc2lkIjoic2lkOjY3MmQ5Mzk4ZTBmOTg0YTczM2I3OWUwYWU0NjU0M2RiIiwic3ViIjoic2lkOmIyYjI1M2JiMGU3MGViMGJhN2YwNGU1OGQwM2FiMDRjIiwiaWRwIjoidHdpdHRlciIsInZlciI6IjMiLCJpc3MiOiJodHRwczovL2doYXBpNDZhenVyZS5henVyZXdlYnNpdGVzLm5ldC8iLCJhdWQiOiJodHRwczovL2doYXBpNDZhenVyZS5henVyZXdlYnNpdGVzLm5ldC8iLCJleHAiOjE0NjM5MjczMjMsIm5iZiI6MTQ2MTMzNTMyM30.ALW45aLz40f3irlESmqnm2wrMm5WHpET2iCpUDXC2MM";
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
            var response = await Request.GetTable(MainDbContext.ApiUrl, tableName);
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

            var response = await Request.GetTable(MainDbContext.ApiUrl, tableName);
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
            var response = await Request.GetTable(MainDbContext.ApiUrl, tableName, _cred);
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
            var response = await Request.GetTable(MainDbContext.ApiUrl, "People", _cred);
            //The Json response is always surrounded by [].
            Assert.StartsWith("{\"ID\":", response);
        }

        const string SensorsHistory = "SensorsHistory";

        [Fact]
        public async Task SensorHistGetOldestDay()
        {
            var response = await Request.GetTable(MainDbContext.ApiUrl, $"{SensorsHistory}/0/1", _cred);
            Assert.Null(response);
        }
    }
}