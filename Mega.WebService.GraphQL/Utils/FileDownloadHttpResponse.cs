using Hopex.Common.JsonMessages;
using Mega.WebService.GraphQL.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Mega.WebService.GraphQL.Utils
{
    class FileDownloadHttpResponse
    {
        internal static HttpResponseMessage From(WebServiceResult result)
        {
            var downloadInfo = JsonConvert.DeserializeObject<FileDownloadMacroResponse>(result.Content);
            var contentBytes = Convert.FromBase64String(downloadInfo.Content);
            var contentStream = new StreamContent(new MemoryStream(contentBytes));
            var actionResult = new HttpResponseMessage(HttpStatusCode.OK) { Content = contentStream };
            actionResult.Content.Headers.ContentType = new MediaTypeHeaderValue(downloadInfo.ContentType);
            actionResult.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = downloadInfo.FileName };
            return actionResult;
        }
    }
}
