using Hopex.Model.Abstractions.MetaModel;

using System;
using System.Collections.Generic;

namespace Hopex.Model.MetaModel
{
    internal class HopexMetaModel : IHopexMetaModel
    {
        private readonly Dictionary<string, IClassDescription> _classesByName;
        private readonly Dictionary<string, IClassDescription> _classesById;
        private readonly Dictionary<string, IClassDescription> _interfaces;

        public HopexMetaModel(IHopexMetaModel inherits, string name)
        {
            Inherits = inherits;
            Name = name;
            _classesByName = new Dictionary<string, IClassDescription>(StringComparer.OrdinalIgnoreCase);
            _classesById = new Dictionary<string, IClassDescription>(StringComparer.OrdinalIgnoreCase);
            _interfaces = new Dictionary<string, IClassDescription>(StringComparer.OrdinalIgnoreCase);
        }

        public string Name { get; }
        public IEnumerable<IClassDescription> Classes => _classesByName.Values;
        public IEnumerable<IClassDescription> Interfaces => _interfaces.Values;

        public string Id { get; }
        public IHopexMetaModel Inherits { get; }

        public IClassDescription FindClassDescriptionById(string metaClassId)
        {
            _classesById.TryGetValue(metaClassId, out var cd);
            return cd;
        }

        public IClassDescription GetClassDescription(string schemaName, bool throwExceptionIfNotExists = true)
        {
            if (_classesByName.TryGetValue(schemaName, out var cd))
            {
                return cd;
            }

            if (throwExceptionIfNotExists)
            {
                throw new System.Exception($"{schemaName} not found");
            }
            return null;
        }

        internal void AddClass(IClassDescription cd)
        {
            _classesByName.Add(cd.Name, cd);
            _classesById.Add(cd.Id, cd);
        }

        public IClassDescription GetInterfaceDescription(string schemaName, bool throwExceptionIfNotExists = true)
        {
            if (_interfaces.TryGetValue(schemaName, out var cd))
            {
                return cd;
            }

            if (throwExceptionIfNotExists)
            {
                throw new Exception($"{schemaName} not found");
            }
            return null;
        }

        internal void AddInterface(IClassDescription cd)
        {
            _interfaces.Add(cd.Name, cd);
        }
    }
}
