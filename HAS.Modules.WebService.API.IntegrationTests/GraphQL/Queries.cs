using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HAS.Modules.WebService.API.IntegrationTests.GraphQL
{
    public class Queries : BaseTests
    {
        [OneTimeSetUp]
        public async Task PrepareDatabase()
        {
            await ClearDatabase();
            var response = await EnsureSuccessAndExpected(
                @"mutation createAll
                {
                    application1:createApplication(application:{name:""Name of the application 1""}){ id name }
                    application2:createApplication(application:{name:""Name of the application 2""}){ name }
                    softwareTechnology1:createSoftwareTechnology(softwareTechnology:{name:""Name of the software technology 1""}){ id name }
                    softwareTechnology2:createSoftwareTechnology(softwareTechnology:{name:""Name of the software technology 2""}){ id name }
                }",
                new List<Expectation>
                {
                    new Expectation(@"""Name of the application 1""", res => res.Data.application1.name),
                    new Expectation(@"""Name of the application 2""", res => res.Data.application2.name),
                    new Expectation(@"""Name of the software technology 1""", res => res.Data.softwareTechnology1.name),
                    new Expectation(@"""Name of the software technology 2""", res => res.Data.softwareTechnology2.name)
                });
            var applicationId1 = response.Data.application1.id;
            var softwareTechnologyId1 = response.Data.softwareTechnology1.id;
            var softwareTechnologyId2 = response.Data.softwareTechnology2.id;

            response = await SendQuery($@"mutation updateApplication
                        {{
                            application1:updateApplication(
                                id:""{applicationId1}""
                                application:
                                {{
                                    softwareTechnology_UsedTechnology:
                                    {{
                                        action:ADD list:[{{id:""{softwareTechnologyId1}""}}, {{id:""{softwareTechnologyId2}""}}]
                                    }}
                                }})
                            {{id name}}
                        }}");
            EnsureNoError(response);
        }

        [OneTimeTearDown]
        public async Task ClearDatabase()
        {
            var query = @"mutation deleteAll
                        {
                            deleteApplication: deleteManyApplication(filter:{name_contains: ""Name of the application""}) { deletedCount }
                            deleteSoftwareTechnology: deleteManySoftwareTechnology(filter:{name_contains: ""Name of the software technology""}) { deletedCount }
                        }";
            var response = await SendQuery(query);
            EnsureNoError(response);
            int count = response.Data.deleteApplication?.deletedCount;
            count.Should().BeGreaterOrEqualTo(0);

            count = response.Data.deleteSoftwareTechnology?.deletedCount;
            count.Should().BeGreaterOrEqualTo(0);
        }

        [Test]
        public async Task Count_applications()
        {
            await EnsureSuccessAndExpected(@"query applicationAggregatedValues
                {
                    applicationAggregatedValues(filter:{name_contains: ""Name of the application""}){id(function:COUNT)}
                }",
                "2",
                response => response.Data.applicationAggregatedValues[0].id);
        }

        [Test]
        public async Task Count_applications_softwareTechnology_UsedTechnology()
        {
            await EnsureSuccessAndExpected(@"query application_softwareTechnology_UsedTechnologyAggregatedValues
                {
                    application(filter:{name:""Name of the Application 1""})
                    {
                        softwareTechnology_UsedTechnologyAggregatedValues
                        {
                            id(function:COUNT)
                        }
                    }
                }",
                "2",
                response => response.Data.application[0].softwareTechnology_UsedTechnologyAggregatedValues[0].id);
        }
    }
}
