using Hopex.Model.Abstractions;

namespace Hopex.WebService.Tests.Mocks
{
    internal class MockMegaVersionInformation : MockMegaWrapperObject, IMegaVersionInformation
    {
        public string Name => "MEGA HOPEX V?R?";
    }
}
