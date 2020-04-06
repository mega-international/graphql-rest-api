using Mega.WebService.GraphQL.Tests.Models.Interfaces.Crud;
using Newtonsoft.Json.Linq;
using System;

namespace Mega.WebService.GraphQL.Tests.Models.Crud
{
    public class CrudResult : ICrudResult
    {
        public bool Ok { get; }
        public JToken Result { get; }

        public CrudResult(bool ok, JToken result)
        {
            Ok = ok;
            Result = result;
        }
    }
}
