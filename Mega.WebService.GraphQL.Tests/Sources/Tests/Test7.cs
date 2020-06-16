using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    /*
     *  Copy links between applications and software technologies
     */
    public class Test7 : AbstractTest
    {
        public Test7(Parameters parameters) : base(parameters) { }
        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            var metaclass1 = GetMetaClass(MetaClassNames.Application);
            var metaclass2 = GetMetaClass(MetaClassNames.SoftwareTechnology);
            var linkName = Application.MetaFieldNames.softwareTechnology_UsedTechnology;
            await LinkTransferTest(metaclass1, metaclass2, linkName);
        }
    }
}
