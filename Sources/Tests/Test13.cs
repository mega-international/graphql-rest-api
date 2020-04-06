using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    public class Test13 : AbstractTest
    {
        public Test13(Parameters parameters) : base(parameters) { }
        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            var metaclass = GetMetaClass(MetaClassNames.BusinessProcess);
            await DeletionTest(metaclass);
        }
    }
}
