using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    /*
     *  Copy business capabilities from source to target repository
     */
    public class Test4 : AbstractTest
    {
        public Test4(Parameters parameters) : base(parameters) { }
        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            var metaclass = GetMetaClass(MetaClassNames.BusinessCapability);
            await TransferTest(metaclass);
        }
    }
}
