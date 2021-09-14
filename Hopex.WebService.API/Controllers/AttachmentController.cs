using Hopex.Common.JsonMessages;
using Mega.Has.Commons;
using Mega.Has.WebSite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HAS.Modules.WebService.API.Controllers
{
    [Route("api/attachment")]
    [RequestSizeLimit(100000000)]
    [Authorize(AuthenticationSchemes = "PublicApiKeyScheme, BasicAuthScheme, Bearer, Cookies")]
    public class AttachmentController : BaseController
    {
        private const string UploadMacro = "CD31CDAB4F865BEB";

        private readonly IHASClient _hopex;
        private readonly ILogger<HopexSessionController> _logger;

        public AttachmentController(IHASClient hopex, ILogger<HopexSessionController> logger)
        {
            _hopex = hopex;
            _logger = logger;
        }

        [HttpPost("{documentId}/file")]
        public async Task<IActionResult> UploadFile(string documentId)
        {
            try
            {
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.UploadFile(documentId: {documentId}) enter.");

                Request.EnableBuffering();

                string fileName;
                UpdateMode updateMode;
                try
                {
                    fileName = ParseMandatoryHeaderString("x-hopex-filename");
                    updateMode = ParseMandatoryHeaderEnum<UpdateMode>("x-Hopex-documentversion");
                }
                catch (ArgumentException e)
                {
                    return BadRequest(e.Message);
                }

                var idUploadSession = Guid.NewGuid().ToString();

                var fileStream = Request.Body;
                fileStream.Position = 0;
                var bytesToRead = Request.ContentLength;
                if (bytesToRead != null)
                {
                    var userDataPrefix = GetPacketPrefix(idUploadSession, fileName, bytesToRead.Value);

                    const int bufferSize = 1024 * 1024;

                    var userData = new StringBuilder(2 * userDataPrefix.Length + 2 * bufferSize);
                    var greedyStream = new GreedyStreamReader(fileStream);
                    var buffer = new byte[bufferSize];
                    long posInStream = 0;
                    do
                    {
                        userData.Clear();
                        userData.Append(userDataPrefix);
                        userData.Append(",\"iPosInStream\":");
                        userData.Append(posInStream);
                        userData.Append(",\"hexBlob\":\"");

                        var bytesRead = await greedyStream.ReadAsync(buffer, bufferSize);
                        AppendHexString(userData, buffer.Take(bytesRead));

                        userData.Append("\",\"lnRead\":");
                        userData.Append(bytesRead);
                        userData.Append("}");
                        posInStream += bytesRead;

                        var response = await _hopex.CallMacro(UploadMacro, userData.ToString());

                        if (response == null)
                        {
                            return StatusCode(500);
                        }
                    }
                    while (posInStream < bytesToRead);
                }

                _logger.Log(LogLevel.Trace, $"{GetType().Name}.UploadFile(documentId: {documentId}) leave.");

                return await CreateBusinessDocument(documentId, updateMode, idUploadSession);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("{documentId}/file")]
        public async Task<IActionResult> DownloadFile(string documentId)
        {
            try
            {
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.DownloadFile(documentId: {documentId}) enter.");
                var path = $"/api/attachment/{documentId}/downloadfile";
                var macroResponse = await _hopex.CallWebService<string>(path);
                var macroResult = JsonConvert.DeserializeObject<FileDownloadMacroResponse>(macroResponse);
                var fileResult = File(new MemoryStream(Convert.FromBase64String(macroResult.Content)), macroResult.ContentType, macroResult.FileName);
                _logger.Log(LogLevel.Trace, $"{GetType().Name}.DownloadFile(documentId: {documentId}) leave.");
                return fileResult;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        private string ParseMandatoryHeaderString(string headerName)
        {
            var headers = Request.Headers;
            if (!headers.ContainsKey(headerName))
            {
                throw new ArgumentException($"Missing {headerName} header");
            }
            headers.TryGetValue(headerName, out var headerValue);
            return headerValue;
        }

        private T ParseMandatoryHeaderEnum<T>(string headerName) where T : struct
        {
            var valueString = ParseMandatoryHeaderString(headerName);
            if (!Enum.TryParse(valueString, true, out T parsedUpdateMode))
            {
                throw new ArgumentException($"Invalid {headerName} value ; should be " + string.Join(" or ", Enum.GetNames(typeof(T))));
            }
            return parsedUpdateMode;
        }

        private static string GetPacketPrefix(string idUploadSession, string fileName, long fileLength)
        {
            var packetPrefix = new StringBuilder(1024);
            packetPrefix.Append("{\"clientFileName\":\"");
            packetPrefix.Append(fileName);
            packetPrefix.Append("\",\"idUploadSession\":\"");
            packetPrefix.Append(idUploadSession);
            packetPrefix.Append("\",\"lnStream\":");
            packetPrefix.Append(fileLength);
            return packetPrefix.ToString();
        }

        private static void AppendHexString(StringBuilder hex, IEnumerable<byte> raw)
        {
            const string hexes = "0123456789ABCDEF";
            foreach (var b in raw)
            {
                hex.Append(hexes[(b & 0xF0) >> 4])
                    .Append(hexes[(b & 0x0F)]);
            }
        }

        private class GreedyStreamReader
        {
            private readonly Stream _stream;

            public GreedyStreamReader(Stream stream)
            {
                _stream = stream;
            }

            public async Task<int> ReadAsync(byte[] buffer, int count)
            {
                var totalBytesRead = 0;
                int bytesRead;
                do
                {
                    bytesRead = await _stream.ReadAsync(buffer, totalBytesRead, count - totalBytesRead);
                    totalBytesRead += bytesRead;
                }
                while (totalBytesRead < count && bytesRead > 0);
                return totalBytesRead;
            }
        }

        private async Task<IActionResult> CreateBusinessDocument(string documentId, UpdateMode updateMode, string idUploadSession)
        {
            _logger.Log(LogLevel.Trace, $"{GetType().Name}.CreateBusinessDocument(documentId: {documentId}, updateMode: {updateMode}) enter.");
            var path = $"/api/attachment/{documentId}/uploadfile";
            var attachmentArguments = new AttachmentArguments
            {
                IdUploadSession = idUploadSession,
                UpdateMode = updateMode
            };
            var userData = JsonConvert.SerializeObject(attachmentArguments);
            var macroResponse = await _hopex.CallWebService<string>(path, userData);
            var contentResult = new ContentResult { Content = macroResponse, ContentType = "application/json", StatusCode = 200 };
            _logger.Log(LogLevel.Trace, $"{GetType().Name}.CreateBusinessDocument(documentId: {documentId}, updateMode: {updateMode}) leave.");
            return contentResult;
        }
    }
}
