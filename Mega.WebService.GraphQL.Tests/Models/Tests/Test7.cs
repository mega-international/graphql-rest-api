using Mega.WebService.GraphQL.Tests.Models.Metaclasses;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    /*
     *  Copy links between applications and software technologies
     */
    public class Test7 : AbstractTest
    {
        public Test7(Parameters parameters) : base(parameters) { }
        protected override void Steps(ITestParam oTestParam)
        {
            MetaClass metaclass1 = GetMetaClass(MetaClassNames.Application);
            MetaClass metaclass2 = GetMetaClass(MetaClassNames.SoftwareTechnology);
            LinkTransferTest(metaclass1, metaclass2);
        }
    }
}
