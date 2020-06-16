using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models.Interfaces.Crud
{
    public interface ICrudOutput : ICrudSerializable
    {
        List<ICrudInput> Inputs { get; set; }
        List<ICrudOutput> Outputs { get; set; }
    }
}
