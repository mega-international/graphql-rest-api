using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;

namespace Hopex.Modules.GraphQL.Schema.Filters
{
    public abstract class FilterValue
    {
        public IPropertyDescription PropertyDescription { get; set; }
        public string Operation { get; set; }
        public object Value { get; set; }

        public abstract bool Compare(object valueToCompare);

        internal static FilterValue Create(IPropertyDescription propertyDescription, string operation, object value)
        {
            if(Equals(propertyDescription.Id, MegaId.Create("~310000000D00")))
            {
                return new IdFilterValue
                {
                    PropertyDescription = propertyDescription,
                    Operation = operation,
                    Value = value
                };
            }
            switch(propertyDescription.PropertyType)
            {
                case PropertyType.Date:
                    return new DateTimeFilterValue
                    {
                        PropertyDescription = propertyDescription,
                        Operation = operation,
                        Value = value
                    };
                case PropertyType.Int:
                    return new IntFilterValue
                    {
                        PropertyDescription = propertyDescription,
                        Operation = operation,
                        Value = value
                    };
                case PropertyType.Long:
                    return new LongFilterValue
                    {
                        PropertyDescription = propertyDescription,
                        Operation = operation,
                        Value = value
                    };
                case PropertyType.Double:
                    return new DoubleFilterValue
                    {
                        PropertyDescription = propertyDescription,
                        Operation = operation,
                        Value = value
                    };
                case PropertyType.Enum:
                    return new EnumFilterValue
                    {
                        PropertyDescription = propertyDescription,
                        Operation = operation,
                        Value = value
                    };
                case PropertyType.Boolean:
                    return new BooleanFilterValue
                    {
                        PropertyDescription = propertyDescription,
                        Operation = operation,
                        Value = value
                    };
                case PropertyType.String:
                case PropertyType.RichText:
                    return new StringFilterValue
                    {
                        PropertyDescription = propertyDescription,
                        Operation = operation,
                        Value = value
                    };
                default:
                    return new StringFilterValue
                    {
                        PropertyDescription = propertyDescription,
                        Operation = operation,
                        Value = value
                    };
            }
        }

        public static bool IsValidOperator(string operation)
        {
            return operation == "" ||
                   operation == "not" ||
                   operation == "in" ||
                   operation == "not_in" ||
                   operation == "lt" ||
                   operation == "lte" ||
                   operation == "gt" ||
                   operation == "gte" ||
                   operation == "contains" ||
                   operation == "not_contains" ||
                   operation == "starts_with" ||
                   operation == "not_starts_with" ||
                   operation == "ends_with" ||
                   operation == "not_ends_with";
        }
    }
}
