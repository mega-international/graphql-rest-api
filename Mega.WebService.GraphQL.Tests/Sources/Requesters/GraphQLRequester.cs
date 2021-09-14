using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Mega.WebService.GraphQL.Tests.Sources.Requesters
{
    public enum QueryType : byte
    {
        Query,
        Mutation
    }
    public class GraphQLRequester : BaseRequester
    { 
        private class Token
        {
            [JsonProperty("access_token")]
            public readonly string AccessToken;

            [JsonProperty("expires_in")]
            public readonly long Delay;

            private readonly Stopwatch stopwatch = new Stopwatch();

            public Token(string accessToken, long delay)
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
                return (Delay * 900  <= stopwatch.ElapsedMilliseconds);
            }
        }

        private static readonly string _hopexContext = "x-hopex-context";
        private static readonly string _uasUrl = ConfigurationManager.AppSettings["AuthenticationUrl"];
        private static readonly object _locker = new object();

        public string Login { get; set; } = ConfigurationManager.AppSettings["Login"];
        public string Password { get; set; } = ConfigurationManager.AppSettings["Password"];
        public string EnvironmentId { get; set; }
        public string RepositoryId { get; set; }
        public string ProfileId { get; set; }

        private Token _token = null;

        public GraphQLRequester(Uri uri)
        {
            EndPoint = uri;
        }

        public GraphQLRequester(string uri) : this(new Uri(uri)) { }

        public override void SetConfig(ISessionInfos sessionInfos)
        {
            if(sessionInfos is SessionInfos oldSessionInfos)
            {
                EnvironmentId = oldSessionInfos.Environment;
                RepositoryId = oldSessionInfos.Repository;
                ProfileId = oldSessionInfos.Profile;
            }
        }

        protected override void PrepareHeadersForExecute(HttpClient client)
        {
            SetToken(client);
            string newContext = $"{{\"EnvironmentId\":\"{EnvironmentId}\",\"RepositoryId\":\"{RepositoryId}\",\"ProfileId\":\"{ProfileId}\"}}";
            SetHeadersField(client, _hopexContext, newContext);
            SetHeadersField(client, _hopexWait, "500");
        }

        protected override void PrepareHeadersForResult(HttpClient client, IEnumerable<string> session, IEnumerable<string> task)
        {
            SetToken(client);
            SetHeadersField(client, _hopexWait, "500");
            SetHeadersField(client, _hopexSession, session);
            SetHeadersField(client, _hopexTask, task);
        }

    private void GenerateToken()
        {
            var fields = new Dictionary<string, string>()
            {
                { "grant_type", "password" },
                { "scope", "hopex openid read write" },
                { "username", Login },
                { "password", Password },
                { "client_id", "HopexAPI" },
                { "client_secret", "secret" },
                { "environmentId", EnvironmentId }
            };
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_uasUrl}/connect/token"),
                Content = new FormUrlEncodedContent(fields)
            };
            var client = new HttpClient();
            var response = client.SendAsync(httpRequestMessage);
            var strResponse = response.Result.Content.ReadAsStringAsync().Result;
            _token  = JsonConvert.DeserializeObject<Token>(strResponse);
            if(_token.AccessToken == null)
            {
                throw new HttpRequestException($"Token generation failed on login: {Login} and password {Password}");
            }
        }

        protected override void CheckToken()
        {
            lock(_locker)
            {
                if (_token?.Obsolete() ?? true)
                {
                    GenerateToken();
                }
            }
        }

        protected override void SetToken(HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {_token.AccessToken}");
        }

        private static string GenerateRequest(QueryType queryType,
                                                string requestName,
                                                List< Dictionary<string, string> > inValues,
                                                List<string> outValues,
                                                string classReturned)
        {
            string fragmentName = $"{classReturned}Fragment";

            //begin, ex: mutation{
            string requestContent = queryType == QueryType.Mutation ? "mutation{\n" : "query{\n";

            /*
             * List of result: ex:  n1: createApplication( __params__){ ...fragmentName }
             *                      n2: createApplication( __params__){ ...fragmentName }
             *                      ...
             */
            for(int idx = 0; idx < (inValues?.Count ?? 1); ++idx)
            {
                //begin of application output, ex: n1: createApplication
                requestContent += $"n{idx+1}: {requestName}";

                //parameters, ex: (id: "12345", shortName: "Toto")
                if((inValues?.Count ?? 0) > 0)
                {
                    var currentInValues = inValues [idx];
                    requestContent += "(";
                    var parameters = "";
                    foreach(var input in currentInValues)
                    {
                        parameters += $",\n{input.Key}: {input.Value}";
                    }
                    requestContent += $"{parameters.Remove(0, 1)})\n";
                }

                //body with fragmentName, ex: { ...applicationFragment } 
                requestContent += $"{{...{fragmentName}}}\n";
            }
            requestContent += "}\n";

            /*
             * Fragment part, example:  fragment ApplicationFragment on Application
             *                          {
             *                              id
             *                              name
             *                          }
             */
            requestContent += $"fragment {fragmentName} on {classReturned}\n{{";
            foreach(var output in outValues)
            {
                requestContent += $"{output}\n";
            }
            requestContent += "}";
            return requestContent;
        }

        public static string GenerateRequestMultiArgs(QueryType queryType,
                                                    string requestName,
                                                    List<Dictionary<string, string>> inValues,
                                                    List<string> outValues,
                                                    string classReturned)

        {
            return GenerateRequest(queryType, requestName, inValues, outValues, classReturned);
        }

        public static string GenerateRequestOneArg(QueryType queryType,
                                                    string requestName,
                                                    Dictionary<string, string> inValues,
                                                    List<string> outValues,
                                                    string classReturned)

        {

            List<Dictionary<string, string>> inList = new List<Dictionary<string, string>>()
            {
                inValues
            };
            return GenerateRequest(queryType, requestName, inList, outValues, classReturned);
        }

        public static string GenerateRequestNoArg(QueryType queryType,
                                                    string requestName,
                                                    List<string> outValues,
                                                    string classReturned)
        {
            return GenerateRequest(queryType, requestName, null, outValues, classReturned);
        }
    }
}
