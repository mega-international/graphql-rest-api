using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests.Utils
{
    [Collection("Global")]
    [Trait("Category", "Integration")]
    [Trait("Category", "SkipWhenLiveUnitTesting")]
    public class BaseFixture : IAsyncLifetime
    {

        protected readonly GlobalFixture _fx;
        protected readonly ITestOutputHelper _output;
        private readonly LogsWatcher _logWatcher;
        private DateTime _started;

        public BaseFixture(GlobalFixture fixture, ITestOutputHelper output)
        {
            _fx = fixture;
            this._output = output;
            _logWatcher = new LogsWatcher(output);
            _logWatcher.Reset();            
        }

        public Task InitializeAsync()
        {
            _started = DateTime.Now;
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            var ended = DateTime.Now;
            _output.WriteLine($"Test started {_started:O}\r\nTest ended {ended:O}\r\n");
            return _logWatcher.WriteNewLinesAsync();
        }        
    }
}
