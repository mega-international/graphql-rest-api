using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.FieldModels
{
    public class ListField : Field
    {
        public readonly Field Field;

        public ListField(string name, Field field, bool nullable = true) : base(name, nullable)
        {
            Field = field;
        }

        public override string GetStringFormat(JToken token)
        {
            if(!(token is JArray jsonArr))
            {
                throw new ArgumentException("Token must be a JArray");
            }

            string value = "[\r\n";
            for(int idx = 0;idx < jsonArr.Count;++idx)
            {
                if(idx > 0)
                    value += ",\n";
                value += Field.GetStringFormat(jsonArr [idx]);
            }
            value += "\r\n]";
            return value;
        }

        protected override string GetOutputFormatInternal(bool showName, IReadOnlyDictionary<string, string> parameters)
        {
            var parametersStr = parameters == null ? "" : BuildParametersString(parameters);
            return $"{(showName ? (Name + parametersStr) : "")} {Field.GetOutputFormat(false)}";
        }

        public override string ToString()
        {
            return base.ToString() + $", contentType: {Field.GetType().Name}";
        }
    }
}
