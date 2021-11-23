using Hopex.ApplicationServer.WebServices;
using Hopex.Common.JsonMessages;
using Hopex.Model.Abstractions;
using Mega.Macro.API;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL
{
    [HopexWebService(WebServiceRoute)]
    [HopexMacro(MacroId = "AAC8AB1E5D25678E")]
    public class AttachmentEntryPoint : BaseSatelliteEntryPoint<AttachmentArguments>
    {
        private const string WebServiceRoute = "attachment";
        protected override string PathPrefix => $"/api/{WebServiceRoute}/";

        public AttachmentEntryPoint()
            :this(new PhysicalFileSource()) { }

        public AttachmentEntryPoint(IFileSource fileSource)
        {
            this.fileSource = fileSource;
        }

        protected async override Task<HopexResponse> ExecuteOnObject(IMegaRoot root, string documentId, string method, AttachmentArguments args)
        {
            try
            {
                switch (method.ToLower())
                {
                    case "uploadfile":
                    {
                        Logger.LogInformation($"Uploading file {documentId}");
                        bool success = SaveBusinessDocument(root, documentId, args);
                        var result = new { documentId, success };
                        var response = HopexResponse.Json(JsonConvert.SerializeObject(result));
                        PublishStayInSession(root);
                        return await Task.FromResult(response);
                    }
                    case "downloadfile":
                    {
                        Logger.LogInformation($"Downloading file {documentId}");
                        return await Task.FromResult(GetBusinessDocumentContent(root, documentId));
                    }
                    default:
                        return await Task.FromResult(HopexResponse.Error(400, JsonConvert.SerializeObject($"Unknown method: {method}")));
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Document {documentId} does not exist or is confidential";
                Logger.LogError(ex, errorMessage);
                var result = new ErrorMacroResponse(HttpStatusCode.BadRequest, errorMessage);
                return await Task.FromResult(HopexResponse.Json(JsonConvert.SerializeObject(result)));
            }
        }

        private bool SaveBusinessDocument(IMegaRoot root, string documentId, AttachmentArguments args )
        {
            var filePath = root.CallFunctionString("~jtdsTU(tTLT0[GetUploadedFile]", args.IdUploadSession);
            var businessDocument = root.GetObjectFromId(documentId);
            var documentType = BusinessDocumentType.GetBusinessDocumentType(businessDocument);
            bool success;
            if (args.UpdateMode == UpdateMode.Replace)
            {
                var lastVersionCollection = businessDocument.GetCollection(documentType.LastVersionQueryId);
                var lastVersion = lastVersionCollection.Item(1);
                success = lastVersion.CallFunctionValue<bool>("~s5G0lNm(FXiR[BusinessDocumentSave]", filePath, false);
                if (success)
                {
                    var extensionName = Path.GetExtension(filePath).TrimStart('.');
                    lastVersion.SetPropertyValue("~Ekh4qloyFrmK[File Extension]", extensionName);
                }
            }
            else
            {
                var versionsCollection = businessDocument.GetCollection(documentType.VersionsCollectionId);
                var instanceCreator = versionsCollection.CallFunction<IMegaWizardContext>("~GuX91iYt3z70[InstanceCreator]");
                instanceCreator.InvokePropertyPut("~vjh4n6oyFTKK[File Location]", filePath);
                var newVersionId = instanceCreator.InvokeFunction<double>("Create");
                success = newVersionId != 0f;                
            }
            fileSource.Delete(filePath);
            return success;
        }

        private void PublishStayInSession(IMegaRoot root)
        {
            var publishResult = root.CallFunctionString("~lcE6jbH9G5cK[PublishStayInSessionWizard Command Launcher]", "{\"instruction\":\"PUBLISHINSESSION\",\"ifPossible\":true}");
            if (!publishResult.ToString().Contains("SESSION_PUBLISH"))
            {
                Logger.LogError(new Exception("Session wasn't published"));
            }
            Logger.LogInformation("Session published");
        }

        private HopexResponse GetBusinessDocumentContent(IMegaRoot root, string documentId)
        {
            using (var businessDocument = root.GetObjectFromId(documentId))
            {
                var path = businessDocument.CallFunctionString("~FFopcJjTGnIC[StaticDocumentFilePathGet]");
                var fileName = Path.GetFileName(path);
                return BuildTempFileResult(fileName, path);
            }
        }      

        private class BusinessDocumentType
        {
            public MegaId ClassId {get; private set;}
            public MegaId LastVersionQueryId { get; private set; }
            public MegaId VersionsCollectionId { get; private set; }

            public static BusinessDocumentType SYSTEM =
                new BusinessDocumentType("~aMRn)bUIGjX3[System Business Document]", "~U7CuchkAJ9iB[Last Version of the System Business Document]", "~vMRnR4VIGf84[System Document Versions]");
            public static BusinessDocumentType DATA =
                new BusinessDocumentType("~UkPT)TNyFDK5[Business Document]", "~67CucVkAJ1xA[Last Version of the Business Document]", "~ibEuqCE8GL(D[Document Versions]");

            private BusinessDocumentType(MegaId classId, MegaId lastVersionQueryId, MegaId versionsCollectionId)
            {
                ClassId = classId;
                LastVersionQueryId = lastVersionQueryId;
                VersionsCollectionId = versionsCollectionId;
            }

            internal static BusinessDocumentType GetBusinessDocumentType(IMegaObject businessDocument)
            {
                var classId = businessDocument.GetClassId();
                var root = businessDocument.Root;
                var toolkit = root.CurrentEnvironment.Toolkit;
                if (toolkit.IsSameId(SYSTEM.ClassId, classId))
                    return SYSTEM;
                return DATA;
            }
        }
    }
}
