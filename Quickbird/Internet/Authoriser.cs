namespace Quickbird.Internet
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;
    using Windows.Web.Http.Headers;
    using Newtonsoft.Json.Linq;
    using Util;

    public static class Authoriser
    {
        /// <summary>The Url of the web api that is used to fetch data.</summary>
#if LOCALSERVER
        public const string ApiUrl = "http://localhost:53953/api";

        private const int TimeoutMs = 60000;
#else
        public const string ApiUrl = "https://ghapi46azure.azurewebsites.net/api";
#endif

        public static async Task<bool> Login(string email, string password, Guid personGuid)
        {
            using (var client = CreateConfguredHttpClient(null))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{ApiUrl}/auth/token")))
                {
                    // See https://tools.ietf.org/html/rfc6749#section-4.3
                    // The "Roles" scope is a part of Asp.Net Identity,
                    // it causes the roles attached to this IdentityUser to be attached to this token.
                    request.Content =
                        new HttpFormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("username", email),
                            new KeyValuePair<string, string>("password", password),
                            new KeyValuePair<string, string>("scope", "Roles")
                        });

                    try
                    {
                        var cts = new CancellationTokenSource(TimeoutMs);
                        var response = await client.SendRequestAsync(request).AsTask(cts.Token);
                        if (!response.IsSuccessStatusCode || !response.IsSuccessStatusCode)
                        {
                            Debug.WriteLine("Loging request invalid or denied.");
                            return false;
                        }

                        try
                        {
                            var text = await response.Content.ReadAsStringAsync();
                            var json = JObject.Parse(text);
                            var token = (string) json["access_token"];
                            //var expiriy = (string)json["expires_in"];
                            //var type = (string)json["token_type"];
                            Settings.Instance.SetNewCreds(email, token, personGuid);
                            return true;
                        }
                        catch
                        {
                            Debug.WriteLine("Login request reply Json invalid");
                            return false;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        Debug.WriteLine("Login request timed-out.");
                        return false;
                    }
                    catch
                    {
                        // Any kind of network error.
                        Debug.WriteLine("Login request network failure.");
                        return false;
                    }
                }
            }
        }

        /// <summary>Configures a HttpClient with the cache disabled and authentication token in headers if
        /// supplied.</summary>
        /// <param name="token">Optional authenication token (bearer), null to disable</param>
        /// <returns>Configured HttpClient.</returns>
        private static HttpClient CreateConfguredHttpClient(string token)
        {
            var filter = new HttpBaseProtocolFilter
            {
                AllowUI = false // Disable the interactive ui for credentials.
            };
            // Disable the cache.
            filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
            filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;


            var client = new HttpClient(filter);
            if (token != null)
                client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);

            return client;
        }
    }
}
