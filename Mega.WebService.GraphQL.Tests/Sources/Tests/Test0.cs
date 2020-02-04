using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    /*
     * Launch all test
     */
    public class Test0 : AbstractTest
    {
        public Test0(Parameters parameters) : base(parameters) { }
        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            string strFileName = HttpContext.Current.Server.MapPath("~/App_Data/TestsList.json");
            string strModels = File.ReadAllText(strFileName);
            JArray jModels = JObject.Parse(strModels).GetValue("TestList") as JArray;

            foreach(JObject jModel in jModels)
            {
               /* var testName = jModel.GetValue("namespace");
                AbstractTest test = (AbstractTest)(Activator.CreateInstance(Type.GetType(testModel.NameSpace), testParamsObj));

                //Run test and return result
                return await Task.FromResult(test.Run(null));*/
            }
        }
    }
}
