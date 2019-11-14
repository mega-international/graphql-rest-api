using GraphQL.Client;
using GraphQL.Client.Exceptions;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Models
{
    public enum QueryType : byte
    {
        Query,
        Mutation
    }
    public class GraphQLRequester
    {
        private static readonly string hopexContext = "x-hopex-context";
        private static readonly string hopexSession = "x-hopex-sessiontoken";
        private static readonly string hopexTask = "x-hopex-task";
        private static readonly string hopexWait = "x-hopex-wait";

        private readonly HttpClient client;

        private readonly GraphQLClientOptions options;

        public Uri EndPoint { get; set; }
        public string EnvironmentId { get; set; }
        public string RepositoryId { get; set; }
        public string ProfileId { get; set; }

        public GraphQLRequester(Uri uri)
        {
            client = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            client.DefaultRequestHeaders.Add(hopexWait, "500");
            options = new GraphQLClientOptions();
            EndPoint = uri;
        }

        public GraphQLRequester(string uri) : this(new Uri(uri)) { }

        public void SetUri(string uri)
        {
            EndPoint = new Uri(uri);
        }

        public void SetToken(string token)
        {
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}");
        }

        public void UpdateHeader()
        {
            string newContext = $"{{\"EnvironmentId\":\"{EnvironmentId}\",\"RepositoryId\":\"{RepositoryId}\",\"ProfileId\":\"{ProfileId}\"}}";
            SetHeadersField(hopexContext, newContext);
        }

        private void SetHeadersField(string name, string value)
        {
            string [] values = new string[] {value};
            SetHeadersField(name, values);
        }

        private void SetHeadersField(string name, IEnumerable<string> values)
        {
            var headers = client.DefaultRequestHeaders;
            headers.Remove(name);
            headers.Add(name, values);
        }

        public async Task<GraphQLResponse> SendPostAsync(GraphQLRequest request, bool asyncMode)
        {
            var graphQLString = JsonConvert.SerializeObject(request);
            using(var httpContent = new StringContent(graphQLString))
            {
                httpContent.Headers.ContentType = options.MediaType;
                if(asyncMode)
                {
                    var headers = client.DefaultRequestHeaders;
                    headers.Remove(hopexTask);
                }
                using(var httpResponseMessage = await client.PostAsync(EndPoint, httpContent, CancellationToken.None).ConfigureAwait(false))
                {
                    httpResponseMessage.EnsureSuccessStatusCode();
                    if(asyncMode)
                    {
                        return await GetResultAsyncMode(httpResponseMessage).ConfigureAwait(false);
                    }
                    return await GetResultSyncMode(httpResponseMessage).ConfigureAwait(false);
                }
            }
        }

        private async Task<GraphQLResponse> GetResultSyncMode(HttpResponseMessage httpResponseMessage)
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

        private async Task<GraphQLResponse> GetResultAsyncMode(HttpResponseMessage httpResponseMessage)
        {
            var headers = httpResponseMessage.Headers;
            if(headers.TryGetValues(hopexSession, out var sessionValue) && headers.TryGetValues(hopexTask, out var taskValue))
            {
                SetHeadersField(hopexSession, sessionValue);
                SetHeadersField(hopexTask, taskValue);
            }
            while(httpResponseMessage.StatusCode == HttpStatusCode.PartialContent)
            {
                httpResponseMessage = await client.PostAsync(EndPoint, null, CancellationToken.None).ConfigureAwait(false);
            }
            httpResponseMessage.EnsureSuccessStatusCode();
            var resultContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<GraphQLResponse>(resultContent);
        }

        private async Task<GraphQLResponse> ReadHttpResponseMessageAsync(HttpResponseMessage httpResponseMessage)
        {
            using(var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using(var streamReader = new StreamReader(stream))
            using(var jsonTextReader = new JsonTextReader(streamReader))
            {
                var jsonSerializer = new JsonSerializer
                {
                    ContractResolver = options.JsonSerializerSettings.ContractResolver
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
            for(int idx = 1; idx <= (inValues?.Count ?? 1); ++idx)
            {
                //begin of application output, ex: n1: createApplication
                requestContent += $"n{idx}: {requestName}";

                //parameters, ex: (id: "12345", shortName: "Toto")
                if((inValues?.Count ?? 0) > 0)
                {
                    var currentInValues = inValues [idx - 1];
                    requestContent += "(\n";
                    bool first = true;
                    foreach(var input in currentInValues)
                    {
                        if(!first)
                            requestContent += ", ";
                        requestContent += $"{input.Key}: {input.Value}\n";
                        first = false;
                    }
                    requestContent += ")\n";
                }

                //body with fragmentName, ex: { ...applicationFragment } 
                requestContent += $"{{\n  ...{fragmentName}\n }}\n";
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
                requestContent += $" {output} \n";
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
