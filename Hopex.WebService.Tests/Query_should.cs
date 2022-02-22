using System;
using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Mega.Macro.API;
using Mega.Macro.API.Library;
using Xunit;
using Moq;

namespace Hopex.WebService.Tests
{
    public class Query_should : MockRootBasedFixture
    {
        [Fact]
        public async void Query_an_application()
        {
            var applicationId = MegaId.Create("IubjeRlyFfT1");
            var currentStateId = MegaId.Create("3dVhbTTT9j12");

            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(applicationId, MetaClassLibrary.Application)
                    .WithProperty(MetaAttributeLibrary.CurrentState, currentStateId.ToString() ))
                .WithObject(new MockMegaObject(currentStateId, MetaClassLibrary.StateUml)
                    .WithProperty(MetaAttributeLibrary.ShortName, "Production"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query{application{id,currentState{id,name}}}");

            resp.Should().MatchGraphQL("data.application[0].currentState.name", "Production");
        }

        [Fact]
        public async void Query_an_application_when_current_state_metaclass_does_not_exist()
        {
            var applicationId = MegaId.Create("IubjeRlyFfT1");
            var currentStateId = MegaId.Create("3dVhbTTT9j12");
            var fakeCurrentStateMetaclass = "itgClAZpU100";
            
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(applicationId, MetaClassLibrary.Application)
                    .WithProperty(MetaAttributeLibrary.CurrentState, currentStateId.ToString() ))
                .WithObject(new MockMegaObject(currentStateId, fakeCurrentStateMetaclass)
                    .WithProperty(MetaAttributeLibrary.Name, "Production"))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query{application{id,currentState{id,name}}}");

            resp.Should().MatchGraphQL("data.application[0].currentState.name", "Production");
        }

        [Fact(Skip = "Not working")]
        public async void Query_an_application_with_filter()
        {
            var applicationId = MegaId.Create("IubjeRlyFfT1");
            var currentStateId = MegaId.Create("3dVhbTTT9j12");

            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(applicationId, MetaClassLibrary.Application)
                    .WithProperty(MetaAttributeLibrary.CurrentState, currentStateId.ToString()))
                .WithObject(new MockMegaObject(currentStateId, MetaClassLibrary.StateUml)
                    .WithProperty(MetaAttributeLibrary.ShortName, "Production"))
                .Build();

            var resp = await ExecuteQueryAsync(root, $@"query{{application(filter:{{currentState_in:[""{currentStateId}""]}}){{id,currentState{{id,name}}}}}}");

            resp.Should().MatchGraphQL("data.application[0].currentState.name", "Production");
        }
        
        [Fact]
        public async void Query_aggregated_values_for_applications()
        {
            var applicationId1 = MegaId.Create("VdHRXqsQV500");
            var applicationId2 = MegaId.Create("NdHRQqsQV100");

            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(applicationId1, MetaClassLibrary.Application))
                .WithObject(new MockMegaObject(applicationId2, MetaClassLibrary.Application))
                .Build();

            var resp = await ExecuteQueryAsync(root, @"query applicationAggregatedValues{applicationAggregatedValues{id(function:COUNT)}}");

