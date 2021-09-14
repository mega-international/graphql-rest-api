using Hopex.Model.Abstractions.DataModel;
using System.Collections.Generic;

namespace Hopex.Model.Abstractions.MetaModel
{
    public interface IFieldDescription
    {
        IEnumerable<ISetter> CreateSetters(object value);
    }
}
