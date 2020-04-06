using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    /*
     *  Copy links between applications and business processes
     */
    public class Test3 : AbstractTest
    {
        public Test3(Parameters parameters) : base(parameters) { }
        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            var metaclass1 = GetMetaClass(MetaClassNames.Application);
            var metaclass2 = GetMetaClass(MetaClassNames.BusinessProcess);
            var linkName = Application.MetaFieldNames.businessProcess;
            await LinkTransferTest(metaclass1, metaclass2, linkName);
        }
    }
}
