using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Hopex.ApplicationServer.WebServices;
using Hopex.Model.Abstractions;

namespace Hopex.Modules.GraphQL.Schema
{
    public interface ILanguagesProvider
    {
        Dictionary<string, IMegaObject> GetLanguages(ILogger logger, IMegaRoot root);
        List<string> GetCurrencies(ILogger logger, IMegaRoot root);
    }

    class LanguagesProvider : ILanguagesProvider
    {
        public virtual Dictionary<string, IMegaObject> GetLanguages(ILogger logger, IMegaRoot root)
        {
            ObjectCache cache = MemoryCache.Default;
            if (cache["languages"] is Dictionary<string, IMegaObject> cachedLanguages && cachedLanguages.Any())
            {
                return cachedLanguages;
            }
            var languages = new Dictionary<string, IMegaObject>();
            var languageNeutral = root.GetObjectFromId("~I9o3by0knG00[Neutral]");
            var countryLanguages = languageNeutral.GetCollection("~41000000CW30[Specialized Language]");
            foreach (var language in countryLanguages)
            {
                var languageName = language.GetPropertyValue("~HjL0v9mDp010[Language Code]");
                languages.Add(languageName, language);
            }
            cache.Add("languages", languages, new CacheItemPolicy());
            return languages;
        }

        public virtual List<string> GetCurrencies(ILogger logger, IMegaRoot root)
        {
            ObjectCache cache = MemoryCache.Default;
            if (cache["currencies"] is List<string> cachedLanguages && cachedLanguages.Any())
            {
                return cachedLanguages;
            }
            var currencies = new List<string>();
            var allCurrencies = root.GetCollection("~430000000I40[Currency]", 1, "~a30000000HA0[Currency Code]");
            foreach (var currency in allCurrencies)
            {
                if (currency.IsAvailable)
                {
                    var currencyCode = currency.GetPropertyValue("~a30000000HA0[Currency Code]");
                    currencies.Add(currencyCode);
                }
            }
            cache.Add("currencies", currencies, new CacheItemPolicy());
            return currencies;
        }
    }
}
