using Hopex.ApplicationServer.WebServices;
using Hopex.Common.JsonMessages;
using Hopex.Model.Mocks;
using Mega.Macro.API.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL
{
    [HopexWebService(WebServiceRoute)]
    [HopexMacro(MacroId = "AAC8AB1E5D25678E")]
    public class DiagramEntryPoint : BaseSatelliteEntryPoint<DiagramExportArguments>
    {
        private const string WebServiceRoute = "diagram";
        protected override string PathPrefix => $"/api/{WebServiceRoute}/";

        public DiagramEntryPoint()
                : this(new PhysicalFileSource()) { }

        public DiagramEntryPoint(IFileSource fileSource)
        {
            this.fileSource = fileSource;
        }

        protected override Task<HopexResponse> ExecuteOnObject(IMegaRoot root, string diagramId, string method, DiagramExportArguments args)
        {
            var diagram = root.GetObjectFromId(diagramId);
            var drawing = root.GetDrawingFactory().CreateFromDiagram(diagram, "RO");
            if (drawing == null)
            {
                var result = new ErrorMacroResponse(HttpStatusCode.InternalServerError, "Diagram cannot be opened. Check you have sufficient rights or contact your administrator.");
                return Task.FromResult(HopexResponse.Json(JsonConvert.SerializeObject(result)));
            }

            var fileName = drawing.Name + "." + args.Format.ToString().ToLower();
            var tempFileName = Path.GetTempFileName();
            var _ = 0;
            drawing.SaveAsPicture(tempFileName, _argFormatToMega[args.Format], 0, args.Quality, 0, 0, 0, ref _, ref _, null);

            return Task.FromResult(BuildTempFileResult(fileName, tempFileName));
        }

        private readonly Dictionary<ImageFormat, MegaFilePictureFormat> _argFormatToMega = new Dictionary<ImageFormat, MegaFilePictureFormat>()
        {
            { ImageFormat.Png, MegaFilePictureFormat.Png },
            { ImageFormat.Jpeg, MegaFilePictureFormat.Jpg },
            { ImageFormat.Svg, MegaFilePictureFormat.Svg }
        };
    }
}
