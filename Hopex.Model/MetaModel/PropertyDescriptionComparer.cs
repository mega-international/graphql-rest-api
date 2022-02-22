using Hopex.Model.Abstractions.MetaModel;
using System.Collections.Generic;

namespace Hopex.Model.MetaModel
{
    public class PropertyDescriptionComparer : IEqualityComparer<IPropertyDescription>
    {
        public bool Equals(IPropertyDescription x, IPropertyDescription y)
        {
            return y != null && (x != null && (x.Id == y.Id && x.Name == y.Name));
        }
        public int GetHashCode(IPropertyDescription obj)
        {
            return obj.Id.GetHashCode() ^ obj.Name.GetHashCode();
        }
    }
}
