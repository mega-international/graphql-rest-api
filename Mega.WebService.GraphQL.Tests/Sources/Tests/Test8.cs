using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    public class Test8 : AbstractTest
    {
        public Test8(Parameters parameters) : base(parameters) { }
        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            var metaclass = GetMetaClass(MetaClassNames.Application);
            var linkName = Application.MetaFieldNames.businessProcess;
            await LinkDeletionTest(metaclass, linkName);
        }
    }
}
