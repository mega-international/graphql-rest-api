using GraphQL.Types;
using Hopex.Model.DataModel;

namespace Hopex.Modules.GraphQL.Schema.Formats
{
    public class NameSpaceFormat : EnumerationGraphType<NameSpaceFormatEnum>
    {
        public NameSpaceFormat()
        {
            Name = "nameSpace";
        }
    }
}
