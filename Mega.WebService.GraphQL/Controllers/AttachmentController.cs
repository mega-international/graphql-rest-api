using Hopex.Common.JsonMessages;
using Mega.WebService.GraphQL.Models;
using Mega.WebService.GraphQL.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Mega.WebService.GraphQL.Controllers
{
    [RoutePrefix("api/attachment")]
    public class AttachmentController : BaseController
    {
        private const string UploadMacro = "CD31CDAB4F865BEB";
        
        [HttpPost]
        [Route("{documentId}/file")]
        public async Task<IHttpActionResult> UploadFile(string documentId)
        {
            string fileName;
            UpdateMode updateMode;
            try
            {
                fileName = ParseMandatoryHeaderString("X-Hopex-Filename");
                updateMode = ParseMandatoryHeaderEnum<UpdateMode>("X-Hopex-DocumentVersion");
            }
            catch (ArgumentException e)
            {
                Logger.Error(e.Message, e);
                return BadRequest(e.Message);
            }

            var idUploadSession = Guid.NewGuid().ToString();
            var isSessionCreated = false;

            //if hopex.core >= 900_006
            try
            {
                var createSessionResponse = CallMacro(UploadMacro, "{\"cmd\":\"CreateSession\"}");
                var createSessionResponseJson = createSessionResponse.Content;
                var createSessionResult = JsonConvert.DeserializeObject<UploadCreateSessionResult>(createSessionResponseJson);
                if (createSessionResult != null)
                {
                    if (createSessionResult.Success && !string.IsNullOrWhiteSpace(createSessionResult.IdUploadSession))
                    {
                        idUploadSession = createSessionResult.IdUploadSession;
                        isSessionCreated = true;
                    }
                    else
                    {
                        if (createSessionResult.MaxUploadsReached)
                        {
                            Logger.Error("UploadFile: MaxUploadsReached");
                            return BadRequest("UploadFile: MaxUploadsReached");
                        }
                        Logger.Info("UploadFile: UnknownError (CreateSession API might not be available in this HOPEX version)");
                    }
                }
            }
            catch (Exception)
            {
                Logger.Info("UploadFile: UnknownError (CreateSession API might not be available in this HOPEX version)");
            }

            var content = GetRequestBufferlessStream();

            var sendResult = await SendFileToHopexAsync(idUploadSession, fileName, content);

            var createBusinessDocumentResult = ProcessMacroResult(sendResult, () => CreateBusinessDocument(documentId, updateMode, idUploadSession));

            if (isSessionCreated)
            {
                var uploadDeleteSessionResponse = CallMacro(UploadMacro, $"{{\"cmd\":\"DeleteSession\",\"idUploadSession\":\"{idUploadSession}\"}}");
                var uploadDeleteSessionResult = JsonConvert.DeserializeObject<UploadDeleteSessionResult>(uploadDeleteSessionResponse.Content);
                if (uploadDeleteSessionResult != null && uploadDeleteSessionResult.Success != true)
                {
                    Logger.Warn($"Session: {idUploadSession} hasn't been discarded");
                }
                Logger.Info($"Session: {idUploadSession} has been discarded");
            }

            return createBusinessDocumentResult;
        }

        private string ParseMandatoryHeaderString(string headerName)
        {
            var headers = Request.Headers;
            if (!headers.Contains(headerName))
                throw new ArgumentException($"Missing {headerName} header");
            return headers.GetValues(headerName).First();            
        }

        private T ParseMandatoryHeaderEnum<T>(string headerName) where T : struct
        {
            var valueString = ParseMandatoryHeaderString(headerName);
            if (! Enum.TryParse(valueString, true, out T parsedUpdateMode))
                throw new ArgumentException($"Invalid {headerName} value ; should be " + string.Join(" or ", Enum.GetNames(typeof(T))));
            return parsedUpdateMode;            
        }

        protected virtual Stream GetRequestBufferlessStream()
        {
            return HttpContext.Current.Request.GetBufferlessInputStream(true);
        }

        private async Task<WebServiceResult> SendFileToHopexAsync(string idUploadSession, string fileName, Stream fileStream)
        {
            var bytesToRead = fileStream.Length;
            var userDataPrefix = GetPacketPrefix(idUploadSession, fileName, bytesToRead);

            const int BUFFER_SIZE = 1024 * 1024; // 1 Mo
            StringBuilder userData = new StringBuilder(2 * userDataPrefix.Length + 2 * BUFFER_SIZE); // metadata + 1Mo of hexa encoded data

            var greedyStream = new GreedyStreamReader(fileStream);
            WebServiceResult packetResult;
            var buffer = new byte[BUFFER_SIZE];
            long posInStream = 0;
            do
            {
                userData.Clear();
                userData.Append(userDataPrefix);
                userData.Append(",\"iPosInStream\":");
                userData.Append(posInStream);
                userData.Append(",\"hexBlob\":\"");

                var bytesRead = await greedyStream.ReadAsync(buffer, BUFFER_SIZE);
                AppendHexString(userData, buffer.Take(bytesRead));

                userData.Append("\",\"lnRead\":");
                userData.Append(bytesRead);
                userData.Append("}");
                posInStream += bytesRead;

                packetResult = CallMacro(UploadMacro, userData.ToString());
                if (packetResult.ErrorType != "None") return packetResult;
            } while (posInStream < bytesToRead);
            return packetResult;
        }

        private string GetPacketPrefix(string idUploadSession, string fileName, long fileLength)
        {
            StringBuilder packetPrefix = new StringBuilder(1024);
            packetPrefix.Append("{\"clientFileName\":\"");
            packetPrefix.Append(fileName);
            packetPrefix.Append("\",\"idUploadSession\":\"");
            packetPrefix.Append(idUploadSession);
            packetPrefix.Append("\",\"lnStream\":");
            packetPrefix.Append(fileLength);
            return packetPrefix.ToString();
        }

        private void AppendHexString(StringBuilder hex, IEnumerable<byte> raw)
        {
            const string HEXES = "0123456789ABCDEF";
            foreach (var b in raw)
            {
                hex.Append(HEXES[(b & 0xF0) >> 4])
                   .Append(HEXES[(b & 0x0F)]);
            }
        }

        private IHttpActionResult CreateBusinessDocument(string documentId, UpdateMode updateMode, string idUploadSession)
        {
            var args = new AttachmentArguments
            {
                IdUploadSession = idUploadSession,
                UpdateMode = updateMode
            };
            var path = $"/api/attachment/{documentId}/uploadfile";
            var data = new CallMacroArguments<AttachmentArguments>(path, args);

            var result = CallMacro(GraphQlMacro, data.ToString());

            return ProcessMacroResult(result, () =>
            {
                var message = JsonConvert.DeserializeObject(result.Content);
                return Ok(message);
            });
        }

        [HttpGet]
        [Route("{documentId}/file")]
        public IHttpActionResult DownloadFile(string documentId)
        {
            var data = JsonConvert.SerializeObject(new
            {
                Request = new { Path = $"/api/attachment/{documentId}/downloadfile" }
            });

            var result = CallMacro(GraphQlMacro, data);           

            return ProcessMacroResult(result, () =>
            {
                return ResponseMessage(FileDownloadHttpResponse.From(result));                
            });
        }        
    }
}
