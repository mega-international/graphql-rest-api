using GraphQL.Types;
using Hopex.Model.DataModel;

namespace Hopex.Modules.GraphQL.Schema.Formats
{
    public class EnumFormat : EnumerationGraphType<EnumFormatEnum>
    {
        public EnumFormat()
        {
            Name = "EnumFormat";
        }
    }
}
