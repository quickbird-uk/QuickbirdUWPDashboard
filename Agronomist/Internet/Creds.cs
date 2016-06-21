namespace Agronomist.Internet
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Data.Json;
    using Windows.Security.Authentication.Web;

    /// <summary>
    ///     Authentication token, associated information and broker to create it.
    /// </summary>
    public class Creds
    {
        private Creds(string userId, string token)
        {
            Token = token;
            Userid = userId;

            var parts = Token.Split('.');
            //var header = PadAndParseBase64String(parts[0]); Happens to be useless too.
            var body = PadAndParseBase64String(parts[1]);
            //var signature = parts[2]; This is of no use but its nice to know its there.
            var json = JsonObject.Parse(body);

            try
            {
                var expUnix = json.GetNamedNumber("exp");
                Expiry = DateTimeOffset.FromUnixTimeSeconds((long) expUnix);
            }
            catch (Exception ex)
            {
                throw new Exception("Token has no expiry date.", ex);
            }

            try
            {
                var issuedUnix = json.GetNamedNumber("nbf");
                Start = DateTimeOffset.FromUnixTimeSeconds((long) issuedUnix);
            }
            catch (Exception)
            {
                Start = null;
            }

            StableSid = JsonOptional(json, "stable_sid");
            Subject = JsonOptional(json, "sub");
            IdentityProvider = JsonOptional(json, "idp");
            Version = JsonOptional(json, "ver");
            Issuer = JsonOptional(json, "iss");
            Audience = JsonOptional(json, "aud");
        }

        /// <summary>
        ///     The not before date for the token. Taken is meant to be valid starting from this date.
        /// </summary>
        public DateTimeOffset? Start { get; set; }

        /// <summary>
        ///     The entire raw token to be used for authentication.
        /// </summary>
        public string Token { get; }

        /// <summary>
        ///     The expiry date of the token.
        /// </summary>
        public DateTimeOffset Expiry { get; }

        /// <summary>
        ///     The SID as reported outside of the token (should be the same as the token subject) unique to the user.
        /// </summary>
        public string Userid { get; }

        /// <summary>
        ///     The identity provider (i.e. Twitter).
        /// </summary>
        public string IdentityProvider { get; }

        /// <summary>
        ///     A SID unique to the user-identity pair, this will differ if the same user uses multiple identity providers.
        /// </summary>
        public string StableSid { get; }

        /// <summary>
        ///     The SID of the user that the token is issued for.
        /// </summary>
        public string Subject { get; }

        /// <summary>
        ///     The site that issued the token.
        /// </summary>
        public string Issuer { get; }

        /// <summary>
        ///     The site intened to use use the token.
        /// </summary>
        public string Audience { get; }

        /// <summary>
        ///     Dunno
        /// </summary>
        public string Version { get; }

        /// <summary>
        ///     Ignores json exeption and returns null when json is missing.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private string JsonOptional(JsonObject json, string id)
        {
            string res;
            try
            {
                res = json.GetNamedString(id);
            }
            catch (Exception)
            {
                return null;
            }
            return res;
        }

        /// <summary>
        ///     This will throw an exception of some sort if any step fails.
        /// </summary>
        /// <param name="entryUrl"></param>
        /// <param name="resultUrl"></param>
        /// <returns></returns>
        public static async Task<Creds> FromBroker(string entryUrl, string resultUrl)
        {
            var res =
                await
                    WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, new Uri(entryUrl),
                        new Uri(resultUrl));
            if (res.ResponseStatus == WebAuthenticationStatus.Success)
            {
                var url = Uri.UnescapeDataString(res.ResponseData);
                const string doneToken = "done#token=";
                var authResult = url.Substring(url.IndexOf(doneToken, StringComparison.Ordinal) + doneToken.Length);

                var json = JsonObject.Parse(authResult).GetObject();
                var token = json.GetNamedString("authenticationToken");
                var userId = json.GetNamedObject("user").GetNamedString("userId");
                return new Creds(userId, token);
            }
            if (res.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
            {
                throw new Exception("HTTP error from authenticator: " + res.ResponseErrorDetail);
            }
            throw new Exception("Error from authenticator: " + res.ResponseStatus);
        }

        /// <summary>
        ///     Instant, catches exeptions and returns null on failure.
        /// </summary>
        /// <param name="userId">The SID, (should be same as token subject.</param>
        /// <param name="token">The three part token (header, body, signature).</param>
        /// <returns>A Creds object, or null.</returns>
        public static Creds FromUserIdAndToken(string userId, string token)
        {
            try
            {
                return new Creds(userId, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to make creds...");
                Debug.WriteLine(userId);
                Debug.WriteLine(token);
                Debug.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Pads out base 64 strings with `=` to make sure they are a multiple of 4 in length.
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        private string PadAndParseBase64String(string base64String)
        {
            var len = base64String.Length;
            var rem = len%4;
            if (rem > 0)
            {
                base64String = base64String.PadRight(len + (4 - rem), '=');
            }
            Debug.Assert(base64String.Length%4 == 0);
            var based = Convert.FromBase64String(base64String);
            var converted = Encoding.UTF8.GetString(based);
            return converted;
        }
    }
}