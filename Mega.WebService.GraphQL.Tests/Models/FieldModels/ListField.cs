using Mega.WebService.GraphQL.Tests.Models.FakeDatas;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models.FieldModels
{
    public class ListField : Field
    {
        private Field field;
        public Field Field
        {
            get
            {
                return field;
            }
            set
            {
                field = value;
                field.Name = Name;
            }
        }
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

        public override JToken GetFakeValue(Container container, JObject jObj)
        {
            if(!jObj.TryGetValue(Name, out JToken value))
            {
                return new JArray();
            }
            List<string> ids = new List<string>(value.ToObject<List<string>>());
            string metaclassName = char.ToUpper(Name [0]) + Name.Substring(1);
            ObjectField objField = Field as ObjectField;
            return container.GetAll(metaclassName, objField.Fields, ids);
        }
    }
}
