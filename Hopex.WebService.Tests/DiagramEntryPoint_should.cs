using FluentAssertions;
using Hopex.ApplicationServer.WebServices;
using Hopex.Common.JsonMessages;
using Hopex.Model.Mocks;
using Hopex.Modules.GraphQL;
using Hopex.WebService.Tests.Assertions;
using Hopex.WebService.Tests.Mocks;
using Hopex.WebService.Tests.Mocks.Drawings;
using Mega.Macro.API.Enums;
using Moq;
using Xunit;

namespace Hopex.WebService.Tests
{
    public class DiagramEntryPoint_should
    {
        readonly Mock<MockMegaDrawing> spyDrawing = new Mock<MockMegaDrawing>(null, "RO");
        readonly Mock<IFileSource> spyFileSource = new Mock<IFileSource>();
        readonly SaveAsPictureFileNameCapture fileName = new SaveAsPictureFileNameCapture();
        int _ = 0;

        public DiagramEntryPoint_should()
        {
            spyDrawing.Setup(d => d.Name).Returns("Library diagram");
        }

        [Theory]
        [InlineData(ImageFormat.Png, MegaFilePictureFormat.Png, "image/png", "png")]
        [InlineData(ImageFormat.Jpeg, MegaFilePictureFormat.Jpg, "image/jpeg", "jpeg")]
        [InlineData(ImageFormat.Svg, MegaFilePictureFormat.Svg, "image/svg+xml", "svg")]
        public async void Returns_a_diagram_bitmap_in_several_format(ImageFormat format, MegaFilePictureFormat expectedMegaFormat, string expectedMimeType, string expectedExtension)
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("BGZ4uPrgG(Up")
                    .WithDrawing(spyDrawing.Object))
                .Build();
            spyDrawing
                .Setup(d => d.SaveAsPicture(It.IsAny<string>(), expectedMegaFormat, 0, 0, 0, 0, 0, ref _, ref _, null))
                .Callback(fileName.Capture())
                .Verifiable();
            spyFileSource.Setup(f => f.ReadAllBytes(fileName.Matcher())).Returns(new byte[] { 1, 2, 3, 4 });
            var entryPoint = new TestableDiagramEntryPoint(root, "image", spyFileSource.Object);

            var actual = await entryPoint.Execute(new DiagramExportArguments { Format = format });

            spyDrawing.Verify();
            spyFileSource.Verify(f => f.Delete(fileName.Matcher()), Times.Once);
            actual.Should().BeJson($@"{{""fileName"":""Library diagram.{expectedExtension}"", ""contentType"":""{expectedMimeType}"", ""content"":""AQIDBA==""}}");
        }

        [Fact]
        public async void Change_Jpeg_quality()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("BGZ4uPrgG(Up")
                    .WithDrawing(spyDrawing.Object))
                .Build();
            spyDrawing
                .Setup(d => d.SaveAsPicture(It.IsAny<string>(), MegaFilePictureFormat.Jpg, 0, 129, 0, 0, 0, ref _, ref _, null))
                .Callback(fileName.Capture())
                .Verifiable();
            spyFileSource.Setup(f => f.ReadAllBytes(fileName.Matcher())).Returns(new byte[] { 1, 2, 3, 4 });
            var entryPoint = new TestableDiagramEntryPoint(root, "image", spyFileSource.Object);

            var actual = await entryPoint.Execute(new DiagramExportArguments { Format = ImageFormat.Jpeg, Quality = 129 });

            spyDrawing.Verify();            
        }

        [Fact]        
        public async void Return_clean_error_when_diagram_cannot_be_opened()
        {
            var root = new MockMegaRoot.Builder()
                .WithObject(new MockMegaObject("BGZ4uPrgG(Up"))
                .Build();
            var entryPoint = new TestableDiagramEntryPoint(root, "image");

            var actual = await entryPoint.Execute(new DiagramExportArguments { Format = ImageFormat.Png });

            actual.Should().BeError(500);
        }
    }

    public class TestableDiagramEntryPoint : DiagramEntryPoint
    {
        public TestableDiagramEntryPoint(IMegaRoot root, string action, IFileSource fileSource = null)
            : base(fileSource)
        {
            var _request = new FakeDiagramRequest(action);
            (this as IHopexWebService).SetHopexContext(root, _request, new Logger());
        }

        protected override IMegaRoot GetRoot()
        {
            return (IMegaRoot)HopexContext.NativeRoot;
        }
    }

    internal class FakeDiagramRequest : BaseMockRequest
    {
        readonly string _action;

        internal FakeDiagramRequest(string action)
        {
            _action = action;
        }
        public override string Path => $"/api/diagram/BGZ4uPrgG(Up/{_action}";
    }

    delegate void SaveAsPictureCallback(string fileName, MegaFilePictureFormat filePictureFormat, int bitsPerPixel, int quality, int resolution, int height, int width, ref int imageHeight, ref int imageWidth, object vDispatch);

    class SaveAsPictureFileNameCapture
    {
        string fileName;
        internal SaveAsPictureCallback Capture()
        {
            return new SaveAsPictureCallback((string fn, MegaFilePictureFormat _, int _2, int _3, int _4, int _5, int _6, ref int _7, ref int _8, object _9) =>
            {
                fileName = fn;
            });
        }

        internal string Matcher()
        {
            return Match.Create<string>(s =>
            {
                return s == fileName;
            });
        }
    }
}
