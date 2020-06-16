using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    /*
     *  Copy links between applications and business capabilities
     */
    public class Test5 : AbstractTest
    {
        public Test5(Parameters parameters) : base(parameters) { }
        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            var metaclass1 = GetMetaClass(MetaClassNames.Application);
            var metaclass2 = GetMetaClass(MetaClassNames.BusinessCapability);
            var linkName = Application.MetaFieldNames.businessCapability;
            await LinkTransferTest(metaclass1, metaclass2, linkName);
        }
    }
}
