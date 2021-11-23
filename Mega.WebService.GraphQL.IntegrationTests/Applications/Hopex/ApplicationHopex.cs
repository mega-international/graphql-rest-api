using Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces;
using MegaMapp;
using System.Linq;

namespace Mega.WebService.GraphQL.IntegrationTests.Applications.Hopex
{
    internal class ApplicationHopex : IApplication
    {
        private const string _environmentName = "EnvTestsLab";
        private readonly ServerInfos _serverInfos;
        private readonly MegaApplication _internalApplication;
        public ApplicationHopex(ServerInfos serverInfos)
        {
            _internalApplication = new MegaApplication();
            _serverInfos = serverInfos;
        }

        public IEnvironment GetEnvironment()
        {
            return GetEnvironmentByName(_environmentName);
        }

        public IHeaderBuilder InstanciateBuilder()
        {
            return new HeaderBuilderToken();
        }

        private IEnvironment GetEnvironmentByName(string environmentName)
        {
            var environments = _internalApplication.Environments().Cast<MegaEnvironment>();
            var environmentTestsLab = environments.Where(e => e.Path.Contains(environmentName));
            var environment = environmentTestsLab.FirstOrDefault() ?? environments.First();
            return new EnvironmentHopex(environment, _serverInfos);
        }
    }
}
