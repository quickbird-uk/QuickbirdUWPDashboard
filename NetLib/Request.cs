namespace NetLib
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;
    using JetBrains.Annotations;

    public static class Request
    {
        private const int DefaultTimeout = 10;

        /// <summary>
        ///     Fetches a table, catches errors either returns the resonse or a message starting with "Error".
        /// </summary>
        /// <param name="baseUrl">Base url of the request API.</param>
        /// <param name="tableName">The name of the table to put on the end of the baseUrl.</param>
        /// <param name="cred">A credentials object used to authenticate the request, optional.</param>
        /// <param name="canceller">A token that can be used to cancell the request.</param>
        /// <returns>The response or an error message starting with "Error:"</returns>
        public static async Task<string> RequestTable([NotNull] string baseUrl, [NotNull] string tableName,
            [CanBeNull] Creds cred = null, [CanBeNull] CancellationToken? canceller = null)
        {
            // Must add the AllowUI=false setting otherwise it tries enumerating UI and doesn't report errors properly.
            var client = new HttpClient(new HttpBaseProtocolFilter() {AllowUI = false});
            var tokenHeader = "X-ZUMO-AUTH";
            var headers = client.DefaultRequestHeaders;
            if (cred != null) headers.Add(tokenHeader, cred.Token);

            var url = UrlCombiner.CombineAsSeparateElements(baseUrl, tableName);

            if (canceller == null)
            {
                var cs = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultTimeout));
                canceller = cs.Token;
            }

            try
            {
                var response = await client.GetStringAsync(url).AsTask((CancellationToken) canceller);
                return response;
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine($"Req '{tableName}' cancelled ot timedout.");
                return "Error: Cancelled or timed out.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Req '{tableName}' error: {ex}");
                return $"Error: NetFail: {ex.Message}, {ex.InnerException}, END";
            }
        }
    }
}