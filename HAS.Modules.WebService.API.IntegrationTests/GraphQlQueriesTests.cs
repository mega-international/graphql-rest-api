using System.Net.Http;
using System.Text;
using NUnit.Framework;

namespace HAS.Modules.WebService.API.IntegrationTests
{
    public class Tests
    {
        private const string ServerAddress = "https://w-scr:5001";
        private const string Schema = "ITPM";
        private const string ApiKey = "3FHSS4WUdqBz4KwwnDLx7WAVMqo6oKeSRpBc8V1pmEEzebTHGmLptWog4QJAjDqc1asN8uLrbs1EH7FLAhqDzcx6";

        private readonly string _serverUrl = $"{ServerAddress}/hopexgraphql/api/{Schema}";


        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            const string query = "{" +
                                 "  query" +
                                 "  {" +
                                 "    application" +
                                 "    {" +
                                 "      id" +
                                 "      name" +
                                 "    }" +
                                 "  }" +
                                 "}";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-api-key", ApiKey);
                using (var request = new StringContent(query, Encoding.UTF8, "application/json"))
                {
                    using (var response = httpClient.PostAsync(_serverUrl, request).Result)
                    {
                        Assert.Pass();
                    }
                }
            }
        }
    }
}
