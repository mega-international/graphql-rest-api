using Hopex.Model.Abstractions;
using System;

namespace Hopex.Modules.GraphQL.Dataset
{
    internal abstract class PropertyFormatter
    {
        protected string idForGetProp;

        protected PropertyFormatter(string id)
        {
            idForGetProp = "~" + id;
        }

        internal static PropertyFormatter Create(string id, string metaAttributeFormat, string metaAttributeType)
        {
            var format = MetaAttributeFormatUtils.From(metaAttributeFormat);
            var type = MetaAttributeTypeUtils.From(metaAttributeType);
            if (format == MetaAttributeFormat.Currency)
                return new CurrencyPropertyFormatter(id);
            switch (type)
            {
                case MetaAttributeType.DateTime:
                case MetaAttributeType.DateTime64:
                case MetaAttributeType.AbsoluteDateTime64:
                    return new DatePropertyFormatter(id);
                case MetaAttributeType.Float:
                case MetaAttributeType.Short:
                case MetaAttributeType.Long:
                case MetaAttributeType.Currency:
                    return new DoublePropertyFormatter(id);
                default:
                    return new DisplayPropertyFormatter(id);
            }
        }

        internal abstract object Format(IMegaObject line);
    }

    internal class CurrencyPropertyFormatter : PropertyFormatter
    {
        internal CurrencyPropertyFormatter(string id) : base(id) { }

        internal override object Format(IMegaObject line)
        {
            return line.GetPropertyValue<decimal>(idForGetProp);
        }
    }

    internal class DatePropertyFormatter : PropertyFormatter
    {
        internal DatePropertyFormatter(string id) : base(id) { }

        internal override object Format(IMegaObject line)
        {
            var emptyDate = string.IsNullOrEmpty(line.GetPropertyValue(idForGetProp, "ASCII"));
            if (emptyDate)
                return null;
            else
            {
                var value = line.GetPropertyValue<DateTime>(idForGetProp);
                return value.ToString("yyyy-MM-dd");
            };
        }
    }

    internal class DoublePropertyFormatter : PropertyFormatter
    {
        internal DoublePropertyFormatter(string id) : base(id) { }

        internal override object Format(IMegaObject line)
        {
            var emptyNumber = string.IsNullOrEmpty(line.GetPropertyValue(idForGetProp, "ASCII"));
            if (emptyNumber)
                return null;
            else
                return line.GetPropertyValue<double>(idForGetProp);
        }
    }

    internal class DisplayPropertyFormatter : PropertyFormatter
    {
        internal DisplayPropertyFormatter(string id) : base(id) { }

        internal override object Format(IMegaObject line)
        {
            var value = line.GetPropertyValue(idForGetProp, "Display");
            var emptyValue = string.IsNullOrEmpty(value);
            if (emptyValue)
                return null;
            else
                return value; ;
        }
    }
}
