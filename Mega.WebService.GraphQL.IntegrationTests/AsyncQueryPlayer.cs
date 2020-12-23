using FluentAssertions;
using FluentAssertions.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    class AsyncQueryPlayer
    {
        private readonly ITestOutputHelper _output;

        public AsyncQueryPlayer(ITestOutputHelper output)
        {
            _output = output;
        }

        internal async Task<HttpResponseMessage> PlayAsync(IAsyncQueryBuilder queryBuilder, HttpClient client)
        {
            var request = await queryBuilder.CreateFirstRequestAsync();
            
            HttpResponseMessage response = null;
            Func<Task> sendAsync = async () => response = await client.SendAsync(request);
            var callCount = 1;

            await ShouldBeFast(sendAsync);

            response.StatusCode.Should().Be(HttpStatusCode.PartialContent);
            var taskId = response.Headers.GetValues("x-hopex-task").First();
            var sessionToken = response.Headers.GetValues("x-hopex-sessiontoken").First();

            while (response.StatusCode == HttpStatusCode.PartialContent)
            {
                request = await queryBuilder.CreateNextRequestAsync();
                request.Headers.Add("x-hopex-task", taskId);
                request.Headers.Add("x-hopex-sessiontoken", sessionToken);

                await ShouldBeFast(sendAsync);
                callCount++;
            }

            _output.WriteLine($"Endpoint called {callCount} times.", callCount);
            return response;
        }

        private async Task ShouldBeFast(Func<Task> sendAsync)
        {
            var stopwatch = Stopwatch.StartNew();

            await sendAsync();

            stopwatch.Stop();
            stopwatch.Elapsed.Should().BeLessThan(200.Milliseconds());
            _output.WriteLine($"Action duration: {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    interface IAsyncQueryBuilder
    {
        Task<HttpRequestMessage> CreateFirstRequestAsync();
        Task<HttpRequestMessage> CreateNextRequestAsync();
    }
}
