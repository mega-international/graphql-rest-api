using Mega.WebService.GraphQL.Tests.Models.Metaclasses;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    public class Test13 : AbstractTest
    {
        public Test13(Parameters parameters) : base(parameters) { }
        protected override void Steps(ITestParam oTestParam)
        {
            MetaClass metaclass = GetMetaClass(MetaClassNames.BusinessProcess);
            DeletionTest(metaclass);
        }
    }
}
