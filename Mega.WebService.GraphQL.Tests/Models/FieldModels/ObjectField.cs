using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models.FieldModels
{
    public class ObjectField : Field
    {
        public List<Field> Fields { get; set; }
        public ObjectField(string name, List<Field> fields, bool nullable = true) : base(name, nullable)
        {
            Fields = fields;
        }

        public override string GetStringFormat(JToken token)
        {
            if(!(token is JObject jsonObj))
            {
                throw new ArgumentException("Token must be a JObject");
            }

            string value = "{\r\n";
            foreach(JProperty prop in jsonObj.Properties())
            {
                Field field = Fields.Find(f => f.Name.Equals(prop.Name));
                if(field != null)
                {
                    string strVal = field.GetStringFormat(prop.Value);
                    if(strVal != null)
                    {
                        value += $"{prop.Name}: {field.GetStringFormat(prop.Value)} \r\n";
                    }
                }
            }
            value += "\r\n}";
            return value;
        }

        public override string GetOutputFormat()
        {
            string outputStr = $"{Name}\n{{";
            Fields.ForEach(field => outputStr += $"{field.GetOutputFormat()}\n");
            outputStr += "}";
            return outputStr;
        }
    }
}
