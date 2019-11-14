using Mega.WebService.GraphQL.Tests.Models.Metaclasses;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    /*
     *  Copy business processes from source to target repository
     */
    public class Test2 : AbstractTest
    {
        public Test2(Parameters parameters) : base(parameters) { }
        protected override void Steps(ITestParam oTestParam)
        {
            MetaClass metaclass = GetMetaClass(MetaClassNames.BusinessProcess);
            TransferTest(metaclass);
        }
    }
}
