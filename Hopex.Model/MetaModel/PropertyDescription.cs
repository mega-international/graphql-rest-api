using System;
using System.Collections.Generic;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API;

namespace Hopex.Model.MetaModel
{
    [System.Diagnostics.DebuggerDisplay("{Name}")]
    public class PropertyDescription : IPropertyDescription
    {
        public const string DefaultSetterFormat = "Internal";
        public const string DefaultGetterFormat = "Internal";

        private List<IEnumDescription> _enumValues;
        private string _propertyTypeName;

        public PropertyDescription(IClassDescription classDescription, string name, MegaId id, string description, string propertyType, bool? isRequired, bool? isReadOnly, bool? isFilter)
        {
            ClassDescription = classDescription;
            Name = name;
            Id = Utils.NormalizeHopexId(id);
            Description = description;
            PropertyTypeName = propertyType;
            IsRequired = isRequired == true;
            IsReadOnly = isReadOnly == true;
            IsFilterable = isFilter == true;
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

        public string Name { get; }
        public MegaId Id { get; }
        public string Description { get; internal set; }
        public IEnumerable<IConstraintDescription> Constraints { get; }
        public IEnumerable<IEnumDescription> EnumValues => _enumValues;
        public bool IsReadOnly { get; internal set; }
        public bool IsFilterable { get; internal set; }
        public bool IsRequired { get; internal set; }
        public IClassDescription ClassDescription { get; }
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
                    case "int":
                        NativeType = typeof(int);
                        PropertyType = PropertyType.Int;
                        break;
                    case "boolean":
                        NativeType = typeof(bool);
                        PropertyType = PropertyType.Boolean;
                        break;
                    case "id":
                    case "string":
                        NativeType = typeof(string);
                        PropertyType = PropertyType.String;
                        break;
                    case "long":
                        NativeType = typeof(long);
                        PropertyType = PropertyType.Long;
                        break;
                    case "float":
                        NativeType = typeof(float);
                        PropertyType = PropertyType.Double;
                        break;
                    case "date":
                        NativeType = typeof(DateTime);
                        PropertyType = PropertyType.Date;
                        break;
                    case "double":
                        NativeType = typeof(Double);
                        PropertyType = PropertyType.Double;
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
