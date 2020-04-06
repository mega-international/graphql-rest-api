using GraphQL.Client.Http;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using MegaMapp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Xunit;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    public class GlobalFixture : IDisposable
    {
        private string _sqlConnectionString = "User=sa,Type=SQLSERVER,SERVER=(local)\\SQL2017;UID=sa;PWD=mega";

        private readonly string _scheme = "http";
        private readonly string _host = "localhost";
        private const string _tokenPath = "UAS/connect/token";
        private const string _apiPath = "HOPEXGraphQL/api/";
        private const string CACHED_CONFIG_FILE = "cachedConfig.json";

        public Uri TokenUri => new Uri($"{_scheme}://{_host}/{_tokenPath}");
        private Uri _apiUri => new Uri($"{_scheme}://{_host}/{_apiPath}");

        public string Login => "scr";
        private string _personSystem => "CRONIER Sébastien";
        public string Password => "Hopex";

        private string _preferredEnvironmentPartialName = "EnvTestsLab";
        private string _repositoryName = "GraphQLIntegrationTests";
        private string _profileId = "757wuc(SGjpJ"; // Hopex Customizer

        private MegaDatabase _megaDatabase;
        private string _repositoryId;
        public string EnvironmentId { get; private set; }
        public string ContextHeader => $"{{\"EnvironmentId\":\"{EnvironmentId}\",\"RepositoryId\":\"{_repositoryId}\",\"ProfileId\":\"{_profileId}\"}}";

        public HttpClient Client { get; private set; }
        public MgrImporter MgrImporter { get; private set; }

        private UasToken _uasToken = UasToken.NO_TOKEN;
        private Dictionary<string, GraphQLHttpClient> _graphQLClientDictionary = new Dictionary<string, GraphQLHttpClient>();

        public GlobalFixture()
        {
            var useHttps = Environment.GetEnvironmentVariable("HopexUseHttps");
            if (!string.IsNullOrEmpty(useHttps))
            {
                _scheme = "https";
                _host = GetFullyQualifiedDomainName();
            }

            if (IsRunWithoutInitialization())
            {
                var settings = JsonConvert.DeserializeObject<CachedSettings>(File.ReadAllText(CACHED_CONFIG_FILE));
                EnvironmentId = settings.EnvironmentId;
                _repositoryId = settings.RepositoryId;
                MgrImporter = new NullMgrImporter();
            }
            else
            {
                FindMegaDatabase();
                MgrImporter = new MgrImporter(_megaDatabase);

                var settings = new CachedSettings
                {
                    EnvironmentId = EnvironmentId,
                    RepositoryId = _repositoryId                    
                };
                File.WriteAllText(CACHED_CONFIG_FILE, JsonConvert.SerializeObject(settings));
            }

            Console.WriteLine($"Using UAS {TokenUri}");
            Console.WriteLine($"Using GraphQL {_apiUri}");

            Client = new HttpClient()
            {
                BaseAddress = _apiUri
            };
        }

        private void FindMegaDatabase()
        {
            var sqlConnectionStringFromEnv = Environment.GetEnvironmentVariable("HopexSqlConnectionString");
            if (!string.IsNullOrEmpty(sqlConnectionStringFromEnv))
                _sqlConnectionString = sqlConnectionStringFromEnv;
            Console.WriteLine("Using connection string " + _sqlConnectionString);

            var megaApplication = new MegaApplication();
            var environments = megaApplication.Environments().Cast<MegaEnvironment>();
            var environmentTestsLab = environments.Where(e => e.Path.Contains(_preferredEnvironmentPartialName));
            var environment = environmentTestsLab.FirstOrDefault() ?? environments.First();

            environment.CurrentAdministrator = _personSystem;
            environment.CurrentPassword = Password;

            EnvironmentId = environment.GetProp("EnvHexaIdAbs");
            Console.WriteLine($"Found environment {EnvironmentId} {environment.Path}");

            var databases = environment.Databases();
            _megaDatabase = databases.Cast<MegaDatabase>().FirstOrDefault(db => db.Name.Equals(_repositoryName, StringComparison.InvariantCultureIgnoreCase));
            if (_megaDatabase == null)
            {
                Console.WriteLine("Creating Database " + _repositoryName);
                _megaDatabase = databases.Create(_repositoryName, $@"{environment.Path}\Db\{_repositoryName}", _sqlConnectionString);
            }
            _repositoryId = _megaDatabase.GetProp("EnvHexaIdAbs");
            Console.WriteLine("Found Database " + _repositoryId);
        }

        private static string GetFullyQualifiedDomainName()
        {
            string domainName = "." + IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();
            if (hostName.EndsWith(domainName))
                return hostName;
            return hostName + domainName;
        }

        public async Task FillHeadersAsync(HttpRequestHeaders headers)
        {
            if (_uasToken.Expired())
                _uasToken = await UasToken.CreateAsync(this);
            headers.Authorization = AuthenticationHeaderValue.Parse($"Bearer {_uasToken.AccessToken}");
            headers.Add("X-Hopex-Context", ContextHeader);
        }

        public async Task<GraphQLHttpClient> GetGraphQLClientAsync(string schema)
        {
            if (! _graphQLClientDictionary.ContainsKey(schema))
            {
                var client = new GraphQLHttpClient(new GraphQLHttpClientOptions()
                {
                    EndPoint = new Uri(_apiUri, schema)
                });
                await FillHeadersAsync(client.HttpClient.DefaultRequestHeaders);
                _graphQLClientDictionary.Add(schema, client);
            }
            return _graphQLClientDictionary[schema];
        }

        private bool IsRunWithoutInitialization()
        {
            return Environment.GetEnvironmentVariable("SKIP_INIT") == "1" &&
                File.Exists(CACHED_CONFIG_FILE);
        }

        public class CachedSettings
        {            
            public string EnvironmentId { get; set; }
            public string RepositoryId { get; set; }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    Client.Dispose();
                }
                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.
                disposedValue = true;
            }
        }

        ~GlobalFixture()
        {
            Dispose(false);
        }
    
        public void Dispose()
        {
            Dispose(true);            
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    [CollectionDefinition("Global")]
    public class GlobalCollection : ICollectionFixture<GlobalFixture>
    {
    }
}