            resp.Should().MatchGraphQL("data.applicationAggregatedValues[0].id", "2");
        }

        [Fact]
        public async void Query_more_than_50_items_in_array_filter()
        {

            var root = new MockMegaRoot.Builder().Build();

            var query = "query{application(filter:{name_in:[";
            for(int idx = 0; idx <55; ++idx)
            {
                if(idx != 0)
                {
                    query += ",";
                }
                query += $"\"name{idx}\"";
            }
            query += "]}){id}}";
            var resp = await ExecuteQueryAsync(root, @query);

            resp.Should().MatchGraphQL("errors[0].message", "Number of items in array field [name_in] should not exceed 50");
        }

        [Fact]
        public async void Query_sub_filter_should_work()
        {
            var personSystem = new MockMegaObject(MegaId.Create("FWXuoPkqLzoP"), MetaClassLibrary.PersonSystem).
                WithProperty(MetaAttributeLibrary.Name, "Adam").
                WithProperty(MetaAttributeLibrary.Email, "webeval@mega.com");

            var businessRole = new MockMegaObject(MegaId.Create("WzF2lb0yGb2U"), MetaClassLibrary.BusinessRole).
                WithProperty(MetaAttributeLibrary.ShortName, "Local Application Owner");

            var responsibilityAssignment = new MockMegaObject(MegaId.Create("xWWBevorLnmD"), MetaClassLibrary.ResponsibilityAssignment).
               WithProperty("~M2000000Ce80", "WzF2lb0yGb2U"). //Visiblement on utilise un getProperty dans la condition des résolutions de paths, il le faut donc dans property
               WithRelation(new MockMegaCollection("~M2000000Ce80").WithChildren(businessRole)).
               WithRelation(new MockMegaCollection("~L2000000Ca80").WithChildren(personSystem));

            var application = new MockMegaObject(MegaId.Create("IubjeRlyFfT1"), MetaClassLibrary.Application).
               WithProperty(MetaAttributeLibrary.ShortName, "My Application").
               WithRelation(new MockMegaCollection(MetaAssociationEndLibrary.AssignmentObject_PersonAssignment).WithChildren(responsibilityAssignment));

            var spyRoot = new Mock<MockMegaRoot> { CallBase = true };
            spyRoot.Setup(x => x.GetSelection("SELECT ~T20000000s10[PersonSystem] WHERE ~Sy64inney0Y5[Email] Like \"#webeval#\" AND ~H20000008a80[Assignment]:~030000000240[ResponsibilityAssignment].(~M2000000Ce80[BusinessRole]:~230000000A40[BusinessRole].~310000000D00[Absolute Identifier] = \"~WzF2lb0yGb2U\" AND ~hCr81RIpEvMH[AssignedObject]:~MrUiM9B5iyM0[Application].(~310000000D00[Absolute Identifier] = \"IubjeRlyFfT1\"))", 1, null, 1, null, 1, null)).
                Returns(responsibilityAssignment.GetCollection("~L2000000Ca80"));

            var root = new MockMegaRoot.Builder(spyRoot)
                .WithObject(application)
                    .Build();

            var resp = await ExecuteQueryAsync(root, @"query{application{applicationOwner_PersonSystem(filter:{ email_contains: ""webeval""}) {id} }}");
            resp.Should().ContainsGraphQLCount("data.application[0].applicationOwner_PersonSystem", 1);
        }

        [Fact]
        public async void Query_with_link_property_should_work()
        {
            var businessCapability = new MockMegaObject(MegaId.Create("ofUZ9w8WOL(9"), MetaClassLibrary.BusinessCapability).
                WithProperty(MetaAttributeLibrary.ShortName, "Business Capability");

            var fulfillment = new MockMegaObject(MegaId.Create("SwR5lQHMPf36"), MetaClassLibrary.BusinessCapabilityFulfillment).
                WithProperty(MetaAttributeLibrary.ShortName, "Fulfillment").
                WithProperty(MetaAttributeLibrary.RealizationCostContributionKey, 10.0).
                WithRelation(new MockMegaCollection(MetaAssociationEndLibrary.BusinessCapabilityFulfillment_FulfilledBusinessCapability).
                    WithChildren(businessCapability));
                                
            var applicaiton = new MockMegaObject(MegaId.Create("Yl4vNYExH9U5"), MetaClassLibrary.Application).
                WithProperty(MetaAttributeLibrary.ShortName, "Application").
                WithRelation(new MockMegaCollection(MetaAssociationEndLibrary.ClassOfEnterpriseAgentExternalStructure_OwnedBusinessCapabilityFulfillment).
                    WithChildren(fulfillment));

            var root = new MockMegaRoot.Builder().WithObject(applicaiton).Build();

            var query = @"
                query {
                    application(filter:{id:""Yl4vNYExH9U5""}) {
                        id
                        name
                        businessCapability {
                            id
                            name
                            link1CostContributionKeyRealization
                        }
                    }
                }";
            var resp = await ExecuteQueryAsync(root, query);
            resp.Should().HaveNoGraphQLError();
        }

        [Fact]
        public async void Query_an_application_deployment_date_with_specific_format()
        {
            var applicationId = MegaId.Create("IubjeRlyFfT1");

            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject(applicationId, MetaClassLibrary.Application)
                    .WithProperty(MetaAttributeLibrary.DeploymentDate, new DateTime(2020, 12, 31) ))
                .Build();

            var query = @"
                query application
                {
                    application(filter:{id:""IubjeRlyFfT1""})
                    {
                        deploymentDate (timeOffset:""+02:00"") @date(format:""yyyy/MM/dd HH:mm:ss"")
                    }
                }";

            var resp = await ExecuteQueryAsync(root, query);

            resp.Should().MatchGraphQL("data.application[0].deploymentDate", "2020/12/31 02:00:00");
        }
    }
}
