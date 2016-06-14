namespace NetLib
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;
    using JetBrains.Annotations;
    using System.Net.Http.Headers;
    using Windows.Storage.Streams;
    using Windows.Security.Cryptography;

    public static class Request
    {
        private const int DefaultTimeout = 30;

        /// <summary>
        ///     Fetches a table, catches errors either returns the resonse or a message starting with "Error".
        ///     A valid message will be Json starting enad ending with either [] or {}.
        /// </summary>
        /// <param name="baseUrl">Base url of the request API.</param>
        /// <param name="tableName">The name of the table to put on the end of the baseUrl.</param>
        /// <param name="cred">A credentials object used to authenticate the request, optional.</param>
        /// <param name="canceller">A token that can be used to cancell the request.</param>
        /// <returns>The response or an error message starting with "Error:"</returns>
        public static async Task<string> GetTable([NotNull] string baseUrl, [NotNull] string tableName,
            [CanBeNull] Creds cred = null, [CanBeNull] CancellationToken? canceller = null)
        {
            // Must add the AllowUI=false setting otherwise it tries enumerating UI and doesn't report errors properly.
            var client = new HttpClient(new HttpBaseProtocolFilter {AllowUI = false});
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

        /// <summary>
        ///     Submits a POST request. Returns null on success, otherwise returns error information.
        /// </summary>
        /// <param name="baseUrl">Root URL for the API.</param>
        /// <param name="tableName">The table name to add to the end of the baseUrl.</param>
        /// <param name="data">The data payload to post.</param>
        /// <param name="cred">Authentication credentials if needed.</param>
        /// <param name="canceller">A object that can be sued to canel the request. </param>
        /// <returns>Null on success otherwise an error message.</returns>
        public static async Task<string> PostTable([NotNull] string baseUrl, [NotNull] string tableName,
            [NotNull] string data, [CanBeNull] Creds cred = null, [CanBeNull] CancellationToken? canceller = null)
        {
            // Must add the AllowUI=false setting otherwise it tries enumerating UI and doesn't report errors properly.
            var client = new HttpClient(new HttpBaseProtocolFilter {AllowUI = false});
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
                IBuffer buffUTF8 = CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8);
                IHttpContent content = new HttpBufferContent(buffUTF8, 0, buffUTF8.Length);
                var body = await content.ReadAsStringAsync(); 
                content.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/json");
                
                var response = await client.PostAsync(url, content).AsTask((CancellationToken) canceller);
                var it = response.Content; 
                return response.IsSuccessStatusCode
                    ? null
                    : $"Error: Request returned {response.StatusCode} ({response.ReasonPhrase})";
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