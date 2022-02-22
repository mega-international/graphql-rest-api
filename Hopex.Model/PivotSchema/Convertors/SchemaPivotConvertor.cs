using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.MetaModel;
using Hopex.Model.PivotSchema.Models;
using Mega.Macro.API.Library;
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
        private const string _interfaceLinkAttributesName = "relationship";

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
            if(pivot.OverrideSchema != null)
            {
                parent = await schemaManager.GetMetaModelAsync(new SchemaReference { SchemaName = pivot.OverrideSchema, Version = origin.Version, IgnoreCustom = true }, _validationContext);
                if(parent == null)
                {
                    _validationContext.AddValidationError($"{pivot.OverrideSchema} is not a valid schema to override");
                }
            }

            var schema = new HopexMetaModel(
                parent,
                pivot.Name ?? parent?.Name);

            if(parent != null)
            {
                // Include parent classes
                foreach(ClassDescription cls in parent.Classes)
                {
                    schema.AddClass(cls.Clone(schema));
                }
                foreach(ClassDescription cls in parent.Interfaces)
                {
                    schema.AddInterface(cls.Clone(schema));
                }
            }
            _interfaces = new Dictionary<string, PivotEntityHasProperties>(StringComparer.InvariantCultureIgnoreCase);

            //Chargement des interfaces
            if(pivot.Interfaces != null) // = abstractClass
            {
                foreach(var clazz in pivot.Interfaces)
                {
                    _validationContext.ValidateClass(clazz);

                    if(!(schema.GetInterfaceDescription(clazz.Name, false) is ClassDescription existing))
                    {
                        existing = new ClassDescription(schema, clazz.Name, clazz.Id, clazz.Description, clazz.Constraints?.IsEntryPoint == true);
                        schema.AddInterface(existing);
                        _interfaces.Add(clazz.Name, clazz);
                    }
                    else
                    {
                        existing.Description = clazz.Description ?? existing.Description;
                    }
                    if(clazz.Properties != null)
                    {
                        ReadProperties(clazz.Properties, existing);
                    }

                    ReadRelationships(clazz.Relationships, existing, schema);
                }
            }

            // Chargement de la classe générique
            CreateGenericClass(schema);

            // Chargement des classes
            foreach(var clazz in pivot.Classes)
            {
                _validationContext.ValidateClass(clazz);

                //Création de la classe, si elle existe on écrase quelques attributs
                if(!(schema.GetClassDescription(clazz.Name, false) is ClassDescription existing))
                {
                    existing = new ClassDescription(schema, clazz.Name, clazz.Id, clazz.Description, clazz.Constraints?.IsEntryPoint == true);
                    schema.AddClass(existing);
                }
                else
                {
                    existing.Description = clazz.Description ?? existing.Description;
                }

                //Chargement des properties basiques
                ReadProperties(clazz.Properties, existing);

                //On ajoute les propriétés de l'interface mère pour faire l'héritage
                if(clazz.Implements != null)
                {
                    if(_interfaces.TryGetValue(clazz.Implements, out var intf) && intf.Properties != null)
                    {
                        // TODO pour éviter les doublons avec les interfaces
                        // Normalement ce ne devrait pas être le cas soit la propriété est décrite
                        // dans l'interface, soit dans la classe
                        var properties = intf.Properties.Where(p =>
                                    existing.GetPropertyDescription(p.Name, false) == null).ToList();
                        ReadProperties(properties, existing);
                    }
                }
            }

            //Chargement des relationships
            foreach(var clazz in pivot.Classes)
            {
                var existing = schema.GetClassDescription(clazz.Name) as ClassDescription;
                ReadRelationships(clazz.Relationships, existing, schema);
            }

            if(_validationContext.HasError)
            {
                throw new ValidationException(_validationContext);
            }
            return schema;
        }

        private void CreateGenericClass(HopexMetaModel schema)
        {
            var genericClass = new GenericClassDescription(schema);
            schema.AddClass(genericClass);

            var genericObjectProperties = schema.Interfaces
                .Where(x => x.Id == MetaClassLibrary.GenericObject.Substring(0, 13))
                .SelectMany(x => x.Properties);
            var genericObjectSystemProperties = schema.Interfaces
                .Where(x => x.Id == MetaClassLibrary.GenericObjectSystem.Substring(0, 13))
                .SelectMany(x => x.Properties).ToList();
            var properties = genericObjectProperties.Intersect(genericObjectSystemProperties, new PropertyDescriptionComparer());

            foreach(var property in properties)
            {
                genericClass.AddProperty(property);
            }
        }

        private void ReadRelationships(PivotRelationshipDescription[] relationships, ClassDescription cd, IHopexMetaModel metaModel)
        {
            if (relationships == null)
            {
                return;
            }
            var interfaceLinkAttributes = _interfaces[_interfaceLinkAttributesName];
            foreach (PivotRelationshipDescription rel in relationships)
            {
                //Si la relation n'a pas été traitée, on la créé et l'ajoute
                if(!(cd.GetRelationshipDescription(rel.Name, false) is RelationshipDescription existing))
                {
                    string roleId = rel.Path [0].RoleId;
                    existing = new RelationshipDescription(rel.Id, rel.ReverseId, cd, rel.Name, roleId, rel.Description, rel.Constraints?.IsReadOnly);
                    cd.AddRelationship(existing);
                }
                else
                {
                    //sinon, on la modifie vite fait
                    existing.Description = rel.Description ?? existing.Description;
                    existing.IsReadOnly = rel.Constraints?.IsReadOnly ?? existing.IsReadOnly;
                }

                if (rel.Path.Length > 2)
                {
                    throw new Exception($"Relation {rel.Name} path should be less or equal to 2: current is {rel.Path.Length}");
                }

                existing.SetPath(rel.Path.Select((p, idx) =>
                {
                    var targetClass = metaModel.GetClassDescription(p.MetaClassName);
                    var newPath = new PathDescription(p.Id, p.RoleName, p.RoleId, targetClass, p.Multiplicity, p.Condition);
                    //On ajoute les attributs de lien
                    ReadProperties(p.Properties, newPath, PropertyScope.Relationship);
                    ReadProperties(interfaceLinkAttributes.Properties, newPath, PropertyScope.Relationship);
                    return newPath;
                }).ToList());
            }
        }

        private static void ReadProperties(IEnumerable<PivotPropertyDescription> properties, IElementWithProperties elmWithProps, PropertyScope scope = PropertyScope.Class)
        {
            if (properties == null)
            {
                return;
            }
            foreach (PivotPropertyDescription property in properties)
            {
                if (!(elmWithProps.GetPropertyDescription(property.Name, false) is PropertyDescription existing))
                {
                    existing = new PropertyDescription(property.Name,
                                                       property.Id,
                                                       property.Description,
                                                       property.Constraints?.PropertyType,
                                                       property.Constraints?.IsRequired,
                                                       property.Constraints?.IsReadOnly,
                                                       property.Constraints?.IsUnique,
                                                       property.Constraints?.IsTranslatable,
                                                       property.Constraints?.IsFormattedText,
                                                       property.Constraints?.MaxLength,
                                                       scope)
                    {
                        SetterFormat = property.SetterFormat ?? PropertyDescription.DefaultSetterFormat,
                        GetterFormat = property.GetterFormat ?? PropertyDescription.DefaultGetterFormat
                    };

                    elmWithProps.AddProperty(existing);
                }
                else
                {
                    existing.Description = property.Description ?? existing.Description;
                    existing.SetterFormat = property.SetterFormat ?? existing.SetterFormat ?? PropertyDescription.DefaultSetterFormat;
                    existing.GetterFormat = property.GetterFormat ?? existing.GetterFormat ?? PropertyDescription.DefaultGetterFormat;
                    existing.IsRequired = property.Constraints?.IsRequired ?? existing.IsRequired;
                    existing.IsReadOnly = property.Constraints?.IsReadOnly ?? existing.IsReadOnly;
                    existing.IsUnique = property.Constraints?.IsUnique ?? existing.IsUnique;
                    existing.IsTranslatable = property.Constraints?.IsTranslatable ?? existing.IsTranslatable;
                    existing.IsFormattedText = property.Constraints?.IsFormattedText ?? existing.IsFormattedText;
                }

                if (property.EnumValues != null)
                {
                    foreach (PivotEnumDescription e in property.EnumValues)
                    {
                        EnumDescription @enum;
                        if(property.Constraints.PropertyType == "Int" && int.TryParse(e.InternalValue, out var internalValueInt))
                        {
                            @enum = new EnumDescription(e.Name, e.Id, e.Description, internalValueInt, e.Order);
                        }
                        else
                        {
                            @enum = new EnumDescription(e.Name, e.Id, e.Description, e.InternalValue, e.Order);
                        }
                        existing.AddEnumValue(@enum);
                    }
                }
            }
        }
    }
}
