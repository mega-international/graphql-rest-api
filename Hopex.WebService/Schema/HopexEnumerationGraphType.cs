using GraphQL.Types;

namespace Hopex.Modules.GraphQL.Schema
{
    internal class HopexEnumerationGraphType : EnumerationGraphType
    {
        public override object Serialize(object value)
        {
            return value;
        }

        public override object ParseValue(object value)
        {
            return base.ParseValue(value);
        }
    }
}
