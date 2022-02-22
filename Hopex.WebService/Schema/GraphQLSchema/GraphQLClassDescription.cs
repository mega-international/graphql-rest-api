using Hopex.Model.Abstractions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.Abstractions.MetaModel;
using Mega.Macro.API.Library;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Modules.GraphQL.Schema.GraphQLSchema
{
    internal class GraphQLClassDescription
    {
        private readonly HashSet<GraphQLPropertyDescription> _properties = new HashSet<GraphQLPropertyDescription>(new GraphQLPropertyComparer());
        private readonly HashSet<GraphQLRelationshipDescription> _relationships = new HashSet<GraphQLRelationshipDescription>(new GraphQLRelationshiComparer());
        private bool _basePropertiesGenerated = false;

        public IClassDescription MetaClass { get; }
        public IEnumerable<GraphQLPropertyDescription> Properties => _properties;
        public IEnumerable<GraphQLRelationshipDescription> Relationships => _relationships;

        public GraphQLClassDescription(IClassDescription metaClass)
        {
            MetaClass = metaClass;
        }

        public void GenerateBasicFields(IDictionary<string, GraphQLClassDescription> graphQlClasses)
        {
            GenerateBaseProperties();
            foreach(var relationship in MetaClass.Relationships)
            {
                var lastTargetGraphQlClass = graphQlClasses[relationship.TargetClass.Name];
                var graphQlRelationship = new GraphQLRelationshipDescription(relationship, lastTargetGraphQlClass);
                _relationships.Add(graphQlRelationship);
            }
        }

        public void GenerateContextedFields()
        {
            foreach(var graphQlRelationship in _relationships)
            {
                var lastTargetGraphQlClass = graphQlRelationship.TargetClass;
                var relationship = graphQlRelationship.MetaAssociation;
                if(relationship.Path.Count() > 0)
                {
                    //Attributs de lien
                    var firstPath = relationship.Path[0];
                    lastTargetGraphQlClass.GeneratePathProperties(firstPath, graphQlRelationship);

                    //Attributs de l'interclasse
                    if(relationship.Path.Count() > 1)
                    {
                        lastTargetGraphQlClass.GenerateInterClassProperties(firstPath.TargetClass, graphQlRelationship);
                    }
                }
            }
        }

        private static Func<IClassDescription, IDictionary<string, object>, IEnumerable<ISetter>> CreateSetters(IDictionary<string, GraphQLClassDescription> graphQlClasses)
        {
            return (ent, args) =>
            {
                var graphQlClass = graphQlClasses [ent.Name];
                return graphQlClass.CreateSetter(args, graphQlClasses);
            };
        }

        private static Func<IClassDescription, IDictionary<string, object>, IEnumerable<ISetter>> CustomCreateSetters()
        {
            return (ent, args) =>
            {
                var setters = new List<ISetter>();
                foreach(var kv in args)
                {
                    var propertyName = kv.Key;
                    var value = kv.Value;
                    if(propertyName.Equals("customFields"))
                    {
                        setters.AddRange(ent.CreateCustomPropertySetters(value));
                    }
                    else
                    {
                        throw new Exception($"{kv.Key} is not a valid member for custom relationship input");
                    }
                }
                return setters;
            };
        }

        public IEnumerable<ISetter> CreateSetter(IDictionary<string, object> arguments, IDictionary<string, GraphQLClassDescription> graphQlClasses)
        {
            foreach(var kv in arguments)
            {
                var propertyName = kv.Key;
                var value = kv.Value;
                IEnumerable<ISetter> setters;
                if(propertyName.Equals("customFields"))
                {
                    setters = MetaClass.CreateCustomPropertySetters(value);
                }
                else if(propertyName.Equals("customRelationships"))
                {
                    value = new Tuple<object, Func<IClassDescription, IDictionary<string, object>, IEnumerable<ISetter>>>(value, CustomCreateSetters());
                    setters = MetaClass.CreateCustomRelationshipSetters(value);
                }
                else 
                {
                    IFieldDescription field;
                    if(propertyName == "dataLanguageCode")
                    {
                        field = MetaClass.FindPropertyDescriptionById(MetaAttributeLibrary.DataLanguage.Substring(1, 12));
                        if(field != null)
                        {
                            value = ((IMegaObject)value).MegaUnnamedField.Substring(1, 12);
                        }
                        else
                        {
                            throw new Exception($"dataLanguage is not a valid member of {MetaClass.Name}");
                        }
                    }
                    else
                    {
                        field = Properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))?.MetaAttribute;
                        if(field == null)
                        {
                            value = new Tuple<object, Func<IClassDescription, IDictionary<string, object>, IEnumerable<ISetter>>>(value, CreateSetters(graphQlClasses));
                            field = Relationships.FirstOrDefault(r => r.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))?.MetaAssociation;
                        }
                    }
                    setters = field?.CreateSetters(value) ?? throw new Exception($"{kv.Key} is not a valid member of {MetaClass.Name}");
                }
                foreach(var setter in setters)
                {
                    yield return setter;
                }
            }
        }

        private void GenerateBaseProperties()
        {
            if(!_basePropertiesGenerated)
            {
                GenerateProperties(MetaClass);
                _basePropertiesGenerated = true;
            }
        }

        private void GeneratePathProperties(IPathDescription path, GraphQLRelationshipDescription graphQlRelationship)
        {
            var reversePathes = graphQlRelationship.Reverse?.MetaAssociation.Path;
            GenerateProperties(path, graphQlRelationship, reversePathes);
        }

        private void GenerateInterClassProperties(IClassDescription interClass, GraphQLRelationshipDescription graphQlRelationship)
        {
            var reversePathes = graphQlRelationship.Reverse?.MetaAssociation.Path;
            GenerateProperties(interClass, graphQlRelationship, reversePathes?.Take(reversePathes.Length - 1), "link1");
        }

        private void GenerateProperties(IElementWithProperties element,
            GraphQLRelationshipDescription graphQLRelationship = null,
            IEnumerable<IPathDescription> reversePathes = null,
            string prefix = "")
        {
            var graphQLProperties = element.Properties.Select(p => new GraphQLPropertyDescription(p, prefix + SchemaBuilder.ToValidName(p.Name)));
            foreach(var newProperty in graphQLProperties)
            {
                GraphQLPropertyDescription existing;
                if(_properties.Add(newProperty))
                {
                    existing = newProperty;
                }
                else
                {
                    existing = _properties.First(p => _properties.Comparer.Equals(p, newProperty));
                }
                if(graphQLRelationship != null && reversePathes != null)
                {
                    existing.AddPathesToField(graphQLRelationship, reversePathes);
                }
            }
        }
    }
}
