using GraphQL.Types;

namespace Hopex.Modules.GraphQL.Schema.Types.CustomScalarGraphTypes
{
    class CustomDateGraphType : DateGraphType
    {
        public override object Serialize(object value)
        {
            return value is string ? value : base.Serialize(value);
        }
    }
}
