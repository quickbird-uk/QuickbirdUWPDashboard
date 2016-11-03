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

    /// <summary>Wraps up requests to the server.</summary>
    public static class ServerRequest
    {
        /// <summary>The Url of the web api that is used to fetch data.</summary>
#if LOCALSERVER
        public const string ApiUrl = "http://localhost:53953/api";

        private const int TimeoutMs = 60000;
#else
        public const string ApiUrl = "https://ghapi46azure.azurewebsites.net/api";
#endif

        /// <summary>Sends login request, returning error message or null when credentials sucessfully set.</summary>
        /// <param name="email">The username (which should be an email address).</param>
        /// <param name="password">The password.</param>
        /// <returns>Null when credentials set otherwise an error message.</returns>
        public static async Task<string> Login(string email, string password)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri($"{ApiUrl}/auth/token")))
            {
                // See https://tools.ietf.org/html/rfc6749#section-4.3
                // The "Roles" scope is a part of Asp.Net Identity,
                // it causes the roles attached to this IdentityUser to be attached to this token.
                requestMessage.Content =
                    new HttpFormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("username", email),
                        new KeyValuePair<string, string>("password", password),
                        new KeyValuePair<string, string>("scope", "Roles")
                    });

                using (var response = await Request(requestMessage, null, "Login Request"))
                {
                    if (response == null)
                        return "Network error.";

                    if (!response.IsSuccessStatusCode)
                        return "Login failed.";

                    string token;
                    try
                    {
                        var text = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(text);
                        token = (string) json["access_token"];
                        //var expiriy = (string)json["expires_in"];
                        //var type = (string)json["token_type"];
                    }
                    catch
                    {
                        LogError("Login Request Response", "Login request reply Json invalid");
                        return "Login response error, contact support if this error persists.";
                    }

                    var whoami = new HttpRequestMessage(HttpMethod.Get, new Uri($"{ApiUrl}/AuthManage/WhoAmI"));

                    using (var whoAmIResponse = await Request(whoami, token, "WhoAmI Request"))
                    {
                        if (!whoAmIResponse.IsSuccessStatusCode)
                        {
                            LogError("WhoAmI Request Response", "Auth seems to have failed.");
                            return "Login ID error, contact support if this error persists.";
                        }

                        Guid personGuid;
                        try
                        {
                            var guidText = await whoAmIResponse.Content.ReadAsStringAsync();
                            personGuid = new Guid(guidText);
                        }
                        catch (Exception e)
                        {
                            LogError("WhoAmI Request Response", "Guid decode failed.", e);
                            return "Login ID decode error, contact support if this error persists.";
                        }

                        Settings.Instance.SetNewCreds(email, token, personGuid);
                        return null;
                    }
                }
            }
        }

        /// <summary>A debug error logger, should be replaced with a proper logger eventually.</summary>
        /// <param name="category">Method name or action.</param>
        /// <param name="error">Error description.</param>
        /// <param name="e">Related exeption if it exists.</param>
        private static void LogError(string category, string error, Exception e = null)
        {
            if (e == null)
                Debug.WriteLine($"{category}: {error}");
            else
                Debug.WriteLine($"{category}: {error} ({e.Message})");
        }

        /// <summary>Sends request returning null on all network or timeout errors.</summary>
        /// <param name="message">The request.</param>
        /// <param name="token">Optional auth token, null if not used.</param>
        /// <param name="errorCategory">Name for the request to make error log messages more specific.</param>
        /// <returns>Response or null on error.</returns>
        private static async Task<HttpResponseMessage> Request(HttpRequestMessage message, string token,
            string errorCategory = "Request")
        {
            using (var client = CreateConfguredHttpClient(token))
            {
                try
                {
                    var cts = new CancellationTokenSource(TimeoutMs);
                    return await client.SendRequestAsync(message).AsTask(cts.Token);
                }
                catch (TaskCanceledException e)
                {
                    LogError(errorCategory, "Timeout.", e);
                    return null;
                }
                catch (Exception e)
                {
                    // Any kind of network error.
                    LogError(errorCategory, "Network error.", e);
                    return null;
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
