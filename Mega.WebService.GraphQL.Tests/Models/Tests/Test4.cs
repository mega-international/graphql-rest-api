using Mega.WebService.GraphQL.Tests.Models.Metaclasses;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    /*
     *  Copy business capabilities from source to target repository
     */
    public class Test4 : AbstractTest
    {
        public Test4(Parameters parameters) : base(parameters) { }
        protected override void Steps(ITestParam oTestParam)
        {
            MetaClass metaclass = GetMetaClass(MetaClassNames.BusinessCapability);
            TransferTest(metaclass);
        }
    }
}
