using Hopex.Model.DataModel;
using System.Threading.Tasks;

namespace Hopex.Model.Abstractions.DataModel
{
    public interface ISetter
    {
        Task UpdateElementAsync(HopexDataModel model, IModelElement element);
    }
}
