using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mega.WebService.GraphQL.Tests.Sources.FieldModels
{
    public class EnumField : Field
    {
        public EnumField(string name, bool nullable = true) : base(name, nullable)
        {}

        public override string GetStringFormat(JToken token)
        {
            if(token.Type == JTokenType.Null)
            {
                return null;
            }
            string value = token.ToString();
            Regex undesiredChars = new Regex("[^A-Za-z0-9_]");
            return undesiredChars.Replace(value, "_");
        }

        public override JToken GenerateValueFilter(List<JObject> items, out List<JObject> expected)
        {
            var fieldName = GetOriginalName();
            return _operator.GenerateValueFilter(items, fieldName, false, out expected);
        }
    }
}
