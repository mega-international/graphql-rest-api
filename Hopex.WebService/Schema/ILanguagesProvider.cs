using Mega.Macro.API;
using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema
{
    public interface ILanguagesProvider
    {
        Dictionary<string, string> GetLanguages(object nativeRoot);
    }

    class LanguagesProvider : ILanguagesProvider
    {
        public virtual Dictionary<string, string> GetLanguages(object nativeRoot)
        {
            var megaRoot = MegaWrapperObject.Cast<MegaRoot>(nativeRoot);
            var languages = new Dictionary<string, string>();
            if (megaRoot != null)
            {
                var languageNeutral = megaRoot.GetObjectFromId<MegaObject>("~I9o3by0knG00[Neutral]");
                var countryLanguages = languageNeutral.GetCollection("~41000000CW30[Specialized Language]");
                foreach (MegaObject language in countryLanguages)
                {
                    var languageName = language.GetPropertyValue("~HjL0v9mDp010[Language Code]");
                    languages.Add(languageName, language.MegaUnnamedField);
                }
            }
            return languages;
        }
    }
}
