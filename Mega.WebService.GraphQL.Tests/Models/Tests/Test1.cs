using Mega.WebService.GraphQL.Tests.Models.Metaclasses;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    /*
     *  Copy applications from source to target repository
     */
    public class Test1 : AbstractTest
    {
        public Test1(Parameters parameters) : base(parameters) { }
        protected override void Steps(ITestParam oTestParam)
        {
            MetaClass metaclass = GetMetaClass(MetaClassNames.Application);
            TransferTest(metaclass);
        }
    }
}
