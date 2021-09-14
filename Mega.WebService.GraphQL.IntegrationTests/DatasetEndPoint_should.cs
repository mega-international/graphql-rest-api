using FluentAssertions;
using GraphQL;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.DTO;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    [ImportMgr("DatasetEndPoint_should.mgr")]
    public class DatasetEndPoint_should : BaseFixture
    {
        public DatasetEndPoint_should(GlobalFixture fixture, ITestOutputHelper output)
           : base(fixture, output)
        { }

        [Fact(Skip = "Not working")]
        public async Task Export_a_dataset()
        {
            var response = await ExportDataset(bustCache: true);

            await AssertDatasetLines(response, LinesWithSomeTech());
        }

        [Fact(Skip = "Not working")]
        public async void Refresh_a_dataset()
        {
            var originalResponse = await ExportDataset(bustCache: true);
            await AssertDatasetLines(originalResponse, LinesWithSomeTech());

            await ChangeApplicationTechnology("ADD", 1);

            var cachedResponse = await ExportDataset(bustCache: false);
            await AssertDatasetLines(cachedResponse, LinesWithSomeTech());

            var regeneratedResponse = await ExportDataset(bustCache: true);
            await AssertDatasetLines(regeneratedResponse, LinesWithTechEverywhere());
        }

        [Fact(Skip = "Not working")]
        public async void Export_a_dataset_asynchronously()
        {
            var asyncQuery = new AsyncQueryPlayer(_output);

            var response = await asyncQuery.PlayAsync(new DatasetAsyncQueryBuilder(this), _fx.Client);

            await AssertDatasetLines(response, LinesWithSomeTech());
        }

        class DatasetAsyncQueryBuilder : IAsyncQueryBuilder
        {
            private readonly DatasetEndPoint_should _fx;

            internal DatasetAsyncQueryBuilder(DatasetEndPoint_should fx)
            {
                _fx = fx;
            }

            public async Task<HttpRequestMessage> CreateFirstRequestAsync()
            {
                return await _fx.CreateDatasetRequestAsync(true, "async/");
            }

            public async Task<HttpRequestMessage> CreateNextRequestAsync()
            {
                return await _fx.CreateDatasetRequestAsync(false, "async/");
            }
        }

        private async Task<HttpResponseMessage> ExportDataset(bool bustCache = false)
        {
            var request = await CreateDatasetRequestAsync(bustCache);

            var response = await _fx.Client.SendAsync(request);
            return response;
        }

        private async Task<HttpRequestMessage> CreateDatasetRequestAsync(bool bustCache, string prefix = "")
        {
            var datasetId = "cwQjDcxCVz2F";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{prefix}dataset/{datasetId}/content", UriKind.Relative)
            };
            await _fx.FillHeadersAsync(request.Headers);
            if (bustCache) request.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };
            return request;
        }

        private static async Task AssertDatasetLines(HttpResponseMessage response, DatasetDTOLine[] lines)
        {
            var actualJson = await response.Content.ReadAsStringAsync();
            var actual = SafeParseJson<DatasetDTO>(actualJson);
            actual.Header.Columns.Should().BeEquivalentTo(new DatasetDTOColumn[]
            {
            new DatasetDTOColumn { Id = "AwQjrTxCV53E", Label = "Application" },
            new DatasetDTOColumn { Id = "ZxQjsUxCVXKE", Label = "Application Id" },
            new DatasetDTOColumn { Id = "nuQjiWxCVvXE", Label = "Application-1" },
            new DatasetDTOColumn { Id = "QvQjNYxCVnjE", Label = "Technology Id" },
            new DatasetDTOColumn { Id = "xwQjXYxCV9pE", Label = "Technology" },
            new DatasetDTOColumn { Id = "Nt7s)yDPVHIV", Label = "Expenses" },
            new DatasetDTOColumn { Id = "9r7sJzDPV9OV", Label = "Deployment Date" },
            new DatasetDTOColumn { Id = "Lt7swzDPVTUV", Label = "Cloud Ready Score\\CAST Highlight" }
            }, o => o.WithStrictOrdering());
            actual.Data.Should().BeEquivalentTo(lines, o => o
                .WithoutStrictOrdering()
                .Using<decimal>(ctx => ctx.Subject.Should().BeOneOf(ctx.Expectation, Math.Floor(ctx.Expectation))) // Problem in some computed attributes with money parsing
                .When(line => line.SelectedMemberPath == "Expenses"));
        }

        private static T SafeParseJson<T>(string json)
        {
            T result = default(T);
            Action parsing = () => result = JsonConvert.DeserializeObject<T>(json);
            parsing.Should().NotThrow<JsonReaderException>($"{json} should have been a valid JSON");
            return result;
        }

        private static DatasetDTOLine[] LinesWithSomeTech()
        {
            return new DatasetDTOLine[]
                {
                    new DatasetDTOLine{ Application = "DatasetEndPoint.NoTech", ApplicationId = "8vQjrOxCVbpB", Application1 = "DatasetEndPoint.NoTech",
                        Expenses = 123.45m, DeploymentDate = "2020-09-18", CloudReadyScore = 987.65d },
                    new DatasetDTOLine{ Application = "DatasetEndPoint.2Techs", ApplicationId = "XwQjGPxCVDtC", Application1 = "DatasetEndPoint.2Techs",
                        TechnologyId = "xwQjQPxCV98D", Technology = "DatasetEndPoint.Tech1",
                        Expenses = 0m },
                    new DatasetDTOLine{ Application = "DatasetEndPoint.2Techs", ApplicationId = "XwQjGPxCVDtC", Application1 = "DatasetEndPoint.2Techs",
                        TechnologyId = "nxQjWPxCV9BD", Technology = "DatasetEndPoint.Tech2",
                        Expenses = 0m }
                };
        }

        private static DatasetDTOLine[] LinesWithTechEverywhere()
        {
            return new DatasetDTOLine[]
                {
                    new DatasetDTOLine{ Application = "DatasetEndPoint.NoTech", ApplicationId = "8vQjrOxCVbpB", Application1 = "DatasetEndPoint.NoTech" ,
                        TechnologyId = "xwQjQPxCV98D", Technology = "DatasetEndPoint.Tech1",
                        Expenses = 123.45m, DeploymentDate = "2020-09-18", CloudReadyScore = 987.65d },
                    new DatasetDTOLine{ Application = "DatasetEndPoint.2Techs", ApplicationId = "XwQjGPxCVDtC", Application1 = "DatasetEndPoint.2Techs",
                        TechnologyId = "xwQjQPxCV98D", Technology = "DatasetEndPoint.Tech1",
                        Expenses = 0m },
                    new DatasetDTOLine{ Application = "DatasetEndPoint.2Techs", ApplicationId = "XwQjGPxCVDtC", Application1 = "DatasetEndPoint.2Techs",
                        TechnologyId = "nxQjWPxCV9BD", Technology = "DatasetEndPoint.Tech2",
                        Expenses = 0m }
                };
        }

        private async Task ChangeApplicationTechnology(string action, int expectedTechnologies)
        {
            var request = new GraphQLRequest()
            {
                Query = @"mutation($action:_InputCollectionActionEnum!) {
                            updateApplication(id:""8vQjrOxCVbpB"" idType:INTERNAL application: {
                                softwareTechnology_UsedTechnology: {
                                    action: $action
                                    list:[{id:""xwQjQPxCV98D"", idType:INTERNAL}]
                                }
                            }) { name softwareTechnology_UsedTechnology { name } } }",
                Variables = new { action }
            };
            var graphQLClient = await _fx.GetGraphQLClientAsync("ITPM");
            var response = await graphQLClient.SendQueryAsync<UpdateApplicationResponse>(request);
            response.Should().HaveNoError();
            response.Data.UpdateApplication.SoftwareTechnology_UsedTechnology.Should().HaveCount(expectedTechnologies);
        }

        public override async Task DisposeAsync()
        {
            await ChangeApplicationTechnology("REMOVE", 0);

            await base.DisposeAsync();
        }
    }

    public class DatasetDTO
    {
        [JsonProperty("header")]
        public DatasetDTOHeader Header { get; set; }
        [JsonProperty("data")]
        public IList<DatasetDTOLine> Data { get; set; }


    }

    public class DatasetDTOHeader
    {
        [JsonProperty("columns")]
        public IList<DatasetDTOColumn> Columns { get; set; }
    }

    public class DatasetDTOColumn
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
    }

    public class DatasetDTOLine
    {
        public string Application { get; set; }
        [JsonProperty("Application Id")]
        public string ApplicationId { get; set; }
        [JsonProperty("Application-1")]
        public string Application1 { get; set; }
        [JsonProperty("Technology Id", NullValueHandling = NullValueHandling.Ignore)]
        public string TechnologyId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Technology { get; set; }
        public decimal Expenses { get; set; }
        [JsonProperty("Deployment Date", NullValueHandling = NullValueHandling.Ignore)]
        public string DeploymentDate { get; set; }
        [JsonProperty("Cloud Ready Score\\CAST Highlight", NullValueHandling = NullValueHandling.Ignore )]
        [JsonConverter(typeof(StrictFloatConverter))]
        public double? CloudReadyScore { get; set; }
    }

    public class UpdateApplicationResponse
    {
        public Application UpdateApplication { get; set; }
        public class Application : BasicObject
        {
            public List<BasicObject> SoftwareTechnology_UsedTechnology { get; set; }
        }
    }

    public class StrictFloatConverter : JsonConverter
    {
        readonly JsonSerializer defaultSerializer = new JsonSerializer();

        public override bool CanConvert(Type objectType)
        {
            var type = Nullable.GetUnderlyingType(objectType) ?? objectType;
            return type == typeof(float) || type == typeof(double);            
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    return defaultSerializer.Deserialize(reader, objectType);
                default:
                    throw new JsonSerializationException(string.Format("Token \"{0}\" of type {1} was not a JSON float", reader.Value, reader.TokenType));
            }
        }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
