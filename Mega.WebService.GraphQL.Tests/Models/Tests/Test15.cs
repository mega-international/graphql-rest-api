using Mega.WebService.GraphQL.Tests.Models.Metaclasses;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    public class Test15 : AbstractTest
    {
        public Test15(Parameters parameters) : base(parameters) { }

        protected override void Initialisation()
        {
            _Requester = new GraphQLRequester($"{myServiceUrl}/api/{(asyncMode ? "async" : "")}/{schemaAudit}");
        }

        protected override void Steps(ITestParam oTestParam)
        {
            //MetaClass metaclass = GetMetaClass(MetaClassNames.Application);
            //TransferTest(metaclass);
        }
    }
}
