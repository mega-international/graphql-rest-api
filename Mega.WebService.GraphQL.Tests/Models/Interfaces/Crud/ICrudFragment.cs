using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models.Interfaces.Crud
{
    public interface ICrudFragment : ICrudSerializable
    {
        List<ICrudOutput> Outputs { get; set; }
        ICrudOutput GetOutput();
    }
}
