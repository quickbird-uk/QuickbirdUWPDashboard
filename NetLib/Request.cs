namespace NetLib
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Web.Http;
    using JetBrains.Annotations;

    public static class Request
    {
        private const int DefaultTimeout = 10;

        public static async Task<string> RequestTable([NotNull] string baseUrl, [NotNull] string tableName,
            [CanBeNull] Creds cred = null, [CanBeNull] CancellationToken? canceller = null)
        {
            var client = new HttpClient();
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
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Req '{tableName}' error: {ex.ToString()}");
                return null;
            }
        }
    }
}