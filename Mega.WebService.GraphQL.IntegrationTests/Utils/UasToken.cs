using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.IntegrationTests.Utils
{
    class UasToken
    {
        [JsonProperty("access_token")]
        public readonly string AccessToken;

        [JsonProperty("expires_in")]
        public readonly long Delay;

        private readonly Stopwatch stopwatch = new Stopwatch();

        public UasToken(string accessToken, long delay)
        {
            AccessToken = accessToken;
            Delay = delay;
            stopwatch.Start();
        }

        public bool Expired()
        {
            return (Delay * 1000 <= stopwatch.ElapsedMilliseconds);
        }

        public long RemainingDelay()
        {
            var remaining = Delay * 1000 - stopwatch.ElapsedMilliseconds;
            return remaining > 0 ? remaining : 0;
        }

        public bool Obsolete()
        {
            return (Delay * 900 <= stopwatch.ElapsedMilliseconds);
        }

        public static async Task<UasToken> CreateAsync(GlobalFixture ctx)
        {
            var fields = new Dictionary<string, string>();
            string strResponse;
            fields.Add("grant_type", "password");
            fields.Add("scope", "hopex openid read write");
            fields.Add("username", ctx.Login);
            fields.Add("password", ctx.Password);
            fields.Add("client_id", "HopexAPI");
            fields.Add("client_secret", "secret");
            fields.Add("environmentId", ctx.EnvironmentId);
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = ctx.TokenUri,
                Content = new FormUrlEncodedContent(fields)
            };
            var response = await _client.SendAsync(httpRequestMessage);
            strResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UasToken>(strResponse);
        }

        public static UasToken NO_TOKEN = new UasToken("", 0);

        private static HttpClient _client = new HttpClient();
    }
}
