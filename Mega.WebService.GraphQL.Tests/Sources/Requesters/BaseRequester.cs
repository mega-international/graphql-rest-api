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
using System.Threading;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Requesters
{
    public abstract class BaseRequester : IRequester
    {
        protected static readonly string _hopexSession = "x-hopex-sessiontoken";
        protected static readonly string _hopexTask = "x-hopex-task";
        protected static readonly string _hopexWait = "x-hopex-wait";
        private readonly GraphQLClientOptions _options = new GraphQLClientOptions();
        public Uri EndPoint { get; set; }

        public abstract void SetConfig(ISessionInfos session);
        public async Task<GraphQLResponse> SendPostAsync(GraphQLRequest request, bool asyncMode)
        {
            return await SendPostAsync(request, asyncMode, CancellationToken.None);
        }

        public async Task<GraphQLResponse> SendPostAsync(GraphQLRequest request, bool asyncMode, CancellationToken cancellationToken)
        {
            CheckToken();
            var graphQLString = JsonConvert.SerializeObject(request);
            using (var httpContent = new StringContent(graphQLString))
            {
                httpContent.Headers.ContentType = _options.MediaType;
                using (var client = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan })
                {
                    PrepareHeadersForExecute(client);
                    using (var httpResponseMessage = await client.PostAsync(EndPoint, httpContent, CancellationToken.None).ConfigureAwait(false))
                    {
                        httpResponseMessage.EnsureSuccessStatusCode();
                        cancellationToken.ThrowIfCancellationRequested();
                        if (asyncMode)
                        {
                            return await GetResultAsyncModeAsync(httpResponseMessage, cancellationToken).ConfigureAwait(false);
                        }
                        return await GetResultSyncModeAsync(httpResponseMessage).ConfigureAwait(false);
                    }
                }
            }
        }

        protected virtual void CheckToken() {}
        protected virtual void SetToken(HttpClient client) { }

        protected abstract void PrepareHeadersForExecute(HttpClient client);

        protected abstract void PrepareHeadersForResult(HttpClient client, IEnumerable<string> session, IEnumerable<string> task);

        protected void SetHeadersField(HttpClient client, string name, string value)
        {
            string[] values = new string[] { value };
            SetHeadersField(client, name, values);
        }

        protected void SetHeadersField(HttpClient client, string name, IEnumerable<string> values)
        {
            var headers = client.DefaultRequestHeaders;
            headers.Remove(name);
            headers.Add(name, values);
        }

        private async Task<GraphQLResponse> GetResultSyncModeAsync(HttpResponseMessage httpResponseMessage)
        {
            var response = await ReadHttpResponseMessageAsync(httpResponseMessage).ConfigureAwait(false);
            if (response == null)
            {
                throw new NullReferenceException($"No response returned by post request: {EndPoint.PathAndQuery}");
            }
            if (response.Errors != null && response.Errors.Length > 0)
            {
                throw new HttpRequestException(response.Errors[0].Message);
            }
            return response;
        }

        private async Task<GraphQLResponse> GetResultAsyncModeAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            var headers = httpResponseMessage.Headers;
            var client = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan };
            if (headers.TryGetValues(_hopexSession, out var sessionValue) && headers.TryGetValues(_hopexTask, out var taskValue))
            {
                PrepareHeadersForResult(client, sessionValue, taskValue);
            }
            while (httpResponseMessage.StatusCode == HttpStatusCode.PartialContent)
            {
                CheckToken();
                SetToken(client);
                var content = new StringContent("{\"Query\":\"\"}");
                content.Headers.ContentType = _options.MediaType;
                httpResponseMessage = await client.PostAsync(EndPoint, content, CancellationToken.None).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            httpResponseMessage.EnsureSuccessStatusCode();
            return await GetResultSyncModeAsync(httpResponseMessage).ConfigureAwait(false);
        }

        private async Task<GraphQLResponse> ReadHttpResponseMessageAsync(HttpResponseMessage httpResponseMessage)
        {
            using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var jsonSerializer = new JsonSerializer
                {
                    ContractResolver = _options.JsonSerializerSettings.ContractResolver
                };
                try
                {
                    return jsonSerializer.Deserialize<GraphQLResponse>(jsonTextReader);
                }
                catch (JsonReaderException exception)
                {
                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        throw exception;
                    }
                    throw new GraphQLHttpException(httpResponseMessage);
                }
            }
        }

    }
}
