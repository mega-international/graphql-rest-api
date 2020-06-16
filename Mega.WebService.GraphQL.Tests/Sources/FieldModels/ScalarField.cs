using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mega.WebService.GraphQL.Tests.Sources.FieldModels
{
    public class ScalarField : Field
    {
        public string TypeName { get; set; }
        public ScalarField(string name, string typeName, bool nullable = true) : base(name, nullable)
        {
            TypeName = typeName;
        }

        public override string GetStringFormat(JToken token)
        {
            if(token.Type == JTokenType.Null)
            {
                return null;
            }
            return JsonConvert.SerializeObject(token);
        }

        public override string ToString()
        {
            return base.ToString() + $", typename: {TypeName}";
        }
    }
}
