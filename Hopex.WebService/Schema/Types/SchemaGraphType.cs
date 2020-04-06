using GraphQL.Types;

namespace Hopex.Modules.GraphQL.Schema.Types
{
    public class SchemaGraphType : ObjectGraphType<SchemaType>
        {
            public SchemaGraphType()
            {
                Field(ctx => ctx.Name);
            }
        }
}
