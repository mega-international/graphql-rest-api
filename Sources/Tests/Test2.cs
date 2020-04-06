using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    /*
     *  Copy business processes from source to target repository
     */
    public class Test2 : AbstractTest
    {
        public Test2(Parameters parameters) : base(parameters) { }
        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            var metaclass = GetMetaClass(MetaClassNames.BusinessProcess);
            await TransferTest(metaclass);
        }
    }
}
