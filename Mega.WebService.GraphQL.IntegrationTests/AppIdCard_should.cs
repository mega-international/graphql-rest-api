using FluentAssertions;
using GraphQL;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.DTO;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    [ImportMgr("AppIdCard_should.mgr")]
    public class AppIdCard_should : BaseFixture
    {
        public AppIdCard_should(GlobalFixture fixture, ITestOutputHelper output) : base(fixture, output)
        { }

        [Theory(Skip = "Waiting for correction of WI 36530")]
        [InlineData("Namea", "A", "", new string[] { "OoZ2CiGCVvIV", "nnZ2)hGCVP1V", "gnZ27kGCVDuV", "DnZ2rhGCVvlU", "omZ2ejGCVjcV" })]
        [InlineData("NameB", "CodeC", "comment", new string[] { "OoZ2CiGCVvIV", "nnZ2)hGCVP1V", "gnZ27kGCVDuV", "DnZ2rhGCVvlU", "omZ2ejGCVjcV" })]
        [InlineData("NameC", "CodeD", "Commentaire inexistant", new string[] { "hLit))z9VbnI", "omZ2ejGCVjcV" })]
        public async Task Get_Application_Filtered_With_Or_Separator_And_Ordered_By_Name_Asc(string criteriaName, string criteriaCode, string criteriaComment, string[] expected)
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("ITPM");
            var request = new GraphQLRequest()
            {
                Query =
                @"query
                {
                    application
                    (
                        orderBy: [ name_ASC ],
                        filter:
                        {
                            or:
                            [
                                {name_contains: """ + criteriaName + @"""},
                                {applicationCode_contains: """ + criteriaCode + @"""},
                                {comment_contains: """ + criteriaComment + @"""}
                            ]
                        }
                    )
                    {
                        id
                        name
                    }
                }"
            };

            var response = await graphQLClient.SendQueryAsync<ApplicationResponse>(request);

            response.Should().HaveNoError();
            var applications = response.Data.Application;
            applications.Select(app => app.Id).Should().Equal(expected);
        }

        [Fact]
        public async Task Get_Application_With_Recursive_Relationships()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("ITPM");
            var request = new GraphQLRequest()
            {
                Query =
                @"query {
                    application (filter: {id:""hLit))z9VbnI""}) {
                        id
                        name
                        businessValueAPM
                        functionalSupportAPM
                        technicalEfficiencyAPM
                        technologyCompliance
                        lastAssessmentDate
                        comment(format: RAW)
                        applicationCode
                        applicationType
                        versionNumber
                        cloudComputing
                        currentNumberOfUsers
                        beginLifeDate
                        endLifeDate
                        applicationOwner_PersonSystem {
                            name
                            email
                        }
                        initiative {
                            portfolio {
                                name
                                portfolioManager_PersonSystem {
                                    name
                                    email
                                }
                            }
                        }
                        businessCapability {
                            name
                            comment(format: RAW)
                        }
                        functionality {
                            name
                            comment(format: RAW)
                        }
                        businessProcess {
                            name
                            comment(format: RAW)
                        }
                        softwareTechnology_UsedTechnology {
                            id
                            name
                        }
                        businessOwner_PersonSystem {
                            name
                            email
                        }
                        iTOwner_PersonSystem {
                            name
                            email
                        }
                        softwareDesigner_PersonSystem {
                            name
                            email
                        }
                        financialController_PersonSystem {
                            name
                            email
                        }
                        softwareInstallation {
                            name
                            serverDeployed_DeploymentSupport {
                                name
                            }
                            site_DeploymentSupport {
                                name
                            }
                            usageContext_ApplicationDeploymentContext {
                                name
                                orgUnit_DeploymentUser {
                                    name
                                }
                                numberOfUsers
                            }
                        }
                    }
                }"
            };
            var response = await graphQLClient.SendQueryAsync<ApplicationResponse>(request);

            response.Should().HaveNoError();
            var applications = response.Data.Application;
            applications.Should().HaveCount(1);
            var application = applications[0];
            application.Should().BeEquivalentTo(new Application
            {
                Id = "hLit))z9VbnI",
                Name = "AppCardDemo",
                BusinessValueAPM = null,
                FunctionalSupportAPM = null,
                TechnicalEfficiencyAPM = null,
                TechnologyCompliance = "Expected",
                LastAssessmentDate = null,
                Comment = "",
                ApplicationCode = "CodeDemo",
                ApplicationType = "System",
                VersionNumber = "VersionDemo",
                CloudComputing = "Cloud_SaaS",
                CurrentNumberOfUsers = 0,
                BeginLifeDate = "2020-08-11",
                EndLifeDate = "2020-08-12",
                ApplicationOwner_PersonSystem = new List<ApplicationOwner_PersonSystem>
                {
                    new ApplicationOwner_PersonSystem
                    {
                        Name = "VAURY Jean-Marie",
                        Email = "jmvaury@mega.com"
                    }
                },
                Initiative = new List<Initiative>
                {
                    new Initiative
                    {
                        Portfolio = new List<Portfolio>
                        {
                            new Portfolio
                            {
                                Name = "AppCardDemoPortfolio",
                                PortfolioManager_PersonSystem = new List<PortfolioManager_PersonSystem>
                                {
                                    new PortfolioManager_PersonSystem
                                    {
                                        Name = "VAURY Jean-Marie",
                                        Email = "jmvaury@mega.com"
                                    }
                                }
                            }
                        }
                    }
                },
                BusinessCapability = new List<BusinessCapability>
                {
                    new BusinessCapability
                    {
                        Name = "BusinessCapabilityDemo",
                        Comment = "My Business Capability Comment"
                    }
                },
                Functionality = new List<Functionality>
                {
                    new Functionality
                    {
                        Name = "FunctionalityDemo",
                        Comment = "My Functionality Comment"
                    }
                },
                BusinessProcess = new List<BusinessProcess>
                {
                    new BusinessProcess
                    {
                        Name = "BusinessProcessDemo",
                        Comment = "My BusinessProcess Comment"
                    }
                },
                SoftwareTechnology_UsedTechnology = new List<SoftwareTechnology_UsedTechnology>
                {
                    new SoftwareTechnology_UsedTechnology
                    {
                        Id = "FKitzI1AV9iK",
                        Name = "SoftwareTechnologyDemo"
                    }
                },
                BusinessOwner_PersonSystem = new List<BusinessOwner_PersonSystem>
                {
                    new BusinessOwner_PersonSystem
                    {
                        Name = "GIBELIN Laurent",
                        Email = "lgibelin@mega.com"
                    }
                },
                ITOwner_PersonSystem = new List<ITOwner_PersonSystem>
                {
                    new ITOwner_PersonSystem
                    {
                        Name = "CRONIER Sébastien",
                        Email = "scronier@mega.com"
                    }
                },
                SoftwareDesigner_PersonSystem = new List<SoftwareDesigner_PersonSystem>
                {
                    new SoftwareDesigner_PersonSystem
                    {
                        Name = "CANONICA Sébastien",
                        Email = "scanonica@mega.com"
                    }
                },
                FinancialController_PersonSystem = new List<FinancialController_PersonSystem>
                {
                    new FinancialController_PersonSystem
                    {
                        Name = "METGE Alain",
                        Email = "ametge@mega.com"
                    }
                },
                SoftwareInstallation = new List<SoftwareInstallation>
                {
                    new SoftwareInstallation
                    {
                        Name = "AppCardSoftwareInstallationDemo",
                        ServerDeployed_DeploymentSupport = new List<ServerDeployed_DeploymentSupport>
                        {
                            new ServerDeployed_DeploymentSupport
                            {
                                Name = "ServerDeployedDemo"
                            }
                        },
                        Site_DeploymentSupport = new List<Site_DeploymentSupport>
                        {
                            new Site_DeploymentSupport
                            {
                                Name = "SiteDemo"
                            }
                        },
                        UsageContext_ApplicationDeploymentContext = new List<UsageContext_ApplicationDeploymentContext>
                        {
                            new UsageContext_ApplicationDeploymentContext
                            {
                                Name = "UsageContextDemo",
                                NumberOfUsers = 2,
                                OrgUnit_DeploymentUser = new List<OrgUnit_DeploymentUser>
                                {
                                    new OrgUnit_DeploymentUser
                                    {
                                        Name = "OrgUnitDemo1"
                                    },
                                    new OrgUnit_DeploymentUser
                                    {
                                        Name = "OrgUnitDemo2"
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        [Fact]
        public async Task Get_User_By_Id()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("ITPM");
            var request = new GraphQLRequest()
            {
                Query = @"query user {
                  personSystem(filter:{id:""zTcUyq7ETbI9""}) {
                    id
                    email
                    name
                  }
                }"
            };
            var response = await graphQLClient.SendQueryAsync<PersonSystemResponse>(request);
            response.Should().HaveNoError();
            var personsSystem = response.Data.PersonSystem;
            personsSystem.Should().HaveCount(1);
            var personSystem = personsSystem[0];
            personSystem.Should().BeEquivalentTo(
                new PersonSystem
                {
                    Id = "zTcUyq7ETbI9",
                    Name = "VAURY Jean-Marie",
                    Email = "jmvaury@mega.com"
                });
        }

        [Theory(Skip = "Waiting for correction of WI 36530")]
        [InlineData("NameA", "codeC", "Inexisting comment", new string[] { "bLDJjDfFVfP3", "YMDJSLfFVPU3" })]
        [InlineData("NameC", "codeA", "Inexisting comment", new string[] { "bLDJjDfFVfP3", "(LDJJLfFVjS3" })]
        [InlineData("NameB", "codeB", "Inexisting comment", new string[] { "YMDJSLfFVPU3", "(LDJJLfFVjS3" })]
        [InlineData("NameC", "codeC", "my comment", new string[] { "FKitzI1AV9iK" })]
        public async Task Get_Software_Technologies_Filtered_With_Or_Separator_And_Ordered_By_Name_Asc(string criteriaName, string criteriaCode, string criteriaComment, string[] expected)
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("ITPM");
            var request = new GraphQLRequest()
            {
                Query =
                @"query
                {
                    softwareTechnology
                    (
                        orderBy: [ name_ASC ],
                        filter:
                        {
                            or:
                            [
                                {name_contains: """ + criteriaName + @"""},
                                {technologyCode_contains: """ + criteriaCode + @"""},
                                {comment_contains: """ + criteriaComment + @"""}
                            ]
                        }
                    )
                    {
                        id
                        name
                        comment(format: RAW)
                        technologyCode
                    }
                }"
            };
            var response = await graphQLClient.SendQueryAsync<SoftwareTechnologyResponse>(request);

            response.Should().HaveNoError();
            var softwareTechnologies = response.Data.SoftwareTechnology;
            softwareTechnologies.Select(st => st.Id).Should().Equal(expected);
        }

        [Fact]
        public async Task Get_Software_Technology_With_Recursive_Relationships()
        {
            var graphQLClient = await _fx.GetGraphQLClientAsync("ITPM");
            var request = new GraphQLRequest()
            {
                Query =
                @"query {
                    softwareTechnology (filter: {id:""FKitzI1AV9iK""}) {
                        id
                        name
                        comment(format: RAW)
                        technologyCode
                        companyStandard
                        beginLifeDate
                        endLifeDate
                        initiative {
                            portfolio {
                                name
                                portfolioManager_PersonSystem {
                                    name
                                }
                            }
                        }
                        financialController_PersonSystem {
                            name
                        }
                        technologyCorrespondent_PersonSystem {
                            name
                        }
                        technicalFunctionality {
                            name
                        }
                        type {
                            name
                        }
                        application {
                            id
                            name
                            applicationOwner_PersonSystem {
                                name
                            }
                        }
                    }
                }"
            };
            var response = await graphQLClient.SendQueryAsync<SoftwareTechnologyResponse>(request);

            response.Should().HaveNoError();
            var softwareTechnologies = response.Data.SoftwareTechnology;
            softwareTechnologies.Should().HaveCount(1);
            var softwareTechnology = softwareTechnologies[0];
            softwareTechnology.Should().BeEquivalentTo(new SoftwareTechnology
            {
                Id = "FKitzI1AV9iK",
                Name = "SoftwareTechnologyDemo",
                Comment = "My Comment Demo",
                TechnologyCode = "codeDemo",
                CompanyStandard = "Expected",
                BeginLifeDate = null,
                EndLifeDate = null,
                Initiative = new List<Initiative>
                {
                    new Initiative
                    {
                        Portfolio = new List<Portfolio>
                        {
                            new Portfolio
                            {
                                Name = "PortfolioSTDemo",
                                PortfolioManager_PersonSystem = new List<PortfolioManager_PersonSystem>
                                {
                                    new PortfolioManager_PersonSystem
                                    {
                                        Name = "VAURY Jean-Marie",
                                    }
                                }
                            }
                        }
                    }
                },
                FinancialController_PersonSystem = new List<FinancialController_PersonSystem>
                {
                    new FinancialController_PersonSystem
                    {
                        Name = "VAURY Jean-Marie",
                    }
                },
                TechnologyCorrespondent_PersonSystem = new List<TechnologyCorrespondent_PersonSystem>
                {
                    new TechnologyCorrespondent_PersonSystem
                    {
                        Name = "CRONIER Sébastien",
                    }
                },
                TechnicalFunctionality = new List<TechnicalFunctionality>
                {
                    new TechnicalFunctionality
                    {
                        Name = "TechnicalFunctionalityDemo",
                    }
                },
                Type = new List<Type>
                {
                    new Type
                    {
                        Name = "TypeSTDemo",
                    }
                },
                Application = new List<Application>
                {
                    new Application
                    {
                        Id = "hLit))z9VbnI",
                        Name = "AppCardDemo",
                        ApplicationOwner_PersonSystem = new List<ApplicationOwner_PersonSystem>
                        {
                            new ApplicationOwner_PersonSystem
                            {
                                Name = "VAURY Jean-Marie"
                            }
                        }
                    }
                }
            });
        }

        private class ApplicationResponse
        {
            public List<Application> Application { get; set; }
        }

        private class SoftwareTechnologyResponse
        {
            public List<SoftwareTechnology> SoftwareTechnology { get; set; }
        }

        private class PersonSystemResponse
        {
            public List<PersonSystem> PersonSystem { get; set; }
        }

        private class LanguageResponse
        {
            public List<Language> Language { get; set; }
        }

        private class Application : BasicObject
        {
            public string ApplicationCode { get; set; }
            public string BusinessValueAPM { get; set; }
            public string FunctionalSupportAPM { get; set; }
            public string TechnicalEfficiencyAPM { get; set; }
            public string TechnologyCompliance { get; set; }
            public string LastAssessmentDate { get; set; }
            public string ApplicationType { get; set; }
            public string VersionNumber { get; set; }
            public string CloudComputing { get; set; }
            public int CurrentNumberOfUsers { get; set; }
            public string BeginLifeDate { get; set; }
            public string EndLifeDate { get; set; }

            public List<Initiative> Initiative { get; set; }
            public List<ApplicationOwner_PersonSystem> ApplicationOwner_PersonSystem { get; set; }
            public List<BusinessCapability> BusinessCapability { get; set; }
            public List<Functionality> Functionality { get; set; }
            public List<BusinessProcess> BusinessProcess { get; set; }
            public List<SoftwareTechnology_UsedTechnology> SoftwareTechnology_UsedTechnology { get; set; }
            public List<BusinessOwner_PersonSystem> BusinessOwner_PersonSystem { get; set; }
            public List<ITOwner_PersonSystem> ITOwner_PersonSystem { get; set; }
            public List<SoftwareDesigner_PersonSystem> SoftwareDesigner_PersonSystem { get; set; }
            public List<FinancialController_PersonSystem> FinancialController_PersonSystem { get; set; }
            public List<SoftwareInstallation> SoftwareInstallation { get; set; }
        }

        private class SoftwareTechnology : BasicObject
        {
            public string TechnologyCode { get; set; }
            public string CompanyStandard { get; set; }
            public string BeginLifeDate { get; set; }
            public string EndLifeDate { get; set; }
            public List<Initiative> Initiative { get; set; }
            public List<FinancialController_PersonSystem> FinancialController_PersonSystem { get; set; }
            public List<TechnologyCorrespondent_PersonSystem> TechnologyCorrespondent_PersonSystem { get; set; }
            public List<TechnicalFunctionality> TechnicalFunctionality { get; set; }
            public List<Type> Type { get; set; }
            public List<Application> Application { get; set; }

            public override string ToString()
            {
                return "software technology: " + Name;
            }
        }

        private class Initiative : BasicObject
        {
            public List<Portfolio> Portfolio { get; set; }
        }

        private class Portfolio : BasicObject
        {
            public List<PortfolioManager_PersonSystem> PortfolioManager_PersonSystem { get; set; }
        }

        private class PortfolioManager_PersonSystem : ObjectWithEmail {}
        private class ApplicationOwner_PersonSystem : ObjectWithEmail { }
        private class BusinessCapability : BasicObject { }
        private class Functionality : BasicObject { }
        private class BusinessProcess : BasicObject { }
        private class SoftwareTechnology_UsedTechnology : BasicObject { }
        private class BusinessOwner_PersonSystem : ObjectWithEmail { }
        private class ITOwner_PersonSystem : ObjectWithEmail { }
        private class SoftwareDesigner_PersonSystem : ObjectWithEmail { }
        private class FinancialController_PersonSystem : ObjectWithEmail { }
        private class SoftwareInstallation : BasicObject
        {
            public List<ServerDeployed_DeploymentSupport> ServerDeployed_DeploymentSupport { get; set; }
            public List<Site_DeploymentSupport> Site_DeploymentSupport { get; set; }
            public List<UsageContext_ApplicationDeploymentContext> UsageContext_ApplicationDeploymentContext { get; set; }
        }
        private class ServerDeployed_DeploymentSupport : BasicObject { }
        private class Site_DeploymentSupport : BasicObject { }
        private class UsageContext_ApplicationDeploymentContext : BasicObject
        {
            public List<OrgUnit_DeploymentUser> OrgUnit_DeploymentUser { get; set; }
            public int NumberOfUsers { get; set; }
        }
        private class OrgUnit_DeploymentUser : BasicObject { }
        private class TechnologyCorrespondent_PersonSystem : BasicObject { }
        private class TechnicalFunctionality : BasicObject { }
        private class Type : BasicObject { }

        private class ObjectWithEmail : BasicObject
        {
            public string Email { get; set; }
        }

        private class CurrentContext
        {
            public string UserId { get; set; }
            public string LanguageId { get; set; }
            public string LanguageCode { get; set; }
            public string LanguageName { get; set; }
        }

        private class Language
        {
            public string LanguageId { get; set; }
            public string LanguageCode { get; set; }
            public string LanguageName { get; set; }
        }

        private class EmailObject : BasicObject
        {
            public string Email { get; set; }
        }

        private class PersonSystem : EmailObject {}
    }
}
