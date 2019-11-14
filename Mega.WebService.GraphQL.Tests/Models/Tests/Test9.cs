using Mega.WebService.GraphQL.Tests.Models.Metaclasses;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    public class Test9 : AbstractTest
    {
        public Test9(Parameters parameters) : base(parameters) { }
        protected override void Steps(ITestParam oTestParam)
        {
            MetaClass metaclass1 = GetMetaClass(MetaClassNames.Application);
            MetaClass metaclass2 = GetMetaClass(MetaClassNames.BusinessCapability);
            LinkDeletionTest(metaclass1, metaclass2);
        }
    }
}
