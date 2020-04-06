using FluentAssertions;
using FluentAssertions.Json;
using Hopex.ApplicationServer.WebServices;
using Hopex.Common.JsonMessages;
using Hopex.Model.Abstractions;
using Hopex.Modules.GraphQL;
using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Mega.Macro.API;
using Moq;
using System.Text;
using Xunit;
using static Hopex.WebService.Tests.Assertions.MegaIdMatchers;

namespace Hopex.WebService.Tests
{
    public class AttachmentEntryPoint_should
    {        
        const string DOCUMENT_ID = ")D2nnHysT5lE";
        const string SERVER_FILE_EXTENSION = "txt";
        static readonly string SERVER_FILE_NAME = $"foo.{SERVER_FILE_EXTENSION}";
        static readonly string SERVER_FILE_PATH = $@"c:\temp\{SERVER_FILE_NAME}";
        const string UPLOAD_SESSION_ID = "{81AF2718-13AF-4936-8C09-6754B77359B5}";
        readonly Mock<MockMegaRoot> spyRoot = new Mock<MockMegaRoot>() { CallBase = true };
        readonly Mock<IFileSource> spyFileSource = new Mock<IFileSource>();

        public AttachmentEntryPoint_should()
        {
            spyRoot
                .Setup(doc => doc.CallFunctionString(IsId("~jtdsTU(tTLT0[GetUploadedFile]"), UPLOAD_SESSION_ID, null, null, null, null, null))
                .Returns(SERVER_FILE_PATH);

            spyRoot
                .Setup(root => root.CallFunctionString(IsId("~lcE6jbH9G5cK[PublishStayInSessionWizard Command Launcher]"), "{\"instruction\":\"PUBLISHINSESSION\"}", null, null, null, null, null))
                .Returns("SESSION_PUBLISH")
                .Verifiable();
        }

        [Theory]
        [InlineData("~UkPT)TNyFDK5[Business Document]", "~67CucVkAJ1xA[Last Version of the Business Document]")]
        [InlineData("~aMRn)bUIGjX3[System Business Document]", "~U7CuchkAJ9iB[Last Version of the System Business Document]")]
        public async void Save_business_document_in_last_version(string documentClassId, string expectedCalledQuery)
        {
            var spyVersion = new Mock<MockMegaObject>("000000000001") { CallBase = true };
            var mockRoot = new MockMegaRoot.Builder(spyRoot)
                .WithObject(new MockMegaObject(DOCUMENT_ID, documentClassId)
                    .WithRelation(new MockMegaCollection(expectedCalledQuery)
                        .WithChildren(spyVersion.Object)))
                .Build();
            spyVersion.Setup(doc => doc.CallFunctionValue<bool>(IsId("~s5G0lNm(FXiR[BusinessDocumentSave]"), SERVER_FILE_PATH, false, null, null, null, null)).Returns(true).Verifiable();
            var entryPoint = new TestableAttachmentEntryPoint(mockRoot, "uploadFile", spyFileSource.Object);

            var actual = await entryPoint.Execute(new AttachmentArguments {
                IdUploadSession = UPLOAD_SESSION_ID,
                UpdateMode = UpdateMode.Replace
            });

            spyVersion.Verify();
            spyVersion.Object.GetPropertyValue("~Ekh4qloyFrmK[File Extension]").Should().Be(SERVER_FILE_EXTENSION);
            spyRoot.Verify();
            spyFileSource.Verify(fs => fs.Delete(SERVER_FILE_PATH), Times.Once);
            actual.Should().BeJson($@"{{""documentId"":""{DOCUMENT_ID}"", ""success"":true}}");            
        }


        [Theory]
        [InlineData("~UkPT)TNyFDK5[Business Document]", "~ibEuqCE8GL(D[Document Versions]")]
        [InlineData("~aMRn)bUIGjX3[System Business Document]", "~vMRnR4VIGf84[System Document Versions]")]
        public async void Save_business_document_as_new_version(string documentClassId, string expectedCalledQuery)
        {
            var spyVersions = new Mock<MockMegaCollection>(MegaId.Create(expectedCalledQuery));
            var mockRoot = new MockMegaRoot.Builder(spyRoot)
                .WithObject(new MockMegaObject(DOCUMENT_ID, documentClassId)
                    .WithRelation(spyVersions.Object))
                .Build();

            var spyWizard = new Mock<IMegaWizardContext>(MockBehavior.Strict);
            spyVersions.Setup(v => v.CallFunction<IMegaWizardContext>(IsId("~GuX91iYt3z70[InstanceCreator]"), null, null, null, null, null, null)).Returns(spyWizard.Object);
            var seq = new MockSequence();
            spyWizard.InSequence(seq).Setup(w => w.InvokePropertyPut(IsIdString("~vjh4n6oyFTKK[File Location]"), SERVER_FILE_PATH));
            spyWizard.InSequence(seq).Setup(w => w.InvokeFunction<double>("Create")).Returns(1);

            var entryPoint = new TestableAttachmentEntryPoint(mockRoot, "uploadFile", spyFileSource.Object);

            var actual = await entryPoint.Execute(new AttachmentArguments
            {
                IdUploadSession = UPLOAD_SESSION_ID,
                UpdateMode = UpdateMode.New
            });
           
            spyRoot.Verify();
            spyFileSource.Verify(fs => fs.Delete(SERVER_FILE_PATH), Times.Once);
            actual.Should().BeJson($@"{{""documentId"":""{DOCUMENT_ID}"", ""success"":true}}");
        }

        [Fact]
        public async void Download_business_document()
        {
            var spyDocument = new Mock<MockMegaObject>(MegaId.Create(DOCUMENT_ID), null);
            spyDocument.Setup(doc => doc.CallFunctionString(IsId("~FFopcJjTGnIC[StaticDocumentFilePathGet]"), null, null, null, null, null, null)).Returns(SERVER_FILE_PATH);
            var mockRoot = new MockMegaRoot.Builder().WithObject(spyDocument.Object).Build();
            spyFileSource.Setup(fs => fs.ReadAllBytes(SERVER_FILE_PATH)).Returns(ToBytes("abcd"));
            var entryPoint = new TestableAttachmentEntryPoint(mockRoot, "downloadFile", spyFileSource.Object);

            var actual = await entryPoint.Execute(null);

            spyFileSource.Verify(fs => fs.Delete(SERVER_FILE_PATH), Times.Once);
            actual.Should().BeJson($@"{{""fileName"":""{SERVER_FILE_NAME}"", ""contentType"":""text/plain"", ""content"":""YWJjZA==""}}");            
        }

        private byte[] ToBytes(string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }        
    }         

    public class TestableAttachmentEntryPoint : AttachmentEntryPoint
    {
        public TestableAttachmentEntryPoint(IMegaRoot root, string action, IFileSource fileSource = null)
            : base(fileSource)
        {
            var _request = new FakeAttachmentRequest(action);
            (this as IHopexWebService).SetHopexContext(root, _request, new Logger());
        }

        protected override IMegaRoot GetRoot()
        {
            return (IMegaRoot)HopexContext.NativeRoot;
        }
    }

    internal class FakeAttachmentRequest : BaseMockRequest
    {
        readonly string _action;

        internal FakeAttachmentRequest(string action)
        {
            _action = action;
        }
        public override string Path => $"/api/attachment/)D2nnHysT5lE/{_action}";
    }
}
