using FluentAssertions;
using GraphQL;
using Mega.WebService.GraphQL.IntegrationTests.Assertions;
using Mega.WebService.GraphQL.IntegrationTests.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests
{
    
    public class AttachmentEndPoint_should : BaseFixture
    {
        public AttachmentEndPoint_should(GlobalFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        { }

        public static IEnumerable<object[]> DocumentTypes =>
            new List<object[]> {
                new object[] { DocumentType.Data },
                new object[] { DocumentType.System }
            };

        [Theory]
        [MemberData(nameof(DocumentTypes))]
        public async Task<BusinessDocument> Create_a_business_document(DocumentType type)
        {            
            var request = new GraphQLRequest()
            {
                Query = string.Format(@"mutation {{
                                        {1}({0}:{{ name:""My Document""}})
                                        {{ id, name, downloadUrl, uploadUrl }}
                                      }}", type.GraphQLType, type.Mutation)
            };
            var graphQLClient = await _fx.GetGraphQLClientAsync(type.Schema);

            var response = await graphQLClient.SendQueryAsync<CreateBusinessDocumentResponse>(request);

            response.Should().HaveNoError();
            var document = type.ExtractCreated(response.Data);
            document.Id.Should().NotBeNullOrWhiteSpace();
            document.Name.Should().Be("My Document");
            return document;
        }
        
        [Theory]
        [MemberData(nameof(DocumentTypes))]
        public async void Upload_a_business_document(DocumentType type)
        {
            var document = await Create_a_business_document(type);

            await UploadFileContentAsync(document);
        }

        private async Task<HttpResponseMessage> UploadFileContentAsync(
            BusinessDocument document,
            string documentVersion = "Replace",
            string content = "my file content")
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"attachment/{document.Id}/file", UriKind.Relative),
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes(content))
            };
            await _fx.FillHeadersAsync(request.Headers);
            request.Headers.Add("X-Hopex-Filename", "myfile.txt");
            request.Headers.Add("X-Hopex-DocumentVersion", documentVersion);

            var response = await _fx.Client.SendAsync(request);

            await response.Should().BeOkUploadAsync(document);
            return response;
        }

        [Theory]
        [MemberData(nameof(DocumentTypes))]
        public async void Download_a_business_document(DocumentType type)
        {
            var document = await Create_a_business_document(type);
            await UploadFileContentAsync(document);

            var content = await DownloadFileContent(document.DownloadUrl);

            await content.Should().BeFileAsync("My Document v1.txt", "my file content");
        }

        private async Task<HttpContent> DownloadFileContent(string downloadUrl)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(downloadUrl)
            };
            await _fx.FillHeadersAsync(request.Headers);

            var response = await _fx.Client.SendAsync(request);

            var content = response.Content;
            return content;
        }

        [Theory]
        [MemberData(nameof(DocumentTypes))]
        public async void Add_a_new_business_document_version(DocumentType type)
        {
            var document = await Create_a_business_document(type);
            await UploadFileContentAsync(document, "Replace", "version 1");

            await UploadFileContentAsync(document, "New", "version 2");

            document = await GetDocument(type, document.Id);
            var versions = type.ExtractVersion(document);
            versions.Should().HaveCount(2);

            var content = await DownloadFileContent(versions[0].DownloadUrl);
            await content.Should().BeFileAsync("My Document v1.txt", "version 1");

            content = await DownloadFileContent(versions[1].DownloadUrl);
            await content.Should().BeFileAsync("My Document v2.txt", "version 2");
        }

        private async Task<BusinessDocument> GetDocument(DocumentType type, string documentId)
        {
            var request = new GraphQLRequest()
            {
                Query = string.Format(@"query {{
                                          {1}(filter:{{id: ""{0}""}}) {{
                                            id
                                            name
                                            downloadUrl
                                            uploadUrl
                                            {2} {{
                                              id
                                              downloadUrl
                                            }}
                                          }}
                                        }}", documentId, type.GraphQLType, type.VersionRelation)
            };
            var graphQLClient = await _fx.GetGraphQLClientAsync(type.Schema);
            var response = await graphQLClient.SendQueryAsync<BusinessDocumentListResponse>(request);
            response.Should().HaveNoError();

            return type.ExtractListed(response.Data)[0];
        }
    }   

    public class CreateBusinessDocumentResponse
    {
        public BusinessDocument CreateBusinessDocument { get; set; }
        public BusinessDocument CreateSystemBusinessDocument { get; set; }
    }

    public class BusinessDocumentListResponse
    {
        public BusinessDocument[] BusinessDocument { get; set; }
        public BusinessDocument[] SystemBusinessDocument { get; set; }
    }

    public class BusinessDocument
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DownloadUrl { get; set; }
        public string UploadUrl { get; set; }
        public BusinessDocumentVersion[] BusinessDocumentVersion_DocumentVersions { get; set; }
        public BusinessDocumentVersion[] SystemBusinessDocumentVersion_SystemDocumentVersions { get; set; }
    }

    public class BusinessDocumentVersion
    {
        public string Id { get; set; }
        public string DownloadUrl { get; set; }        
    }

    public class BusinessDocumentUploadResponse
    {
        public string documentId { get; set; }
        public bool success { get; set; }
    }

    public class DocumentType : IXunitSerializable
    {
        public string GraphQLType { get; private set; }
        public string Schema { get; private set; }
        public string Mutation { get; private set; }
        public Func<CreateBusinessDocumentResponse, BusinessDocument> ExtractCreated { get; private set; }
        public string VersionRelation { get; private set; }
        public Func<BusinessDocumentListResponse, BusinessDocument[]> ExtractListed { get; private set; }
        public Func<BusinessDocument, BusinessDocumentVersion[]> ExtractVersion { get; private set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            GraphQLType = info.GetValue<string>("type");
            For(GraphQLType);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("type", GraphQLType);
        }

        public override string ToString() => GraphQLType;

        public DocumentType For(string graphQLType)
        {
            GraphQLType = graphQLType;
            switch (GraphQLType)
            {
                case "businessDocument":
                    Schema = "ITPM";
                    Mutation = "createBusinessDocument";
                    ExtractCreated = r => r.CreateBusinessDocument;
                    VersionRelation = "businessDocumentVersion_DocumentVersions";
                    ExtractListed = r => r.BusinessDocument;
                    ExtractVersion = r => r.BusinessDocumentVersion_DocumentVersions;
                    break;
                case "systemBusinessDocument":
                    Schema = "MetaModel";
                    Mutation = "createSystemBusinessDocument";
                    ExtractCreated = r => r.CreateSystemBusinessDocument;
                    VersionRelation = "systemBusinessDocumentVersion_SystemDocumentVersions";
                    ExtractListed = r => r.SystemBusinessDocument;
                    ExtractVersion = r => r.SystemBusinessDocumentVersion_SystemDocumentVersions;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return this;
        }

        public static DocumentType Data = new DocumentType().For("businessDocument");
        public static DocumentType System = new DocumentType().For("systemBusinessDocument");
    }
}
