using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    /*
     *  Copy software technologies from source to target repository
     */
    public class Test6 : AbstractTest
    {
        public Test6(Parameters parameters) : base(parameters) { }
        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            var metaclass = GetMetaClass(MetaClassNames.SoftwareTechnology);
            await TransferTest(metaclass);
        }
    }
}
