using System;
using System.Collections.Generic;
using Hopex.Model.Abstractions;

namespace Hopex.Modules.GraphQL.Schema
{
    class LanguagesEnumerationGraphType : HopexEnumerationGraphType
    {

        internal LanguagesEnumerationGraphType(Dictionary<string, IMegaObject> languages, Func<string, string> toValidName)
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
