using Mega.WebService.GraphQL.Tests.Models.Metaclasses;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    /*
     *  Copy links between applications and business capabilities
     */
    public class Test5 : AbstractTest
    {
        public Test5(Parameters parameters) : base(parameters) { }
        protected override void Steps(ITestParam oTestParam)
        {
            MetaClass metaclass1 = GetMetaClass(MetaClassNames.Application);
            MetaClass metaclass2 = GetMetaClass(MetaClassNames.BusinessCapability);
            LinkTransferTest(metaclass1, metaclass2);
        }
    }
}
