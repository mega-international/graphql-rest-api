using Mega.WebService.GraphQL.Tests.Models.Metaclasses;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    public class Test11 : AbstractTest
    {
        public Test11(Parameters parameters) : base(parameters) { }
        protected override void Steps(ITestParam oTestParam)
        {
            MetaClass metaclass = GetMetaClass(MetaClassNames.SoftwareTechnology);
            DeletionTest(metaclass);
        }
    }
}
