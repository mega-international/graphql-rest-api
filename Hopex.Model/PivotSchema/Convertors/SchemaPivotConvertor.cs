using System.Linq;
using System.Threading.Tasks;
using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Hopex.Model.PivotSchema.Models;

namespace Hopex.Model.PivotSchema.Convertors
{
    public class PivotConvertor : IPivotSchemaConvertor
    {
        private readonly ValidationContext _validationContext;

        public PivotConvertor(ValidationContext validationContext)
        {
            _validationContext = validationContext;
        }

        public async Task<ValidationContext> ValidateAsync(IHopexMetaModelManager schemaManager, Models.PivotSchema pivot)
        {
            await ConvertAsync(schemaManager, pivot, false);
            return _validationContext;
        }

        public Task<IHopexMetaModel> ConvertAsync(IHopexMetaModelManager schemaManager, Models.PivotSchema pivot)
        {
            return ConvertAsync(schemaManager, pivot, true);
        }

        private async Task<IHopexMetaModel> ConvertAsync(IHopexMetaModelManager schemaManager, Models.PivotSchema pivot, bool throwException)
        {
            IHopexMetaModel parent = null;
            if (pivot.OverrideSchema != null)
            {
                parent = await schemaManager.GetMetaModelAsync(pivot.OverrideSchema, _validationContext);
                if (parent == null)
                {
                    _validationContext.AddValidationError($"{pivot.OverrideSchema} is not a valid schema to override");
                }
            }

            var schema = new HopexMetaModel(
                parent,
                pivot.Name ?? parent?.Name);

            if (parent != null)
            {
                // Include parent classes
                foreach (ClassDescription cls in parent.Classes)
                {
                    schema.AddClass(cls.Clone(schema));
                }
            }

            // Then merge attributes
            foreach (PivotClassDescription clazz in pivot.Classes)
            {
                _validationContext.ValidateClass(clazz);

                if (!(schema.GetClassDescription(clazz.Name, false) is ClassDescription existing))
                {
                    existing = new ClassDescription(schema, clazz.Name, clazz.Id, clazz.Description, clazz.IsEntryPoint == true);
                    schema.AddClass(existing);
                }
                else
                {
                    existing.Description = clazz.Description ?? existing.Description;
                }

                ReadProperties(clazz, existing);
                ReadRelationships(clazz, existing);
            }
            if (_validationContext.HasError)
            {
                throw new ValidationException(_validationContext);
            }
            return schema;
        }

        private static void ReadRelationships(PivotClassDescription clazz, ClassDescription cd)
        {
            if (clazz.Relationships == null)
            {
                return;
            }

            foreach (PivotRelationshipDescription rel in clazz.Relationships)
            {
                string roleId = rel.Path[0].RoleId;
                if (!(cd.GetRelationshipDescription(rel.Name, false) is RelationshipDescription existing))
                {
                    existing = new RelationshipDescription(cd, rel.Name, roleId, rel.Description);
                    cd.AddRelationship(existing);
                }
                else
                {
                    existing.Description = rel.Description ?? existing.Description;
                }

                if (rel.Path != null)
                {
                    existing.SetPath(rel.Path.Select(p => new PathDescription(p.RoleName, p.RoleId, p.MetaClassId, p.MetaClassName)));
                }
            }
        }

        private static void ReadProperties(PivotClassDescription clazz, ClassDescription cd)
        {
            if (clazz.Properties == null)
            {
                return;
            }

            foreach (PivotPropertyDescription property in clazz.Properties)
            {
                if (!(cd.GetPropertyDescription(property.Name, false) is PropertyDescription existing))
                {
                    existing = new PropertyDescription(cd, property.Name,
                                                       property.Id,
                                                       property.Description,
                                                       property.Constraints?.PropertyType,
                                                       property.Constraints?.IsRequired,
                                                       property.Constraints?.IsReadOnly,
                                                       property.Constraints?.IsFilter)
                    {
                        SetterFormat = property.SetterFormat ?? PropertyDescription.DefaultSetterFormat,
                        GetterFormat = property.GetterFormat ?? PropertyDescription.DefaultGetterFormat
                    };

                    cd.AddProperty(existing);
                }
                else
                {
                    existing.Description = property.Description ?? existing.Description;
                    existing.SetterFormat = property.SetterFormat ?? existing.SetterFormat ?? PropertyDescription.DefaultSetterFormat;
                    existing.GetterFormat = property.GetterFormat ?? existing.GetterFormat ?? PropertyDescription.DefaultGetterFormat;
                    existing.IsRequired = property.Constraints?.IsRequired ?? existing.IsRequired;
                    existing.IsReadOnly = property.Constraints?.IsReadOnly ?? existing.IsReadOnly;
                    existing.IsFilterable = property.Constraints?.IsFilter ?? existing.IsFilterable;
                }

                if (property.EnumValues != null)
                {
                    foreach (PivotEnumDescription e in property.EnumValues)
                    {
                        EnumDescription @enum = new EnumDescription(e.Name, e.Id, e.Description, e.InternalValue);
                        existing.AddEnumValue(@enum);
                    }
                }
            }
        }
    }
}
