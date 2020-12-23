using System;
using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema
{
    class CurrenciesEnumerationGraphType : HopexEnumerationGraphType
    {
        internal CurrenciesEnumerationGraphType(List<string> currencies, Func<string, string> toValidName)
        {
            Name = "Currencies";
            if (currencies != null)
            {
                foreach (var currency in currencies)
                {
                    AddValue(toValidName(currency), "", currency);
                }
            }
        }
    }
}
