using GraphQL;
using Hopex.Model.Abstractions.MetaModel;

namespace Hopex.Modules.GraphQL.Schema.Types
{
    public class SchemaType
    {
        public string Name { get; private set; }
        public string GraphQLNameInSchema { get; private set; }

        public SchemaType(IHopexMetaModel schema, string graphQLNameInSchema)
        {
            Name = schema.Name;
            GraphQLNameInSchema = graphQLNameInSchema.ToCamelCase();
        }
    }
}
