using Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces;
using MegaMapp;
using System;
using System.Linq;

namespace Mega.WebService.GraphQL.IntegrationTests.Applications.Hopex
{
    class EnvironmentHopex : IEnvironment
    {
        private readonly ServerInfos _serverInfos;
        private readonly MegaEnvironment _internalEnvironment;
        public EnvironmentHopex(MegaEnvironment environment, ServerInfos serverInfos)
        {
            _internalEnvironment = environment;
            _serverInfos = serverInfos;
        }
        public string Id => _internalEnvironment.GetProp("EnvHexaIdAbs");
        public string Path => _internalEnvironment.Path;
        public IRepository GetRepositoryByName(string repositoryName)
        {
            var databases = _internalEnvironment.Databases();
            var megaDatabase = databases.Cast<MegaDatabase>().FirstOrDefault(db => db.Name.Equals(repositoryName, StringComparison.InvariantCultureIgnoreCase));
            return new RepositoryHopex(megaDatabase, this, _serverInfos);
        }

        public void SetCurrentAdmin(string adminName, string password)
        {
            _internalEnvironment.CurrentAdministrator = adminName;
            _internalEnvironment.CurrentPassword = password;
        }

        public IRepository CreateRepository(string name, string options)
        {
            var databases = _internalEnvironment.Databases();
            var megaDatabase = databases.Create(name, $@"{Path}\Db\{name}", options);
            return new RepositoryHopex(megaDatabase, this, _serverInfos);
        }
    }
}
