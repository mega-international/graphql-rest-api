using Newtonsoft.Json.Linq;
using System;

namespace Mega.WebService.GraphQL.Tests.Sources.FieldModels
{
    public class ListField : Field
    {
        public readonly Field Field;

        public ListField(string name, Field field, bool nullable = false) : base(name, nullable)
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

        public override string GetOutputFormat()
        {
            return Field.GetOutputFormat();
        }

        public override string ToString()
        {
            return base.ToString() + $", contentType: {Field.GetType().Name}";
        }
    }
}
