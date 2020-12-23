using GraphQL.Types;

namespace Hopex.Modules.GraphQL.Schema
{
    public class HopexEnumerationGraphType : EnumerationGraphType
    {
        public override object Serialize(object value)
        {
            return value;
        }
    }
}
