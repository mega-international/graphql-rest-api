using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using System;
using System.Collections.Generic;

namespace Hopex.Model.MetaModel
{
    [System.Diagnostics.DebuggerDisplay("{Name}")]
    public class PropertyDescription : IPropertyDescription
    {
        public const string DefaultSetterFormat = "Internal";
        public const string DefaultGetterFormat = "Internal";

        private List<IEnumDescription> _enumValues;
        private string _propertyTypeName;

        public PropertyDescription(string name, string id, string description, string propertyType, bool? isRequired, bool? isReadOnly, bool? isUnique, bool? isTranslatable = false, bool? isFormattedText = false, int? maxLength = null, PropertyScope scope=PropertyScope.Class)
        {
            Name = name;
            Scope = scope;
            Id = Utils.NormalizeHopexId(id);
            Description = description;
            PropertyTypeName = propertyType;
            IsRequired = isRequired == true;
            IsReadOnly = isReadOnly == true;
            IsUnique = isUnique == true;
            IsTranslatable = isTranslatable == true;
            IsFormattedText = isFormattedText == true;
            MaxLength = maxLength;
            Constraints = new List<IConstraintDescription>();
        }

        internal void AddEnumValue(IEnumDescription @enum)
        {
            if (EnumValues == null)
            {
                _enumValues = new List<IEnumDescription>();
            }
            _enumValues.Add(@enum);
            PropertyType = PropertyType.Enum;
        }

        public virtual IEnumerable<ISetter> CreateSetters(object value)
        {
            yield return PropertySetter.Create(this, value);
        }

        public string Name { get; }
        public PropertyScope Scope { get; }
        public string Id { get; }
        public string Description { get; internal set; }
        public IEnumerable<IConstraintDescription> Constraints { get; }
        public IEnumerable<IEnumDescription> EnumValues => _enumValues;
        public bool IsReadOnly { get; internal set; }
        public bool IsUnique { get; internal set; }
        public bool IsRequired { get; internal set; }
        public bool IsTranslatable { get; internal set; }
        public bool IsFormattedText { get; internal set; }
        public int? MaxLength { get; internal set; }
        public string SetterFormat { get; internal set; }
        public string GetterFormat { get; internal set; }

        internal string PropertyTypeName
        {
            get => _propertyTypeName;
            set
            {
                _propertyTypeName = value;
                switch (_propertyTypeName.ToLower())
                {
                    case "id":
                        NativeType = typeof(string);
                        PropertyType = PropertyType.Id;
                        break;
                    case "int":
                        NativeType = typeof(int);
                        PropertyType = PropertyType.Int;
                        break;
                    case "long":
                        NativeType = typeof(long);
                        PropertyType = PropertyType.Long;
                        break;
                    case "float":
                        NativeType = typeof(float);
                        PropertyType = PropertyType.Double;
                        break;
                    case "double":
                        NativeType = typeof(double);
                        PropertyType = PropertyType.Double;
                        break;
                    case "string":
                        NativeType = typeof(string);
                        PropertyType = PropertyType.String;
                        break;
                    case "boolean":
                        NativeType = typeof(bool);
                        PropertyType = PropertyType.Boolean;
                        break;
                    case "date":
                        NativeType = typeof(DateTime);
                        PropertyType = PropertyType.Date;
                        break;
                    case "currency":
                        NativeType = typeof(double);
                        PropertyType = PropertyType.Currency;
                        break;
                    case "binary":
                        NativeType = typeof(byte[]);
                        PropertyType = PropertyType.Binary;
                        break;
                    default:
                        throw new Exception("Unknow type " + _propertyTypeName);
                }
            }
        }

        public PropertyType PropertyType { get; private set; }
        public Type NativeType { get; private set; }
    }
}
