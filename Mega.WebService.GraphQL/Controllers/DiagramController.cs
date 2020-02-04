using Hopex.Common.JsonMessages;
using Mega.WebService.GraphQL.Models;
using Mega.WebService.GraphQL.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Mega.WebService.GraphQL.Controllers
{
    [RoutePrefix("api/diagram")]
    public class DiagramController : BaseController
    {
        [HttpGet]
        [Route("{diagramId}/image")]
        public IHttpActionResult GetImage(string diagramId)
        {
            ImageFormat? format = GetImageFormatFromHeader();
            if (!format.HasValue)
                return StatusCode(HttpStatusCode.NotAcceptable);

            var diagramArgs = new DiagramExportArguments { Format = format.Value };
            if (! TryParseQualityHeader(ref diagramArgs))
                return BadRequest("Invalid X-Hopex-JpegQuality, should a number between 1 and 100");            

            var data = new CallMacroArguments<DiagramExportArguments>($"/api/diagram/{diagramId}/image", diagramArgs);            

            var result = CallMacro(GraphQlMacro, data.ToString());
            return ProcessMacroResult(result, () =>
            {
                return ResponseMessage(FileDownloadHttpResponse.From(result));
            });
        }

        private ImageFormat? GetImageFormatFromHeader()
        {
            var negociator = new DefaultContentNegotiator(true);
            var result = negociator.Negotiate(typeof(object), Request, _diagramFormatters);
            if (result == null) return null;
            return ((DiagramFormatter)result.Formatter).MegaFormat;            
        }

        static readonly MediaTypeFormatter[] _diagramFormatters = new MediaTypeFormatter[]
        {
            new PngDiagramFormater(),
            new JpegDiagramFormater(),
            new SvgDiagramFormater()
        };

        abstract class DiagramFormatter : MediaTypeFormatter
        {
            public abstract ImageFormat MegaFormat { get; }
            public override bool CanReadType(Type type) => true;
            public override bool CanWriteType(Type type) => true;
        }

        class PngDiagramFormater : DiagramFormatter
        {
            internal PngDiagramFormater()
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/png"));
            }

            public override ImageFormat MegaFormat => ImageFormat.Png;
        }

        class JpegDiagramFormater : DiagramFormatter
        {
            internal JpegDiagramFormater()
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/jpeg"));
            }

            public override ImageFormat MegaFormat => ImageFormat.Jpeg;
        }

        class SvgDiagramFormater : DiagramFormatter
        {
            internal SvgDiagramFormater()
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/svg+xml"));
            }

            public override ImageFormat MegaFormat => ImageFormat.Svg;
        }

        private bool TryParseQualityHeader(ref DiagramExportArguments diagramArgs)
        {
            if (diagramArgs.Format == ImageFormat.Jpeg)
            {
                if (Request.Headers.TryGetValues("X-Hopex-JpegQuality", out IEnumerable<string> values))
                {
                    int parsedQuality;
                    if (int.TryParse(values.First(), out parsedQuality) && parsedQuality >= 1 && parsedQuality <= 100 )
                    {
                        diagramArgs.Quality = (int)Math.Round(255f + 254f * (1 - parsedQuality ) / 99f);
                        return true;
                    }
                    return false;
                }                
            }
            return true;
        }
    }
}
