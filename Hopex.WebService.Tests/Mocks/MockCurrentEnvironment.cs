using Hopex.Model.Abstractions;

namespace Hopex.WebService.Tests.Mocks
{
    internal class MockCurrentEnvironment : MockMegaWrapperObject, IMegaCurrentEnvironment
    {
        public IMegaToolkit Toolkit => new MockToolkit();

        public IMegaSite Site => new MockMegaSite();

        public string EnvironmentPath => @"C:\Data\MyEnv\Db";
    }
}
