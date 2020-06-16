using Mega.Bridge.Services;

namespace Mega.WebService.GraphQL.Utils
{
    public interface IHopexServiceFinder
    {
        string FindSession(string sspUrl, string securityKey, string environment, string dataLanguage, string guiLanguage, string profile, string personId, string accessMode, bool useHopexApiMwas = false);
        IHopexService GetMwasService(string mwasUrl, string hopexSessionToken);
        IHopexService GetService(string mwasUrl, string securityKey);
        
    }

    internal class RealHopexServiceFinder : IHopexServiceFinder
    {
        public string FindSession(string sspUrl, string securityKey, string environment, string dataLanguage, string guiLanguage, string profile, string personId, string accessMode, bool useHopexApiMwas = false)
        {
            return HopexService.FindSession(sspUrl, securityKey, environment, dataLanguage, guiLanguage, profile, personId, accessMode, useHopexApiMwas);
        }

        public IHopexService GetMwasService(string mwasUrl, string hopexSessionToken)
        {
            var nativeService = HopexServiceHelper.GetMwasService(mwasUrl, hopexSessionToken);
            return new RealHopexService(nativeService);
        }

        public IHopexService GetService(string mwasUrl, string securityKey)
        {
            return new RealHopexService(new HopexService(mwasUrl, securityKey));
        }
    }
}
