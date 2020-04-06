using System.Collections.Specialized;
using System.Configuration;

namespace Mega.WebService.GraphQL.Utils
{
    public interface IConfigurationManager
    {
        NameValueCollection AppSettings { get; }
        object GetSection(string sectionName);
    }

    class RealConfigurationManager : IConfigurationManager
    {
        public NameValueCollection AppSettings => ConfigurationManager.AppSettings;

        public object GetSection(string sectionName)
        {
            return ConfigurationManager.GetSection(sectionName);
        }
    }
}
