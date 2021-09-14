using FluentAssertions;
using GraphQL;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    [ImportMgr("DiagramEndPoint_should.Export_a_diagram.mgr")]
    public class DiagramEndPoint_should : BaseFixture
    {
        public DiagramEndPoint_should(GlobalFixture fixture, ITestOutputHelper output)
            :base(fixture, output)
        {}

        private const string DIAGRAM_ID = "a78WG3eOUHvF";

        [Fact]
        public async void Export_a_diagram()
        {
            var request = await CreateDiagramRequestAsync();

            var response = await _fx.Client.SendAsync(request);

            await response.Should().BeDiagramAsync();
        }

        [Fact]
        public async void Export_a_diagram_asynchronously()
        {
            var diagram = await GetDiagramAsync();
            var uri = new Uri(diagram.DownloadUrl.Replace("api/", "api/async/"));
            var asyncQuery = new AsyncQueryPlayer(_output);

            var response = await asyncQuery.PlayAsync(new DiagramAsyncQueryBuilder(this, uri), _fx.Client);

            await response.Should().BeDiagramAsync();
        }

        [Fact]
        public async void List_describing_diagrams_with_custom_properties()
        {
            var request = new GraphQLRequest()
            {
                Query = @"query appWithDiagram {
                            application(filter:{ id:""bsROZBMFU1tD""}) {
                                id
                                name
                                diagram {
                                    id
                                    name
                                    step: customField(id:""8aZ5wO15L100"")
                                    describedOcc: customField(id:""8un6Aj15L500"")
                                }
                            }
                          }"
            };
            var graphQLClient = await _fx.GetGraphQLClientAsync("ITPM");

            var response = await graphQLClient.SendQueryAsync<ApplicationWithDiagramsResponse>(request);

            response.Should().HaveNoError();
            var applications = response.Data.Application;
            applications.Should().HaveCount(1);
            applications[0].Id.Should().Be("bsROZBMFU1tD");
            applications[0].Diagram.Should().HaveCount(1);
            applications[0].Diagram[0].Id.Should().Be(DIAGRAM_ID);
            applications[0].Diagram[0].Step.Should().Be("1");
            applications[0].Diagram[0].DescribedOcc.Should().Be("bsROZBMFU1tD");
        }

        private async Task<HttpRequestMessage> CreateDiagramRequestAsync(Uri uri = null)
        {
            var requestUri = uri ?? new Uri($"diagram/{DIAGRAM_ID}/image", UriKind.Relative);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = requestUri
            };
            await _fx.FillHeadersAsync(request.Headers);
            request.Headers.Accept.ParseAdd("image/svg+xml");
            return request;
        }

        private async Task<DiagramListResponse.DiagramInstance> GetDiagramAsync()
        {
            var request = new GraphQLRequest()
            {
                Query = $"query {{diagram(filter:{{id: \"{DIAGRAM_ID}\"}}) {{ id, downloadUrl }} }}"
            };
            var graphQLClient = await _fx.GetGraphQLClientAsync("ITPM");
            var response = await graphQLClient.SendQueryAsync<DiagramListResponse>(request);
            response.Should().HaveNoError();

            return response.Data.Diagram[0];
        }

        public class ApplicationWithDiagramsResponse
        {
            public List<ApplicationWithDiagrams> Application { get; set; }

            public class ApplicationWithDiagrams
            {
                public string Id { get; set; }
                public List<DiagramInstance> Diagram { get; set; }

                public class DiagramInstance
                {
                    public string Id { get; set; }
                    public string Step { get; set; }
                    public string DescribedOcc { get; set; }
                }
            }
        }

        class DiagramAsyncQueryBuilder : IAsyncQueryBuilder
        {
            private readonly DiagramEndPoint_should _fx;
            private readonly Uri _uri;

            internal DiagramAsyncQueryBuilder(DiagramEndPoint_should fx, Uri uri)
            {
                _fx = fx;
                _uri = uri;
            }

            public async Task<HttpRequestMessage> CreateFirstRequestAsync()
            {
                return await _fx.CreateDiagramRequestAsync(_uri);
            }

            public async Task<HttpRequestMessage> CreateNextRequestAsync()
            {
                return await CreateFirstRequestAsync();
            }
        }
    }

    public class DiagramListResponse
    {
        public DiagramInstance[] Diagram { get; set; }

        public class DiagramInstance
        {
            public string Id { get; set; }
            public string DownloadUrl { get; set; }
        }
    }
}
