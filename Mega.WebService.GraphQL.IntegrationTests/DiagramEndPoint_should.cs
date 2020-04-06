using FluentAssertions;
using FluentAssertions.Extensions;
using GraphQL;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    public class DiagramEndPoint_should : BaseFixture, IClassFixture<DiagramEndPointFixture>
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

            var request = await CreateDiagramRequestAsync(uri);
            HttpResponseMessage response = null;
            Func<Task> sendAsync = async () => response = await _fx.Client.SendAsync(request);
            var callCount = 1;

            await ShouldBeFast(sendAsync);

            response.StatusCode.Should().Be(HttpStatusCode.PartialContent);
            var taskId = response.Headers.GetValues("x-hopex-task").First();
            var sessionToken = response.Headers.GetValues("x-hopex-sessiontoken").First();

            while (response.StatusCode == HttpStatusCode.PartialContent)
            {
                request = await CreateDiagramRequestAsync(uri);
                request.Headers.Add("x-hopex-task", taskId);
                request.Headers.Add("x-hopex-sessiontoken", sessionToken);

                await ShouldBeFast(sendAsync);
                callCount++;
            }

            _output.WriteLine($"Endpoint called {callCount} times.", callCount);
            await response.Should().BeDiagramAsync();
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

        private async Task ShouldBeFast(Func<Task> sendAsync)
        {
            var stopwatch = Stopwatch.StartNew();

            await sendAsync();

            stopwatch.Stop();
            stopwatch.Elapsed.Should().BeLessThan(200.Milliseconds());
            _output.WriteLine($"Action duration: {stopwatch.ElapsedMilliseconds} ms");
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
                                diagram_Description {
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
            applications[0].Diagram_Description.Should().HaveCount(1);
            applications[0].Diagram_Description[0].Id.Should().Be(DIAGRAM_ID);
            applications[0].Diagram_Description[0].Step.Should().Be("1");
            applications[0].Diagram_Description[0].DescribedOcc.Should().Be("bsROZBMFU1tD");
        }

        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Local deserialization only")]
        public class ApplicationWithDiagramsResponse
        {
            public List<ApplicationWithDiagrams> Application { get; set; }

            public class ApplicationWithDiagrams
            {
                public string Id { get; set; }
                public List<DiagramInstance> Diagram_Description { get; set; }

                public class DiagramInstance
                {
                    public string Id { get; set; }
                    public string Step { get; set; }
                    public string DescribedOcc { get; set; }
                }
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


    [Collection("Global")]
    public class DiagramEndPointFixture
    {
        public DiagramEndPointFixture(GlobalFixture fx)
        {
            fx.MgrImporter.Import("DiagramEndPoint_should.Export_a_diagram.mgr");
        }
    }

}
