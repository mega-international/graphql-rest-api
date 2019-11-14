using Mega.WebService.GraphQL.Tests.Models.Metaclasses;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    /*
     *  Copy links between applications and business processes
     */
    public class Test3 : AbstractTest
    {
        public Test3(Parameters parameters) : base(parameters) { }
        protected override void Steps(ITestParam oTestParam)
        {
            MetaClass metaclass1 = GetMetaClass(MetaClassNames.Application);
            MetaClass metaclass2 = GetMetaClass(MetaClassNames.BusinessProcess);
            LinkTransferTest(metaclass1, metaclass2);
        }
    }
}
