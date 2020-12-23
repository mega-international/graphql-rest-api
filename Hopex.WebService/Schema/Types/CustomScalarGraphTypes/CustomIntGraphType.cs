using GraphQL.Types;

namespace Hopex.Modules.GraphQL.Schema.Types.CustomScalarGraphTypes
{
    class CustomIntGraphType : IntGraphType
    {
        public override object Serialize(object value)
        {
            return value is string ? value : base.Serialize(value);
        }
    }
}
