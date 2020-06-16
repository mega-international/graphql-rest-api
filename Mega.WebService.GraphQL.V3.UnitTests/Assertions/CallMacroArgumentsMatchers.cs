using Mega.WebService.GraphQL.Models;
using Moq;
using Newtonsoft.Json;
using System;

namespace Mega.WebService.GraphQL.V3.UnitTests.Assertions
{
    public static class CallMacroArgumentsMatchers<T>
    {
        public static string HasJsonArg(Predicate<CallMacroArguments<T>> predicate)
        {
            return Match.Create<string>(s =>
            {
                var args = JsonConvert.DeserializeObject<CallMacroArguments<T>>(s);
                return predicate(args);
            });
        }
    }
}
