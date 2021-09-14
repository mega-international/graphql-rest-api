using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL.Schema.Types
{
    public class InputCustomRelationshipType : InputObjectGraphType
    {
        public InputCustomRelationshipType()
        {
            Name = "_InputCustomRelationship";

            Field<NonNullGraphType<CollectionActionGraphType>>("action");
            Field<NonNullGraphType<StringGraphType>>("relationId");
            Field<NonNullGraphType<ListGraphType<InputCustomRelationChildType>>>("list");
        }

        internal static void AddCustomRelations(IInputObjectGraphType typeToEnrich)
        {
            typeToEnrich.AddField(new FieldType
            {
                Type = typeof(ListGraphType<InputCustomRelationshipType>),
                Name = "customRelationships"
            });            
        }

        internal static bool IsCustomRelationsArgument(KeyValuePair<string, object> kv)
        {
            return kv.Key.Equals("customRelationships");
        }
    }

    public class InputCustomRelationChildType : InputObjectGraphType
    {
        public InputCustomRelationChildType()
        {
            Name = "_InputCustomRelationChild";
            Field<NonNullGraphType<StringGraphType>>("id");
            InputCustomPropertyType.AddCustomFields(this);
        }
    }
}
