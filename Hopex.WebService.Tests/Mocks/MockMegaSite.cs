using Hopex.Model.Abstractions;

namespace Hopex.WebService.Tests.Mocks
{
    internal class MockMegaSite : MockMegaWrapperObject, IMegaSite
    {
        public IMegaVersionInformation VersionInformation => new MockMegaVersionInformation();
    }
}
