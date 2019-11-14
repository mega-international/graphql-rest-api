using Mega.WebService.GraphQL.Tests.Models.Metaclasses;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    public class Test10 : AbstractTest
    {
        public Test10(Parameters parameters) : base(parameters) { }
        protected override void Steps(ITestParam oTestParam)
        {
            MetaClass metaclass1 = GetMetaClass(MetaClassNames.Application);
            MetaClass metaclass2 = GetMetaClass(MetaClassNames.SoftwareTechnology);
            LinkDeletionTest(metaclass1, metaclass2);
        }
    }
}
