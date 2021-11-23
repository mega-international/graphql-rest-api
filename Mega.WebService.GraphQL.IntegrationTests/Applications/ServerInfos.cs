using System;

namespace Mega.WebService.GraphQL.IntegrationTests.Applications
{
    internal struct ServerInfos
    {
        public string Scheme { get; private set; }
        public string Host { get; private set; }
        public string AdminKey { get; private set; }
        public ServerInfos(string scheme, string host, string adminKey)
        {
            Scheme = scheme;
            Host = host;
            AdminKey = adminKey;
        }
        public Uri CreateUri(string options)
        {
            return new Uri($"{Scheme}://{Host}/{options}");
        }
    }
}
