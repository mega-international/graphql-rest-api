using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    public class Test9 : AbstractTest
    {
        public Test9(Parameters parameters) : base(parameters) { }
        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            var metaclass = GetMetaClass(MetaClassNames.Application);
            var linkName = Application.MetaFieldNames.businessCapability;
            await LinkDeletionTest(metaclass, linkName);
        }
    }
}
