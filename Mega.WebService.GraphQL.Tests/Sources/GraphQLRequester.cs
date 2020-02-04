using GraphQL.Client;
using GraphQL.Client.Exceptions;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources
{
    public enum QueryType : byte
    {
        Query,
        Mutation
    }
    public class GraphQLRequester
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
        private static readonly string _hopexSession = "x-hopex-sessiontoken";
        private static readonly string _hopexTask = "x-hopex-task";
        private static readonly string _hopexWait = "x-hopex-wait";
        private static readonly string _uasUrl = ConfigurationManager.AppSettings["AuthenticationUrl"];
        private static readonly string _login = ConfigurationManager.AppSettings["Login"];
        private static readonly string _password = ConfigurationManager.AppSettings["Password"];

        private readonly HttpClient _client;

        private readonly GraphQLClientOptions _options;

        private Token _token = null;

        public Uri EndPoint { get; set; }
        public string EnvironmentId { get; set; }
        public string RepositoryId { get; set; }
        public string ProfileId { get; set; }

        public GraphQLRequester(Uri uri)
        {
            _client = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            _client.DefaultRequestHeaders.Add(_hopexWait, "500");
            _options = new GraphQLClientOptions();
            EndPoint = uri;
        }

        public GraphQLRequester(string uri) : this(new Uri(uri)) { }

        public void SetUri(string uri)
        {
            EndPoint = new Uri(uri);
        }

        public void SetToken(string accessToken)
        {
            _client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {accessToken}");
        }

        public void UpdateHeader()
        {
            string newContext = $"{{\"EnvironmentId\":\"{EnvironmentId}\",\"RepositoryId\":\"{RepositoryId}\",\"ProfileId\":\"{ProfileId}\"}}";
            SetHeadersField(_hopexContext, newContext);
        }

        private void SetHeadersField(string name, string value)
        {
            string [] values = new string[] {value};
            SetHeadersField(name, values);
        }

        private void SetHeadersField(string name, IEnumerable<string> values)
        {
            var headers = _client.DefaultRequestHeaders;
            headers.Remove(name);
            headers.Add(name, values);
        }

        private void GenerateToken()
        {
            var fields = new Dictionary<string, string>();
            Task<HttpResponseMessage> response;
            string strResponse;
            fields.Add("grant_type", "password");
            fields.Add("scope", "hopex openid read write");
            fields.Add("username", _login);
            fields.Add("password", _password);
            fields.Add("client_id", "HopexAPI");
            fields.Add("client_secret", "secret");
            fields.Add("environmentId", EnvironmentId);

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_uasUrl}/connect/token"),
                Content = new FormUrlEncodedContent(fields)
            };
            response = _client.SendAsync(httpRequestMessage);
            strResponse = response.Result.Content.ReadAsStringAsync().Result;
            _token  = JsonConvert.DeserializeObject<Token>(strResponse);
            SetToken(_token.AccessToken);
        }

        public async Task<GraphQLResponse> SendPostAsync(GraphQLRequest request, bool asyncMode)
        {
            return await SendPostAsync(request, asyncMode, CancellationToken.None);
        }

        public async Task<GraphQLResponse> SendPostAsync(GraphQLRequest request, bool asyncMode, CancellationToken cancellationToken)
        {
            if(_token?.Obsolete() ?? true)
            {
                GenerateToken();
            }
            var graphQLString = JsonConvert.SerializeObject(request);
            using(var httpContent = new StringContent(graphQLString))
            {
                httpContent.Headers.ContentType = _options.MediaType;
                if(asyncMode)
                {
                    var headers = _client.DefaultRequestHeaders;
                    headers.Remove(_hopexTask);
                }
                using(var httpResponseMessage = await _client.PostAsync(EndPoint, httpContent, CancellationToken.None).ConfigureAwait(false))
                {
                    httpResponseMessage.EnsureSuccessStatusCode();
                    cancellationToken.ThrowIfCancellationRequested();
                    if(asyncMode)
                    {
                        return await GetResultAsyncModeAsync(httpResponseMessage, cancellationToken).ConfigureAwait(false);
                    }
                    return await GetResultSyncModeAsync(httpResponseMessage).ConfigureAwait(false);
                }
            }
        }

        private async Task<GraphQLResponse> GetResultSyncModeAsync(HttpResponseMessage httpResponseMessage)
        {
            var response = await ReadHttpResponseMessageAsync(httpResponseMessage).ConfigureAwait(false);
            if(response == null)
            {
                throw new NullReferenceException($"No response returned by post request: {EndPoint.PathAndQuery}");
            }
            if(response.Errors != null && response.Errors.Length > 0)
            {
                throw new HttpRequestException(response.Errors [0].Message);
            }
            return response;
        }

        private async Task<GraphQLResponse> GetResultAsyncModeAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            var headers = httpResponseMessage.Headers;
            if(headers.TryGetValues(_hopexSession, out var sessionValue) && headers.TryGetValues(_hopexTask, out var taskValue))
            {
                SetHeadersField(_hopexSession, sessionValue);
                SetHeadersField(_hopexTask, taskValue);
            }
            while(httpResponseMessage.StatusCode == HttpStatusCode.PartialContent)
            {
                if(_token?.Obsolete() ?? true)
                {
                    GenerateToken();
                }
                httpResponseMessage = await _client.PostAsync(EndPoint, null, CancellationToken.None).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }
            httpResponseMessage.EnsureSuccessStatusCode();
            return await GetResultSyncModeAsync(httpResponseMessage).ConfigureAwait(false);
        }

        private async Task<GraphQLResponse> ReadHttpResponseMessageAsync(HttpResponseMessage httpResponseMessage)
        {
            using(var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using(var streamReader = new StreamReader(stream))
            using(var jsonTextReader = new JsonTextReader(streamReader))
            {
                var jsonSerializer = new JsonSerializer
                {
                    ContractResolver = _options.JsonSerializerSettings.ContractResolver
                };
                try
                {
                    return jsonSerializer.Deserialize<GraphQLResponse>(jsonTextReader);
                }
                catch(JsonReaderException exception)
                {
                    if(httpResponseMessage.IsSuccessStatusCode)
                    {
                        throw exception;
                    }
                    throw new GraphQLHttpException(httpResponseMessage);
                }
            }
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
