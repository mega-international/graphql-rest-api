using FluentAssertions;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using HAS.Modules.WebService.API.IntegrationTests.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HAS.Modules.WebService.API.IntegrationTests.GraphQL
{
    public abstract class BaseTests
    {
        protected class Expectation
        {
            public readonly string _expected;
            public readonly Func<GraphQLResponse, JToken> _dataGetter;

            public Expectation(string expected, Func<GraphQLResponse, JToken> dataGetter)
            {
                _expected = expected;
                _dataGetter = dataGetter;
            }

            public void Ensure(GraphQLResponse response)
            {
                var responseJSON = _dataGetter(response);
                var expectedJSON = JToken.Parse(_expected);
                responseJSON.Should().BeEquivalentTo(expectedJSON);
            }
        }


        protected readonly ConfigurationServer _configuration = new ConfigurationServer();
        protected string ServerAdress => _configuration.Server;
        protected string ApiKey => _configuration.ApiKey;
        protected const string _schema = "ITPM";
        protected string ServerUrl => $"{ServerAdress}/hopexgraphql/api/{_schema}";

        protected async Task<GraphQLResponse> SendQuery(string query)
        {
            var request = new GraphQLRequest
            {
                Query = query
            };
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-api-key", ApiKey);
                using (var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"))
                {
                    using (var response = await httpClient.PostAsync(ServerUrl, content))
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<GraphQLResponse>(responseContent);
                    }
                }
            }
        }

        protected void EnsureNoError(GraphQLResponse response)
        {
            response.Errors.Should().BeNullOrEmpty();
        }

        protected void AssertJSONObject(JObject current, string expected)
        {
            var expectedObj = JObject.Parse(expected);
            AssertJSONObject(current, expectedObj);
        }

        protected void AssertJSONObject(JObject current, JObject expected)
        {
            current.Should().Contain(expected);
        }

        protected async Task<GraphQLResponse> EnsureSuccessAndExpected(string query, string expected)
        {
            return await EnsureSuccessAndExpected(query, expected, response => response.Data);
        }

        protected async Task<GraphQLResponse> EnsureSuccessAndExpected(string query, string expected, Func<GraphQLResponse, JToken> dataGetter)
        {
            var expectations = new List<Expectation> { new Expectation(expected, dataGetter) };
            return await EnsureSuccessAndExpected(query, expectations);
        }

        protected async Task<GraphQLResponse> EnsureSuccessAndExpected(string query, IEnumerable<Expectation> expectations)
        {
            var response = await SendQuery(query);
            EnsureNoError(response);
            foreach(var expectation in expectations)
            {
                expectation.Ensure(response);
            }
            return response;
        }
    }
}
