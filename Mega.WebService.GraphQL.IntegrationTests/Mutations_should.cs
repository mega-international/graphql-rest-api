using FluentAssertions;
using GraphQL;
using GraphQL.Client.Http;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.DTO;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Local deserialization only")]
    // ReSharper disable once InconsistentNaming
    public class Mutations_should : BaseFixture
    {
        // ReSharper disable once InconsistentNaming
        private GraphQLHttpClient _graphQLClient;

        public Mutations_should(GlobalFixture fixture, ITestOutputHelper output) : base(fixture, output)
        { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _graphQLClient = await _fx.GetGraphQLClientAsync("ITPM");
        }

        [Fact]
        public async void Update_multiple_applications()
        {
            var request1 = new GraphQLRequest
            {
                Query = @"mutation{application:createUpdateApplication(application:{name:""Name of the application 1""}){id name}}"
            };
            var response1 = await _graphQLClient.SendQueryAsync<CreateUpdateApplication>(request1);
            response1.Should().HaveNoError();
            response1.Data.Application.Should().BeEquivalentTo(new ApplicationInstance { Name = "Name of the application 1" });
            var request2 = new GraphQLRequest
            {
                Query = @"mutation{application:createUpdateApplication(application:{name:""Name of the application 2""}){id name}}"
            };
            var response2 = await _graphQLClient.SendQueryAsync<CreateUpdateApplication>(request2);
            response2.Should().HaveNoError();
            response2.Data.Application.Should().BeEquivalentTo(new ApplicationInstance { Name = "Name of the application 2" });
            var request3 = new GraphQLRequest
            {
                Query = @"mutation{application:updateManyApplication(filter:{name_contains:""Name of the application""}application:{comment:""comment""}){id name comment}}"
            };
            var response3 = await _graphQLClient.SendQueryAsync<UpdateApplication>(request3);
            response3.Should().HaveNoError();
            response3.Data.Application.Should().BeEquivalentTo(new List<ApplicationInstance>
            {
                new ApplicationInstance { Name = "Name of the application 1", Comment = "comment" },
                new ApplicationInstance { Name = "Name of the application 2", Comment = "comment" }
            });
        }

        [Fact]
        public async void Update_multiple_applications_and_add_link()
        {
            var request1 = new GraphQLRequest
            {
                Query = @"mutation{application:createUpdateApplication(id: ""idExt1"", idType: EXTERNAL, application: {name: ""Name of the application 1""}){externalId name}}"
            };
            var response1 = await _graphQLClient.SendQueryAsync<CreateUpdateApplication>(request1);
            response1.Should().HaveNoError();
            response1.Data.Application.Should().BeEquivalentTo(new ApplicationInstance { ExternalId = "idExt1", Name = "Name of the application 1" });

            var request2 = new GraphQLRequest
            {
                Query = @"mutation{application:createUpdateApplication(id: ""idExt2"", idType: EXTERNAL, application: {name: ""Name of the application 2""}){externalId name}}"
            };
            var response2 = await _graphQLClient.SendQueryAsync<CreateUpdateApplication>(request2);
            response2.Should().HaveNoError();
            response2.Data.Application.Should().BeEquivalentTo(new ApplicationInstance { ExternalId = "idExt2", Name = "Name of the application 2" });

            var request3 = new GraphQLRequest
            {
                Query = @"mutation{businessProcess:createUpdateBusinessProcess(id: ""idExt3"", idType: EXTERNAL, businessProcess: {name: ""Name of the business process""}){externalId name}}"
            };
            var response3 = await _graphQLClient.SendQueryAsync<CreateUpdateBusinessProcess>(request3);
            response3.Should().HaveNoError();
            response3.Data.BusinessProcess.Should().BeEquivalentTo(new BusinessProcessInstance { ExternalId = "idExt3", Name = "Name of the business process" });

            var request4 = new GraphQLRequest
            {
                Query = @"mutation updateManyApplication {
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
                        }"
            };
            var response4 = await _graphQLClient.SendQueryAsync<UpdateApplication>(request4);
            response4.Should().HaveNoError();
            response4.Data.Application.Should().BeEquivalentTo(new List<ApplicationInstance>
            {
                new ApplicationInstance { Name = "Name of the application 1", ApplicationType = "SoftwarePackage", ExternalId = "idExt1",
                    BusinessProcess = new List<BusinessProcessInstance>{ new BusinessProcessInstance { ExternalId = "idExt3" } } },
                new ApplicationInstance { Name = "Name of the application 2", ApplicationType = "SoftwarePackage", ExternalId = "idExt2",
                    BusinessProcess = new List<BusinessProcessInstance>{ new BusinessProcessInstance { ExternalId = "idExt3" } } }
            });
        }

        [Fact]
        public async void CreateUpdate_with_temporary_id()
        {
            var request = new GraphQLRequest
            {
                Query = @"mutation
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
                                id                                
                                name
                                initiative
                                {
                                    id
                                    name
                                    portfolio
                                    {
                                        id
                                        name
                                    }
                                }
                            }
                        }"
            };

            var response = await _graphQLClient.SendQueryAsync<CreateUpdateApplication>(request);

            response.Should().HaveNoError();
            response.Data.Application.Should().BeEquivalentTo(
                new ApplicationInstance
                {
                    Name = "Name of the application",
                    Initiative = new List<InitiativeInstance>
                        {
                            new InitiativeInstance
                            {
                                Name = "Name of the initiative",
                                Portfolio = new List<PortfolioInstance>
                                {
                                    new PortfolioInstance
                                    {
                                        Name = "Name of the portfolio"
                                    }
                                }
                            }
                        }
                });
        }

        [Fact]
        public async void Update_application_with_null_values()
        {
            var requestCreate = new GraphQLRequest
            {
                Query = @"mutation{
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
                        }}"
            };
            var responseCreate = await _graphQLClient.SendQueryAsync<CreateUpdateApplication>(requestCreate);
            responseCreate.Should().HaveNoError();
            responseCreate.Data.Application.Should().BeEquivalentTo(new ApplicationInstance {
                Name = "Name of the application 3",
                OperatingApplicationDate = "2020-12-25",
                ApplicationCode = "Code",
                Cost = 98.76,
                ApplicationStereotype = "DataManagement",
                ExternalId = "App3",
                FreezePastTimePeriod = true
            });


            var requestUpdate = new GraphQLRequest
            {
                Query = @"mutation{
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
                        }}"
            };
            var responseUpdate = await _graphQLClient.SendQueryAsync<CreateUpdateApplication>(requestUpdate);
            responseUpdate.Should().HaveNoError();
            responseUpdate.Data.Application.Should().BeEquivalentTo(new ApplicationInstance
            {
                Name = "Name of the application 3",
                OperatingApplicationDate = null,
                ApplicationCode = "",
                Cost = null,
                ApplicationStereotype = null,
                ExternalId = "",
                FreezePastTimePeriod = false
            });
        }

        [Fact]
        public async void Create_user_with_language_code()
        {
            var requestCreate = new GraphQLRequest
            {
                Query = @"mutation createUser {
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
                        }"
            };
            var responseCreate = await _graphQLClient.SendQueryAsync<CreateUpdatePersonSystem>(requestCreate);
            responseCreate.Should().HaveNoError();
            responseCreate.Data.User.Should().BeEquivalentTo(new PersonSystemInstance
            {
                Name = "Name of the user",
                DataLanguage = new DataLanguageInstance
                {
                    Name = "Italiano",
                    LanguageCode = "IT"
                }
            });

        }

        public override async Task DisposeAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"mutation deleteAll
                        {
                            deletePortfolio: deleteManyPortfolio(filter:{name_contains: ""Name of the portfolio""}) { deletedCount }
                            deleteInitiative: deleteManyInitiative(filter:{name_contains: ""Name of the initiative""}) { deletedCount }
                            deleteApplication: deleteManyApplication(filter:{name_contains: ""Name of the application""}) { deletedCount }
                            deleteBusinessProcess: deleteManyBusinessProcess(filter:{name_contains: ""Name of the business process""}) { deletedCount }
                            deletePersonSystem: deleteManyPersonSystem(filter:{name_contains: ""Name of the user""}) { deletedCount }
                        }"
            };
            var response = await _graphQLClient.SendQueryAsync<DeleteAllResponse>(request);
            var data = response.Data;
            data.DeletePortfolio?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            data.DeleteInitiative?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            data.DeleteApplication?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            data.DeleteBusinessProcess?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            data.DeletePersonSystem?.DeletedCount.Should().BeGreaterOrEqualTo(0);
            await base.DisposeAsync();            
        }

        public class UpdateApplication
        {
            public List<ApplicationInstance> Application { get; set; }
        }

        public class CreateUpdateApplication
        {
            public ApplicationInstance Application { get; set; }
        }

        public class CreateUpdateBusinessProcess
        {
            public BusinessProcessInstance BusinessProcess { get; set; }
        }

        public class CreateUpdatePersonSystem
        {
            public PersonSystemInstance User { get; set; }
        }

        public class DeleteAllResponse
        {
            public DeleteResult DeletePortfolio { get; set; }            
            public DeleteResult DeleteInitiative { get; set; }            
            public DeleteResult DeleteApplication { get; set; }
            public DeleteResult DeleteBusinessProcess { get; set; }
            public DeleteResult DeletePersonSystem { get; set; }
        }

        public class ApplicationInstance
        {
            public string Name { get; set; }
            public string Comment { get; set; }
            public string OperatingApplicationDate { get; set; }
            public string ApplicationCode { get; set; }
            public double? Cost { get; set; }
            public string ApplicationStereotype { get; set; }
            public string ApplicationType { get; set; }
            public string ExternalId { get; set; }
            public bool FreezePastTimePeriod { get; set; }
            public List<InitiativeInstance> Initiative { get; set; }
            public List<BusinessProcessInstance> BusinessProcess { get; set; }
        }

        public class BusinessProcessInstance
        {
            public string Name { get; set; }
            public string ExternalId { get; set; }
        }

        public class PersonSystemInstance
        {
            public string Name { get; set; }
            public DataLanguageInstance DataLanguage { get; set; }
        }

        public class InitiativeInstance
        {
            public string Name { get; set; }
            public List<PortfolioInstance> Portfolio { get; set; }
        }

        public class PortfolioInstance
        {
            public string Name { get; set; }
        }

        public class DataLanguageInstance
        {
            public string Name { get; set; }
            public string LanguageCode { get; set; }
        }
    }
}
