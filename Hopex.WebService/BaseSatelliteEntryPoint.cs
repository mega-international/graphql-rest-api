using Hopex.ApplicationServer.WebServices;
using Hopex.Common.JsonMessages;
using Hopex.Model.Mocks;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace Hopex.Modules.GraphQL
{
    public abstract class BaseSatelliteEntryPoint<T> : HopexWebService<T> where T : class
    {
        protected IFileSource fileSource;
        protected abstract string PathPrefix { get; }

        protected virtual IMegaRoot GetRoot()
        {
            return RealMegaRootFactory.FromNativeRoot(HopexContext.NativeRoot);
        }

        public override async Task<HopexResponse> Execute(T args)
        {
            var method = HopexContext.Request.Path.Substring(HopexContext.Request.Path.LastIndexOf('/') + 1);
            var root = GetRoot();
            var path = HopexContext.Request.Path;
            var objectId = path.Substring(PathPrefix.Length, path.LastIndexOf('/') - PathPrefix.Length);
            return await ExecuteOnObject(root, objectId, method, args);
        }

        protected abstract Task<HopexResponse> ExecuteOnObject(IMegaRoot root, string objectId, string method, T args);

        protected HopexResponse BuildTempFileResult(string fileName, string fullPath)
        {            
            var contentType = GetMimeMapping(fileName);
            var bytes = fileSource.ReadAllBytes(fullPath);
            var content = Convert.ToBase64String(bytes);
            var result = new FileDownloadMacroResponse { FileName = fileName, ContentType = contentType, Content = content };
            fileSource.Delete(fullPath);
            return HopexResponse.Json(JsonConvert.SerializeObject(result));
        }

        private string GetMimeMapping(string fileName)
        {
            var extensionName = Path.GetExtension(fileName).TrimStart('.').ToLower();
            if (extensionName == "svg") return "image/svg+xml";
            return MimeMapping.GetMimeMapping(fileName);
        }
    }
}
