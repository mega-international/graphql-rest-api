using GraphQL.Types;

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
