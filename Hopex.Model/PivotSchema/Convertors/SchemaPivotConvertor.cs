using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Hopex.Model.PivotSchema.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hopex.Model.PivotSchema.Convertors
{
    public class PivotConvertor : IPivotSchemaConvertor
    {
        private readonly ValidationContext _validationContext;
        private Dictionary<string, PivotEntityHasProperties> _interfaces;

        public PivotConvertor(ValidationContext validationContext)
        {
            _validationContext = validationContext;
        }

        public async Task<ValidationContext> ValidateAsync(IHopexMetaModelManager schemaManager, Models.PivotSchema pivot, SchemaReference origin)
        {
            await ConvertAsync(schemaManager, pivot, origin);
            return _validationContext;
        }

        public async Task<IHopexMetaModel> ConvertAsync(IHopexMetaModelManager schemaManager, Models.PivotSchema pivot, SchemaReference origin)
        {
            IHopexMetaModel parent = null;
            if (pivot.OverrideSchema != null)
            {
                parent = await schemaManager.GetMetaModelAsync(new SchemaReference { SchemaName = pivot.OverrideSchema, Version = origin.Version, IgnoreCustom = true }, _validationContext);
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
                foreach (ClassDescription cls in parent.Interfaces)
                {
                    schema.AddInterface(cls.Clone(schema));
                }
            }
            _interfaces = new Dictionary<string, PivotEntityHasProperties>(StringComparer.InvariantCultureIgnoreCase);
            if (pivot.Interfaces != null) // = abstractClass
            {
                foreach (var clazz in pivot.Interfaces)
                {
                    _validationContext.ValidateClass(clazz);

                    if (!(schema.GetInterfaceDescription(clazz.Name, false) is ClassDescription existing))
                    {
                        existing = new ClassDescription(schema, clazz.Name, clazz.Id, clazz.Description, clazz.Constraints?.IsEntryPoint == true);
                        schema.AddInterface(existing);
                        _interfaces.Add(clazz.Name, clazz);
                    }
                    else
                    {
                        existing.Description = clazz.Description ?? existing.Description;
                    }
                    if (clazz.Properties != null)
                    {
                        ReadProperties(clazz.Properties, existing);
                    }

                    ReadRelationships(clazz, existing);
                }
            }

            // Then merge attributes
            foreach (var clazz in pivot.Classes)
            {
                _validationContext.ValidateClass(clazz);

                if (!(schema.GetClassDescription(clazz.Name, false) is ClassDescription existing))
                {
                    existing = new ClassDescription(schema, clazz.Name, clazz.Id, clazz.Description, clazz.Constraints?.IsEntryPoint == true);
                    schema.AddClass(existing);
                }
                else
                {
                    existing.Description = clazz.Description ?? existing.Description;
                }
                if (clazz.Properties != null)
                {
                    ReadProperties(clazz.Properties, existing);
                }

                if (clazz.Implements != null)
                {
                    if (_interfaces.TryGetValue(clazz.Implements, out var intf) && intf.Properties != null)
                    {
                        // TODO pour éviter les doublons avec les interfaces
                        // Normalement ce ne devrait pas être le cas soit la propriété est décrite
                        // dans l'interface, soit dans la classe
                        var properties = intf.Properties.Where(p =>
                                    existing.GetPropertyDescription(p.Name, false) == null);
                        ReadProperties(properties, existing);
                    }
                }
            }
            foreach (var clazz in pivot.Classes)
            {
                var existing = schema.GetClassDescription(clazz.Name) as ClassDescription;
                ReadRelationships(clazz, existing);
            }

            if (_validationContext.HasError)
            {
                throw new ValidationException(_validationContext);
            }
            return schema;
        }

        private void ReadRelationships(PivotClassDescription clazz, ClassDescription cd)
        {
            if (clazz.Relationships == null)
            {
                return;
            }

            // All classes targeting by a relationship inherits the default
            // relationship properties
            var saw = new HashSet<string>(); // Ensure to add default properties only once

            foreach (PivotRelationshipDescription rel in clazz.Relationships)
            {
                string roleId = rel.Path[0].RoleId;
                if (!(cd.GetRelationshipDescription(rel.Name, false) is RelationshipDescription existing))
                {
                    existing = new RelationshipDescription(rel.Id, rel.ReverseId, cd, rel.Name, roleId, rel.Description);
                    cd.AddRelationship(existing);
                }
                else
                {
                    existing.Description = rel.Description ?? existing.Description;
                }
                var targetClass = cd.MetaModel.FindClassDescriptionById(rel.Path.Last().MetaClassId);
                // TODO BUG dans json Implements est tjs null
                rel.Implements = "relationship"; // A virer qd le json est bon
                // END bug
                if (rel.Implements != null)
                {
                    if (_interfaces.TryGetValue(rel.Implements, out var intf)
                        && intf.Properties != null
                        && saw.Add(rel.Path.Last().MetaClassId))
                    {
                        ReadProperties(intf.Properties, targetClass, PropertyScope.Relationship);
                    }
                }
                existing.TargetClass = targetClass;
                existing.SetPath(rel.Path.Select(p =>
                {
                    var newPath = new PathDescription(p.Id, p.RoleName, p.RoleId, p.MetaClassId, p.MetaClassName, p.Multiplicity, p.Condition);
                    if (p.Properties != null)
                    {
                        var extendedClassName = rel.TargetClassName;
                        var extendedClass = new ClassDescription(targetClass.MetaModel,
                                                        extendedClassName,
                                                        targetClass.Id,
                                                        targetClass.Description,
                                                        false,
                                                        targetClass);
                        existing.TargetClass = extendedClass;

                        ReadProperties(p.Properties, extendedClass, PropertyScope.TargetClass);
                    }
                    return newPath;
                }));
            }
        }

        private static void ReadProperties(IEnumerable<PivotPropertyDescription> properties, IClassDescription cd, PropertyScope scope = PropertyScope.Class)
        {
            foreach (PivotPropertyDescription property in properties)
            {
                if (!(cd.GetPropertyDescription(property.Name, false) is PropertyDescription existing))
                {
                    existing = new PropertyDescription(cd,
                                                       property.Name,
                                                       property.Id,
                                                       property.Description,
                                                       property.Constraints?.PropertyType,
                                                       property.Constraints?.IsRequired,
                                                       property.Constraints?.IsReadOnly,
                                                       property.Constraints?.IsTranslatable,
                                                       property.Constraints?.IsFormattedText,
                                                       property.Constraints?.MaxLength,
                                                       scope)
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
                    existing.IsTranslatable = property.Constraints?.IsTranslatable ?? existing.IsTranslatable;
                    existing.IsFormattedText = property.Constraints?.IsFormattedText ?? existing.IsFormattedText;
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
