using Mega.WebService.GraphQL.Tests.Sources.Tests;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using static Mega.WebService.GraphQL.Tests.Sources.Tests.AbstractTest;

namespace Mega.WebService.GraphQL.Tests.Models
{
    public class TestModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nameDisplay")]
        public string NameDisplay { get; set; }

        [JsonProperty("namespace")]
        public string NameSpace { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("hideSourceRepository")]
        public bool HideSourceRepository { get; set; }

        private AbstractTest _test;

        public async Task<ProgressionModel> Run(Parameters parameters, ProgressionModel progression)
        {
            _test = (AbstractTest)(Activator.CreateInstance(Type.GetType(NameSpace), parameters));
            var result = await _test.Run(null, progression);
            _test = null;
            return result;
        }

        /*public Result GetResult()
        {
            return _test?.TestResult ?? null;
        }*/
    }
}
