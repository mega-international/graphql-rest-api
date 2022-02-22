using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.PivotSchema.Models;
using Mega.Macro.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.Model.MetaModel
{
    [System.Diagnostics.DebuggerDisplay("{RoleName}->{TargetSchemaName}")]
    internal class PathDescription : IPathDescription
    {
        private readonly Dictionary<string, IPropertyDescription> _properties;
        public PathDescription(string roleName, string roleId)
        {
            RoleName = roleName;
            RoleId = Utils.NormalizeHopexId(roleId);
        }

        public PathDescription(string id, string roleName, string roleId, IClassDescription targetClass, string multiplicity, PivotPathConditionDescription pathConditionDescription)
        {
            Id = id;
            RoleName = roleName;
            RoleId = Utils.NormalizeHopexId(roleId);
            TargetClass = targetClass;
            Multiplicity = multiplicity;
            if (pathConditionDescription != null)
            {
                Condition = new PathConditionDescription(pathConditionDescription);
            }
            _properties = new Dictionary<string, IPropertyDescription>(StringComparer.OrdinalIgnoreCase);
        }
        public string Id { get; }
        public string RoleName { get; }
        public string RoleId { get; }
        public string Multiplicity { get; }
        public IPathConditionDescription Condition { get; }
        public string TargetSchemaId => TargetClass.Id;
        public string TargetSchemaName => TargetClass.Name;
        public IClassDescription TargetClass { get; private set; }
        public IEnumerable<IPropertyDescription> Properties
        {
            get
            {
                foreach(var p in _properties.Values)
                {
                    yield return p;
                }
            }
        }

        public void AddProperty(IPropertyDescription prop)
        {
            _properties.Add(prop.Name, prop);
        }

        public IPropertyDescription GetPropertyDescription(string propertyName, bool throwExceptionIfNotExists = true)
        {
            if(_properties.TryGetValue(propertyName, out IPropertyDescription cd))
            {
                return cd;
            }
            if(throwExceptionIfNotExists)
            {
                throw new Exception($"{propertyName} not found in path with role {RoleName}");
            }
            return null;
        }

        public IPropertyDescription FindPropertyDescriptionById(MegaId id)
        {
            var normalizedId = Utils.NormalizeHopexId(id).ToString();
            return _properties.Values
                .Where(prop => prop.Id.Equals(normalizedId))
                .FirstOrDefault();
        }
    }
}
