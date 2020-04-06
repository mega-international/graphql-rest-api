using Mega.Macro.API;
using System;
using System.Text.RegularExpressions;

namespace Hopex.WebService.Tests.Mocks
{
    static class MegaIdUtils
    {
        internal static void EnsureValidPropertyId(MegaId propertyId, string methodName)
        {
            var isValidMegaField = IsMegaField(propertyId);
            if (!isValidMegaField)
                throw new Exception($"{methodName}:Name '{propertyId}' is not a valid argument");
        }

        internal static bool IsMegaField(MegaId propertyId)
        {
            return propertyId.Value is string
                && Regex.Match((string)propertyId.Value, @"~[\w\d\(\)]{12}(\[.*\])?").Success;
        }
    }
}
