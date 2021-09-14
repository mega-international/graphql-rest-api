using GraphQL.Types;
using System;
using GraphQL.Language.AST;

namespace Hopex.Modules.GraphQL.Schema.Types.CustomScalarGraphTypes
{
    class CustomDateGraphType : DateGraphType
    {
        public override object Serialize(object value)
        {
            return value is string ? value : base.Serialize(value);
        }

        public override object ParseValue(object value)
        {
            if(value is DateTime dateTime)
            {
                value = dateTime.Date;
            }
            return DateTime.Parse(value.ToString(), null, System.Globalization.DateTimeStyles.AdjustToUniversal);
        }

        public override bool CanParseLiteral(IValue value)
        {
            return true;
        }

        public override object ParseLiteral(IValue value)
        {
            return ParseValue(value.Value);
        }
    }
}
