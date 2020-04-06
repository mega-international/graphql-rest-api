using Mega.WebService.GraphQL.Tests.Models.Interfaces.Crud;
using Mega.WebService.GraphQL.Tests.Sources.FieldModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mega.WebService.GraphQL.Tests.Models.Crud
{
    public class CrudInput : ICrudInput
    {
        private readonly string _name;
        public JToken Value { get; set; }
        public Field Field { get; set; }

        public CrudInput(string name, JToken value, Field field)
        {
            _name = name;
            Value = value;
            Field = field;
        }

        public string Serialize()
        {
            return $"{_name}: {Field.GetStringFormat(Value)}";
        }
    }
}
