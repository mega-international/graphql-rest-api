using Hopex.ApplicationServer.WebServices;
using Hopex.Common.JsonMessages;
using Hopex.Model;
using Hopex.Model.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;

namespace Hopex.Modules.GraphQL
{
    public abstract class BaseSatelliteEntryPoint<TArguments> : HopexWebService<TArguments> where TArguments : class
    {
        protected IFileSource fileSource;
        protected abstract string PathPrefix { get; }

        protected virtual IMegaRoot GetRoot()
        {
            return RealMegaRootFactory.FromNativeRoot(HopexContext.NativeRoot);
        }

        public override async Task<HopexResponse> Execute(TArguments args)
        {
            var method = HopexContext.Request.Path.Substring(HopexContext.Request.Path.LastIndexOf('/') + 1);
            var root = GetRoot();
            var path = HopexContext.Request.Path;
            var objectId = path.Substring(PathPrefix.Length, path.LastIndexOf('/') - PathPrefix.Length);
            return await ExecuteOnObject(root, objectId, method, args);
        }

        protected abstract Task<HopexResponse> ExecuteOnObject(IMegaRoot root, string objectId, string method, TArguments args);

        protected HopexResponse BuildTempFileResult(string fileName, string fullPath)
        {            
            var bytes = fileSource.ReadAllBytes(fullPath);
            fileSource.Delete(fullPath);
            return BuildFileContentResult(fileName, bytes);            
        }

        protected HopexResponse BuildFileContentResult(string fileName, byte[] bytes)
        {
            var contentType = GetMimeMapping(fileName);
            //if (Utils.IsRunningInHAS)
            //{
            //    var response = HopexResponse.Binary(new MemoryStream(bytes), contentType);
            //    var contentDisposition = new System.Net.Mime.ContentDisposition
            //    {
            //        FileName = fileName
            //    };
            //    response.Headers.Add(new KeyValuePair<string, string>("Content-Disposition", contentDisposition.ToString()));
            //    return response;
            //}
            var content = Convert.ToBase64String(bytes);
            var result = new FileDownloadMacroResponse { FileName = fileName, ContentType = contentType, Content = content };
            return HopexResponse.Json(JsonConvert.SerializeObject(result));
        }

        private string GetMimeMapping(string fileName)
        {
            var extensionName = Path.GetExtension(fileName).TrimStart('.').ToLower();
            if (extensionName == "svg") return "image/svg+xml";
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);
            return contentType ?? "application/octet-stream";
        }
    }
}
