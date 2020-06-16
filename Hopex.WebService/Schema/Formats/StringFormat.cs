using GraphQL.Types;
using Hopex.Model.DataModel;

namespace Hopex.Modules.GraphQL.Schema.Formats
{
    public class StringFormat : EnumerationGraphType<StringFormatEnum>
    {
        public StringFormat()
        {
            Name = "StringFormat";
        }
    }
}
