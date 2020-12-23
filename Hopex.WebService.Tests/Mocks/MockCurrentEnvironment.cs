using Hopex.Model.Abstractions;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockCurrentEnvironment : MockMegaWrapperObject, IMegaCurrentEnvironment
    {
        public IMegaToolkit Toolkit { get; private set; } = new MockToolkit();

        public IMegaSite Site { get; private set; } = new MockMegaSite();

        public string EnvironmentPath => @"C:\Data\MyEnv\Db";

        public IMegaResources Resources { get; internal set; } = new MockMegaResources();

        public virtual dynamic GetMacro(string macroId)
        {
            throw new System.NotImplementedException();
        }
    }
}
