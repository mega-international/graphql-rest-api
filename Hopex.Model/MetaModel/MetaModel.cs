using System;
using System.Collections.Generic;
using Hopex.Model.Abstractions.MetaModel;

namespace Hopex.Model.MetaModel
{
    internal class HopexMetaModel : IHopexMetaModel
    {
        private readonly Dictionary<string, IClassDescription> _classes;

        public HopexMetaModel(IHopexMetaModel inherits, string name)
        {
            Inherits = inherits;
            Name = name;
            _classes = new Dictionary<string, IClassDescription>(StringComparer.OrdinalIgnoreCase);
        }

        public string Name { get; }
        public IEnumerable<IClassDescription> Classes => _classes.Values;

        public string Id { get; }
        public IHopexMetaModel Inherits { get; }

        public IClassDescription GetClassDescription(string schemaName, bool throwExceptionIfNotExists = true)
        {
            if (_classes.TryGetValue(schemaName, out var cd))
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
            _classes.Add(cd.Name, cd);
        }
    }
}
