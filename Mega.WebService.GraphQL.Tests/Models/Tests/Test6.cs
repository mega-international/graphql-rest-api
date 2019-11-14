using Mega.WebService.GraphQL.Tests.Models.Metaclasses;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    /*
     *  Copy software technologies from source to target repository
     */
    public class Test6 : AbstractTest
    {
        public Test6(Parameters parameters) : base(parameters) { }
        protected override void Steps(ITestParam oTestParam)
        {
            MetaClass metaclass = GetMetaClass(MetaClassNames.SoftwareTechnology);
            TransferTest(metaclass);
        }
    }
}
