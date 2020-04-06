using Mega.WebService.GraphQL.Tests.Sources.FieldModels;
using Newtonsoft.Json.Linq;

namespace Mega.WebService.GraphQL.Tests.Models.Interfaces.Crud
{
    public interface ICrudInput : ICrudSerializable
    {
        JToken Value { get; set; }
        Field Field { get; set; }
    }
}
