using FluentAssertions;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.DTO;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Local deserialization")]
    public class CurrentContext_should : BaseFixture
    {

        private GraphQLHttpClient _metamodelClient;
        private GraphQLHttpClient _itpmClient;

        public CurrentContext_should(GlobalFixture fixture, ITestOutputHelper output) : base(fixture, output)
        { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _metamodelClient = await _fx.GetGraphQLClientAsync("Metamodel");
            _itpmClient = await _fx.GetGraphQLClientAsync("ITPM");
        }

        [Fact]
        public async Task Retrieve_various_global_properties()
        {
            var request = new GraphQLRequest()
            {
                Query = @"query { _currentContext {
                            userId
                            databaseId
                            libraryId
                            profileId
                            currencyCode
                            currencyId
                            language
                            languageName
                            languageId
                            databaseLanguage
                            databaseLanguageName
                            databaseLanguageId
                            systemLanguage
                            systemLanguageName
                            systemLanguageId
                            workingEnvironmentTemplate
                            workingEnvironmentGroupTemplate
                            workingEnvironmentTopicTemplate
                            workingEnvironment
                            workingEnvironmentGroup
                            workingEnvironmentTopic
                            workingEnvironmentEntryPoint
                          }}"
            };

            var response = await _metamodelClient.SendQueryAsync<CurrentContextResponse>(request);

            response.Should().HaveNoError();
            var context = response.Data._currentContext;
            context.Should().BeEquivalentTo(new CurrentContextResponse.CurrentContext()
            {
                UserId = "XgCp3syUNLAH",
                DatabaseId = _fx.RepositoryId,
                LibraryId = null,
                ProfileId = _fx.ProfileId,
                CurrencyCode = "EUR",
                CurrencyId = "D6I9paoDIfFQ",
                Language = "EN",
                LanguageName = "English",
                LanguageId = "00(6wlHmk400",
                DatabaseLanguage = "EN",
                DatabaseLanguageName = "English",
                DatabaseLanguageId = "00(6wlHmk400",
                SystemLanguage = "EN",
                SystemLanguageName = "English",
                SystemLanguageId = "00(6wlHmk400",
                WorkingEnvironmentTemplate = null,
                WorkingEnvironmentGroupTemplate = null,
                WorkingEnvironmentTopicTemplate = null,
                WorkingEnvironment = null,
                WorkingEnvironmentGroup = null,
                WorkingEnvironmentTopic = null,
                WorkingEnvironmentEntryPoint = null
            });
        }

        [Fact]
        public async Task Query_language_codes()
        {
            var response = await _metamodelClient.SendQueryAsync<TypeResponse>(@"{__type(name: ""Languages"") {name enumValues {name}}}");

            response.Should().HaveNoError();
            response.Data.__type.EnumValues.Select(ev => ev.Name).Should().Contain(new[] { "FR", "EN" });
        }

        [Fact]
        public async Task Query_available_languages()
        {
            var response = await _metamodelClient.SendQueryAsync<LanguageResponse>(@"query language {language(filter:{languageCode_not:""""}) { id name languageCode}}");

            response.Should().HaveNoError();
            var languages = response.Data.Language;
            languages.Select(l => l.LanguageCode).Should().Contain(new[] { "FR", "EN" });
            languages.Should().Contain(l => l.Name.Contains("Compatibility"));
        }

        [Fact]
        public async Task Query_neutral_sub_languages()
        {
            var response = await _metamodelClient.SendQueryAsync<LanguageResponse>(
                @"query allAvailableLanguages{
                    language(orderBy:[name_ASC] filter:{language_GeneralLanguage_some:{id:""I9o3by0knG00""}}){
                        id
                        languageCode
                        name
                    }}");

            response.Should().HaveNoError();
            var languages = response.Data.Language;
            languages.Select(l => l.LanguageCode).Should().Contain(new[] { "FR", "EN" });
            languages.Should().BeInAscendingOrder(l => l.Name);
            languages.Should().NotContain(l => l.Name.Contains("Compatibility"));
        }

        [Fact]
        public async Task Switch_data_languages()
        {
            await _metamodelClient.SendQueryAsync<LanguageResponse>(@"mutation {_updateCurrentContext(currentContext:{language:FR}) {language}}");

            var response = await _metamodelClient.SendQueryAsync<CurrentContextResponse>(@"query { _currentContext { language databaseLanguage systemLanguage } }");

            response.Should().HaveNoError();
            var context = response.Data._currentContext;
            context.Should().BeEquivalentTo(new CurrentContextResponse.CurrentContext()
            {
                Language = "FR",
                DatabaseLanguage = "EN",
                SystemLanguage = "EN",
            });
        }

        [Fact]
        public async Task Support_translated_names()
        {
            var createQuery = @" mutation {
                            createBusinessProcess(id: ""Support_translated_names"" idType: EXTERNAL businessProcess:{
                                name: ""Name in english""
                                comment: ""Comment in english""
                            }) { name comment }}";
            var createResponse = await _itpmClient.SendQueryAsync<CreateBusinessProcessResponse>(createQuery);
            createResponse.Should().HaveNoError();
            createResponse.Data.CreateBusinessProcess.Should().BeEquivalentTo(new BasicObject
            {
                Name = "Name in english",
                Comment = "Comment in english"
            });

            await _itpmClient.SendQueryAsync<LanguageResponse>(@"mutation {_updateCurrentContext(currentContext:{language:FR}) {language}}");
            var updateQuery = @" mutation {
                            updateBusinessProcess(id: ""Support_translated_names"" idType: EXTERNAL businessProcess:{
                                name: ""Nom en français""
                                comment: ""Commentaire en français""
                            }) {
                                name comment
                                nameEN: name(language: EN) commentEN: comment(language: EN)
                                nameFR: name(language: FR) commentFR: comment(language: FR)
                            }}";
            var updateResponse = await _itpmClient.SendQueryAsync<UpdateBusinessProcessResponse>(updateQuery);
            updateResponse.Should().HaveNoError();
            updateResponse.Data.UpdateBusinessProcess.Should().BeEquivalentTo(new TranslatedBasicObject
            {
                Name = "Nom en français",
                Comment = "Commentaire en français",
                NameFR = "Nom en français",
                CommentFR = "Commentaire en français",
                NameEN = "Name in english",
                CommentEN = "Comment in english"
            });
        }

        public override async Task DisposeAsync()
        {
            var contextResponse = await _metamodelClient.SendQueryAsync<LanguageResponse>(
                @"mutation {
                    _updateCurrentContext(currentContext:{language:EN}) {language}
                  }");
            contextResponse.Should().HaveNoError();

            var deleteResponse = await _itpmClient.SendQueryAsync<DeleteBusinessProcessResponse>(
                @"mutation {
                    deleteBusinessProcess(id: ""Support_translated_names"" idType: EXTERNAL) { deletedCount }
                  }");
            deleteResponse.Data.DeleteBusinessProcess?.DeletedCount.Should().BeGreaterOrEqualTo(0);

            await base.DisposeAsync();
        }

        public class TypeResponse
        {
            public Type __type { get; set; }

            public class Type
            {
                public string Name { get; set; }
                public List<BasicObject> EnumValues { get; set; }

            }
        }

        public class LanguageResponse
        {
            public List<Language> Language { get; set; }           
        }

        public class Language : BasicObject
        {
            public string LanguageCode { get; set; }

        }

        public class CreateBusinessProcessResponse
        {
            public BasicObject CreateBusinessProcess { get; set; }
        }

        public class UpdateBusinessProcessResponse
        {
            public TranslatedBasicObject UpdateBusinessProcess { get; set; }
        }

        public class DeleteBusinessProcessResponse
        {
            public DeleteResult DeleteBusinessProcess { get; set; }
        }

        public class TranslatedBasicObject : BasicObject
        {
            public string NameEN { get; set; }
            public string CommentEN { get; set; }
            public string NameFR { get; set; }
            public string CommentFR { get; set; }
        }
    }
}
