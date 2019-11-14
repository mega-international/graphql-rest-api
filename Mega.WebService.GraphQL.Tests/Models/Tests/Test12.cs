using Mega.WebService.GraphQL.Tests.Models.Metaclasses;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    public class Test12 : AbstractTest
    {
        public Test12(Parameters parameters) : base(parameters) { }
        protected override void Steps(ITestParam oTestParam)
        {
            MetaClass metaclass = GetMetaClass(MetaClassNames.BusinessCapability);
            DeletionTest(metaclass);
        }
    }
}
