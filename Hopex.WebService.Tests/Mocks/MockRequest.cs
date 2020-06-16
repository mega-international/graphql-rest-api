using System.Collections.Generic;
using Hopex.ApplicationServer.WebServices;

namespace Hopex.WebService.Tests.Mocks
{
    internal class BaseMockRequest : IHopexRequest
    {
        public string CorrelationId => "test-1";
        public IDictionary<string, string[]> Headers { get; } = new Dictionary<string, string[]>();
        public int Start => 0;
        public int Limit => 20;
        public virtual string Path => throw new System.NotImplementedException();
        public IDictionary<string, string[]> Query { get; } = new Dictionary<string, string[]>();
    }

    internal class MockraphQLRequest : BaseMockRequest
    {
        private readonly string _schema;

        public MockraphQLRequest(string schema)
        {
            _schema = schema;
        }

        public override string Path => $"/api/graphql/{_schema}";
    }
}
