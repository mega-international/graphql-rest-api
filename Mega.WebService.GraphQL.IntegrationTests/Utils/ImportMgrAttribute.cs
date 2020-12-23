using System;

namespace Mega.WebService.GraphQL.IntegrationTests.Utils
{
    public class ImportMgrAttribute : Attribute
    {
        public string File { get; private set; }
        public ImportMgrAttribute(string file)
        {
            File = file;
        }
    }
}
