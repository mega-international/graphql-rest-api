using GraphQL.Types;

namespace Hopex.Modules.GraphQL.Schema.Types.CustomScalarGraphTypes
{
    class CustomDecimalGraphType : DecimalGraphType
    {
        public override object Serialize(object value)
        {
            return value is string ? value : base.Serialize(value);
        }
    }
}
