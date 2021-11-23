using GraphQL.Client.Http;
using Mega.WebService.GraphQL.IntegrationTests.Applications;
using Mega.WebService.GraphQL.IntegrationTests.Applications.HAS;
using Mega.WebService.GraphQL.IntegrationTests.Applications.Hopex;
using Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Xunit;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    internal enum RequesterType
    {
        Token = 0,
        ApiKey = 1
    }

    public class GlobalFixture : IDisposable
    {
        private string _sqlConnectionString = $"User={ConfigurationManager.AppSettings["DBUser"]},Type={ConfigurationManager.AppSettings["DBType"]},SERVER={ConfigurationManager.AppSettings["DBServer"]};UID={ConfigurationManager.AppSettings["DBUser"]};PWD={ConfigurationManager.AppSettings["DBPassword"]}";

        private readonly RequesterType _requesterType = (RequesterType)Enum.Parse(typeof(RequesterType), ConfigurationManager.AppSettings["RequesterType"]);
        private readonly string _scheme = "https";
        private readonly string _host = ConfigurationManager.AppSettings["Host"];
        private readonly string _apiKeyAdmin = ConfigurationManager.AppSettings["ApiKeyAdmin"];
        private const string _apiPath = "HOPEXGraphQL/api/";
        private const string CACHED_CONFIG_FILE = "cachedConfig.json";

        private Uri ApiUri => new Uri($"{_scheme}://{_host}/{_apiPath}");
        private string LoginId => "8fCpjryUN96H";
        private string LoginName => "scr";
        private string PersonId => "XgCp3syUNLAH";
        private string PersonName => "CRONIER Sébastien";
        private string Password => "Hopex";
        private string RepositoryName => "GraphQLIntegrationTests";
        public string ProfileId { get; private set; } = "757wuc(SGjpJ"; // Hopex Customizer
        public string RepositoryId { get; private set; }
        public string EnvironmentId { get; private set; }
        public string ApiKey { get; private set; }

        public HttpClient Client { get; private set; }
        
        private readonly Dictionary<string, GraphQLHttpClient> _graphQLClientDictionary = new Dictionary<string, GraphQLHttpClient>();

        private IApplication _application;
        private SessionDatas _sessionDatas;
        private readonly ServerInfos _serverInfos;
        private readonly UserInfos _userInfos;

        public GlobalFixture()
        {
            _userInfos = new UserInfos
            {
                LoginId = LoginId,
                LoginName = LoginName,
                PersonId = PersonId,
                PersonName = PersonName,
                Password = Password
            };
            var useHttps = Environment.GetEnvironmentVariable("HopexUseHttps");
            if (!string.IsNullOrEmpty(useHttps))
            {
                _scheme = "https";
                _host = GetFullyQualifiedDomainName();
            }
            _serverInfos = new ServerInfos(_scheme, _host, _apiKeyAdmin);

            CreateApplication();

            if (IsRunWithoutInitialization())
            {
                var settings = JsonConvert.DeserializeObject<CachedSettings>(File.ReadAllText(CACHED_CONFIG_FILE));
                EnvironmentId = settings.EnvironmentId;
                RepositoryId = settings.RepositoryId;
                ApiKey = settings.ApiKey;
            }
            else
            {
                var megaDatabase = FindMegaDatabase();
                new MgrImporter(megaDatabase).ImportAll();

                var settings = new CachedSettings
                {
                    EnvironmentId = EnvironmentId,
                    RepositoryId = RepositoryId,
                    ApiKey = ApiKey
                };
                File.WriteAllText(CACHED_CONFIG_FILE, JsonConvert.SerializeObject(settings));
            }

            Console.WriteLine($"Using GraphQL {ApiUri}");

            Client = new HttpClient()
            {
                BaseAddress = ApiUri
            };
        }

        private void CreateApplication()
        {
            switch(_requesterType)
            {
                case RequesterType.ApiKey:
                {
                    _application = new ApplicationHAS(_serverInfos);
                    break;
                }
                case RequesterType.Token:
                {
                    _application = new ApplicationHopex(_serverInfos);
                    break;
                }
                default:
                {
                   throw new NotSupportedException($"Cannot instanciate application with request type: {_requesterType}");
                }
            }
        }

        private IRepository FindMegaDatabase()
        {
            var sqlConnectionStringFromEnv = Environment.GetEnvironmentVariable("HopexSqlConnectionString");
            if (!string.IsNullOrEmpty(sqlConnectionStringFromEnv))
                _sqlConnectionString = sqlConnectionStringFromEnv;
            Console.WriteLine("Using connection string " + _sqlConnectionString);

            var environment = _application.GetEnvironment();
            environment.SetCurrentAdmin(PersonName, Password);

            EnvironmentId = environment.Id;
            Console.WriteLine($"Found environment {EnvironmentId} {environment.Path}");

            var repository = environment.GetRepositoryByName(RepositoryName);
            if(repository == null)
            {
                Console.WriteLine("Creating Database " + RepositoryName);
                repository = environment.CreateRepository(RepositoryName, _sqlConnectionString);
            }

            RepositoryId = repository.Id;
            Console.WriteLine("Found Database " + RepositoryId);

            _sessionDatas = repository.CreateSessionDatas(_userInfos, ProfileId);
            ApiKey = _sessionDatas.ApiKey;
            return repository;
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
            IHeaderBuilder builder = _application.InstanciateBuilder();
            await builder.FillHeadersAsync(_sessionDatas, headers);
        }

        public async Task<GraphQLHttpClient> GetGraphQLClientAsync(string schema)
        {
            if (! _graphQLClientDictionary.ContainsKey(schema))
            {
                var client = new GraphQLHttpClient(new GraphQLHttpClientOptions()
                {
                    EndPoint = new Uri(ApiUri, schema)
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
            public string ApiKey { get; set; }
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
