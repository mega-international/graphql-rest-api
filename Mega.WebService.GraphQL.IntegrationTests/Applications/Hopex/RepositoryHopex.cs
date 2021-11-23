using Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces;
using MegaMapp;

namespace Mega.WebService.GraphQL.IntegrationTests.Applications.Hopex
{
    class RepositoryHopex : IRepository
    {
        private readonly ServerInfos _serverInfos;
        private readonly MegaDatabase _internalRepository;
        private readonly IEnvironment _environment;
        public RepositoryHopex(MegaDatabase repository, IEnvironment environment, ServerInfos serverInfos)
        {
            _internalRepository = repository;
            _environment = environment;
            _serverInfos = serverInfos;
        }
        public string Id => _internalRepository.GetProp("EnvHexaIdAbs");
        public SessionDatas CreateSessionDatas(UserInfos userInfos, string profileId)
        {
            return new SessionDatas
            {
                Login = userInfos.LoginName,
                Password = userInfos.Password,
                EnvironmentId = _environment.Id,
                RepositoryId = Id,
                ProfileId = profileId,
                Scheme = _serverInfos.Scheme,
                Host = _serverInfos.Host
            };
        }

        public int Import(string ImportFileName, string RejectFileName, string Options)
        {
            return _internalRepository.Import(ImportFileName, RejectFileName, Options);
        }
    }
}
