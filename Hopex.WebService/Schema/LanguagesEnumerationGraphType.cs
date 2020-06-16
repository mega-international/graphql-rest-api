using System;
using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema
{
    class LanguagesEnumerationGraphType : HopexEnumerationGraphType
    {

        internal LanguagesEnumerationGraphType(Dictionary<string, string> languages, Func<string, string> toValidName)
        {
            Name = "Languages";
            if (languages != null)
            {
                foreach (var language in languages)
                {
                    AddValue(toValidName(language.Key), "", language.Value);
                }
            }
        }
    }
}
