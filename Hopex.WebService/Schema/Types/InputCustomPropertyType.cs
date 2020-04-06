using GraphQL.Types;
using System.Collections.Generic;

namespace Hopex.Modules.GraphQL.Schema.Types
{
    public class InputCustomPropertyType : InputObjectGraphType
    {
        public InputCustomPropertyType()
        {
            Name = "CustomFieldInput";
            Field<NonNullGraphType<StringGraphType>>("id");
            Field<NonNullGraphType<StringGraphType>>("value");
        }

        internal static void AddCustomFields(IInputObjectGraphType typeToEnrich)
        {
            typeToEnrich.AddField(new FieldType
            {
                Type = typeof(ListGraphType<InputCustomPropertyType>),
                Name = "customFields",
            }) ;
        }

        internal static bool IsCustomFieldsArgument(KeyValuePair<string, object> kv)
        {
            return kv.Key.Equals("customFields");
        }
    }
}
