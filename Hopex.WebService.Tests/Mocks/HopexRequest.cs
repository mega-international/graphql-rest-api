using System.Collections.Generic;
using Hopex.ApplicationServer.WebServices;

namespace Hopex.WebService.Tests.Mocks
{
    internal class HopexRequest : IHopexRequest
    {
        private readonly string _schema;

        public HopexRequest(string schema)
        {
            _schema = schema;
        }

        public string CorrelationId => "test-1";
        public IDictionary<string, string[]> Headers { get; } = new Dictionary<string, string[]>();
        public int Start => 0;
        public int Limit => 20;
        public string Path => $"/api/graphql/{_schema}";
        public IDictionary<string, string[]> Query { get; } = new Dictionary<string, string[]>();
    }
}
