using FluentAssertions;
using HAS.Modules.WebService.API.IntegrationTests.Assertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace HAS.Modules.WebService.API.IntegrationTests.GraphQL
{
    public class Mutations : BaseTests
    {
        [TearDown]
        public async Task ClearDatabase()
        {
            var query = @"mutation deleteAll
                        {
                            deletePortfolio: deleteManyPortfolio(filter:{name_contains: ""Name of the portfolio""}) { deletedCount }
                            deleteInitiative: deleteManyInitiative(filter:{name_contains: ""Name of the initiative""}) { deletedCount }
                            deleteApplication: deleteManyApplication(filter:{name_contains: ""Name of the application""}) { deletedCount }
                            deleteBusinessProcess: deleteManyBusinessProcess(filter:{name_contains: ""Name of the business process""}) { deletedCount }
                            deletePersonSystem: deleteManyPersonSystem(filter:{name_contains: ""Name of the user""}) { deletedCount }
                        }";
            var response = await SendQuery(query);
            EnsureNoError(response);

            int count = response.Data.deletePortfolio?.deletedCount;
            count.Should().BeGreaterOrEqualTo(0);

            count = response.Data.deleteInitiative?.deletedCount;
            count.Should().BeGreaterOrEqualTo(0);

            count = response.Data.deleteApplication?.deletedCount;
            count.Should().BeGreaterOrEqualTo(0);

            count = response.Data.deleteBusinessProcess?.deletedCount;
            count.Should().BeGreaterOrEqualTo(0);

            count = response.Data.deletePersonSystem?.deletedCount;
            count.Should().BeGreaterOrEqualTo(0);
        }

        [Test]
        public async Task Update_multiple_applications()
        {
            var query = @"mutation{application:createUpdateApplication(application:{name:""Name of the application 1""}){name}}";
            var response = await SendQuery(query);
            EnsureNoError(response);
            JObject app = response.Data.application;
            app.Should().MatchJSON(@"{""name"": ""Name of the application 1""}");

            query = @"mutation{application:createUpdateApplication(application:{name:""Name of the application 2""}){name}}";
            response = await SendQuery(query);
            EnsureNoError(response);
            app = response.Data.application;
            app.Should().MatchJSON(@"{""name"": ""Name of the application 2""}");

            query = @"mutation{application:updateManyApplication(filter:{name_contains:""Name of the application""}application:{comment:""comment""}){name comment}}";
            response = await SendQuery(query);
            EnsureNoError(response);
            JArray apps = response.Data.application;
            apps.Should().MatchJSON(@"[
                    {""name"": ""Name of the application 1"", ""comment"": ""comment""},
                    {""name"": ""Name of the application 2"", ""comment"": ""comment""}
                ]");
        }

        [Test]
        public async Task Update_multiple_applications_and_add_link()
        {
            var query = @"mutation{application:createUpdateApplication(id: ""idExt1"", idType: EXTERNAL, application: {name: ""Name of the application 1""}){externalId name}}";
            var response = await SendQuery(query);
            EnsureNoError(response);
            JObject item = response.Data.application;
            item.Should().MatchJSON(@"
                {
                    ""name"": ""Name of the application 1"",
                    ""externalId"": ""idExt1""
                }");
            
            query = @"mutation{application:createUpdateApplication(id: ""idExt2"", idType: EXTERNAL, application: {name: ""Name of the application 2""}){externalId name}}";
            response = await SendQuery(query);
            EnsureNoError(response);
            item = response.Data.application;
            item.Should().MatchJSON(@"
                {
                    ""name"": ""Name of the application 2"",
                    ""externalId"": ""idExt2""
                }");

            query = @"mutation{businessProcess:createUpdateBusinessProcess(id: ""idExt3"", idType: EXTERNAL, businessProcess: {name: ""Name of the business process""}){externalId name}}";
            response = await SendQuery(query);
            EnsureNoError(response);
            item = response.Data.businessProcess;
            item.Should().MatchJSON(@"
                {
                    ""name"": ""Name of the business process"",
                    ""externalId"": ""idExt3""
                }");

            query = @"mutation updateManyApplication {
                            application:updateManyApplication(filter:{externalId_starts_with:""idExt""} application:{
                                applicationType: SoftwarePackage
                                businessProcess:
                                {
                                    action: ADD
                                    list:[{ id: ""idExt3"" idType: EXTERNAL}]
                                }
                            }) {
                                name
                                applicationType
                                externalId
                                businessProcess { externalId }
                            }
                        }";
            response = await SendQuery(query);
            EnsureNoError(response);
            JArray apps = response.Data.application;
            apps.Should().MatchJSON(@"
            [
                {
                    ""name"": ""Name of the application 1"",
                    ""applicationType"": ""SoftwarePackage"",
                    ""externalId"": ""idExt1"",
                    ""businessProcess"":
                    [
                        { ""externalId"": ""idExt3"" }
                    ],
                },
                {
                    ""name"": ""Name of the application 2"",
                    ""applicationType"": ""SoftwarePackage"",
                    ""externalId"": ""idExt2"",
                    ""businessProcess"":
                    [
                        { ""externalId"": ""idExt3"" }
                    ],
                }
            ]");
        }

        [Test]
        public async Task CreateUpdate_with_temporary_id()
        {
            await EnsureSuccessAndExpected(@"mutation
            {
                portfolio: createUpdatePortfolio(id: ""MyNewPortfolioID1"" idType: TEMPORARY portfolio:
                {
                    name: ""Name of the portfolio""
                })
                {
                    id                                
                    name
                }
                initiative: createUpdateInitiative(id: ""MyNewInitiativeID1"" idType: TEMPORARY initiative:
                {
                    name: ""Name of the initiative""
                    portfolio:
                    {
                        action: ADD
                        list: [
                            {
                                id: ""MyNewPortfolioID1"" idType: TEMPORARY
                            }
                        ]
                    }
                })
                {
                    id                                
                    name
                }
                application: createUpdateApplication(id: ""MyNewApplicationID1"" idType: TEMPORARY application:
                {
                    name: ""Name of the application""
                    initiative:
                    {
                        action: ADD
                        list: [
                            {
                                id: ""MyNewInitiativeID1"" idType: TEMPORARY
                            }
                        ]
                    }
                })
                {                           
                    name
                    initiative
                    {
                        name
                        portfolio
                        {
                            name
                        }
                    }
                }
            }",
            @"{
                ""name"": ""Name of the application"",
                ""initiative"":
                [{
                    ""name"": ""Name of the initiative"",
                    ""portfolio"":  [{ ""name"": ""Name of the portfolio"" }]
                }]
            }",
            response => response.Data.application);
        }

        [Test]
        public async Task Update_application_with_null_values()
        {
            await EnsureSuccessAndExpected(
                @"mutation{
                            application: createUpdateApplication(application:{
                                name: ""Name of the application 3""
                                operatingApplicationDate: ""2020-12-25"",
                                applicationCode: ""Code"",
                                cost: 98.76,    
                                applicationStereotype: DataManagement,
                                externalId: ""App3"",
                                freezePastTimePeriod: true}, creationMode: BUSINESS)
                        {
                            name
                            operatingApplicationDate
                            applicationCode
                            cost
                            applicationStereotype
                            externalId
                            freezePastTimePeriod
                        }}",

                @"{
                    ""name"" : ""Name of the application 3"",
                    ""operatingApplicationDate"" : ""2020-12-25"",
                    ""applicationCode"" : ""Code"",
                    ""cost"" : 98.76,
                    ""applicationStereotype"" : ""DataManagement"",
                    ""externalId"" : ""App3"",
                    ""freezePastTimePeriod"" : true
                }", response => response.Data.application);

            await EnsureSuccessAndExpected(
                 @"mutation{
                            application: updateApplication(application:{
                                operatingApplicationDate: null,
                                applicationCode: null,
                                cost: null,    
                                applicationStereotype: null,
                                externalId: null,
                                freezePastTimePeriod: null}, id: ""App3"", idType: EXTERNAL)
                        {
                            name
                            operatingApplicationDate
                            applicationCode
                            cost
                            applicationStereotype
                            externalId
                            freezePastTimePeriod
                        }}",

                 @"{
                    ""name"": ""Name of the application 3"",
                    ""operatingApplicationDate"": null,
                    ""applicationCode"": """",
                    ""cost"": null,
                    ""applicationStereotype"": null,
                    ""externalId"": """",
                    ""freezePastTimePeriod"": false
                 }", response => response.Data.application);
        }

        [Test]
        public async Task Create_user_with_language_code()
        {
            await EnsureSuccessAndExpected(
                @"mutation createUser {
                    user: createPersonSystem(personSystem:{
                        name: ""Name of the user""
                        dataLanguageCode: IT
                    }) {
                        name
                        dataLanguage
                        {
                            ...on Language
                            {
                                name
                                languageCode
                            }
                        }
                    }
                }",
                @"{
                    ""name"": ""Name of the user"",
                    ""dataLanguage"":
                    {
                        ""name"": ""Italiano"",
                        ""languageCode"": ""IT""
                    }
                }", response => response.Data.user);
        }
    }
}
