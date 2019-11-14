using System.Collections.Generic;
using Hopex.Model.PivotSchema.Models;

namespace Hopex.Model.PivotSchema.Convertors
{
    public class ValidationContext
    {
        public List<string> Errors { get; } = new List<string>();

        public bool HasError => Errors.Count > 0;

        internal void AddValidationError(string message)
        {
            Errors.Add(message);
        }

        internal void ValidateClass(PivotClassDescription cls)
        {
            if (!Utils.CheckName(cls.Name))
            {
                AddValidationError($"{cls.Name} is not a valid class name.");
            }

            var properties = new HashSet<string>();
            var relationships = new HashSet<string>();


            if (cls.Properties != null)
            {
                foreach (var prop in cls.Properties)
                {
                    ValidateProperty(cls, properties, prop);
                }
            }
            if (cls.Relationships != null)
            {
                foreach (var rel in cls.Relationships)
                {
                    ValidateRelationship(cls, properties, relationships, rel);
                }
            }
        }

        internal void ValidateRelationship(PivotClassDescription cls, HashSet<string> properties, HashSet<string> relationships, PivotRelationshipDescription rel)
        {
            if (string.IsNullOrWhiteSpace(rel.Name))
            {
                AddValidationError($"Relationship name is required for class {cls.Name}");
            }
            if (!Utils.CheckName(rel.Name))
            {
                AddValidationError($"{rel.Name} is not a valid relationship name.");
            }

            if (properties.Contains(rel.Name))
            {
                AddValidationError($"Relationship {rel.Name} is duplicate with a property for class {cls.Name}");
            }
            if (relationships.Contains(rel.Name))
            {
                AddValidationError($"Duplicate relationship {rel.Name} for class {cls.Name}");
            }
            if (rel.Path?.Length == 0)
            {
                AddValidationError($"Invalid path declaration for relationship {cls.Name}.{rel.Name}");
            }

            var ix = 0;
            foreach (var hop in rel.Path)
            {
                if (string.IsNullOrWhiteSpace(hop.RoleId))
                {
                    AddValidationError($"Invalid roleid for path {ix} in relationship {cls.Name}.{rel.Name}");
                }
                if (string.IsNullOrWhiteSpace(hop.RoleName))
                {
                    AddValidationError($"Invalid roleName for path {ix} in relationship {cls.Name}.{rel.Name}");
                }
                if (string.IsNullOrWhiteSpace(hop.MetaClassId))
                {
                    AddValidationError($"Invalid metaClassId for path {ix} in relationship {cls.Name}.{rel.Name}");
                }
                if (string.IsNullOrWhiteSpace(hop.MetaClassName))
                {
                    AddValidationError($"Invalid metaClassName for path {ix} in relationship {cls.Name}.{rel.Name}");
                }
                ix++;
            }
            relationships.Add(rel.Name);
        }

        internal void ValidateProperty(PivotClassDescription cls, HashSet<string> properties, PivotPropertyDescription prop)
        {
            if (string.IsNullOrWhiteSpace(prop.Name))
            {
                AddValidationError($"Property name is required for class {cls.Name}");
            }
            if (!Utils.CheckName(prop.Name))
            {
                AddValidationError($"{prop.Name} is not a valid property name.");
            }
            if (properties.Contains(prop.Name))
            {
                AddValidationError($"Duplicate property {prop.Name} for class {cls.Name}");
            }

            // check type
            // check formats
            // check prop.EnumValues

            properties.Add(prop.Name);
        }
    }
}
