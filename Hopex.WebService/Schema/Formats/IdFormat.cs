using GraphQL.Types;
using Hopex.Model.DataModel;

namespace Hopex.Modules.GraphQL.Schema.Formats
{
    public class IdFormat : EnumerationGraphType<IdFormatEnum>
    {
        public IdFormat()
        {
            Name = "format";
        }
    }
}
