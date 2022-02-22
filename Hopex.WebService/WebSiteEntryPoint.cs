using Hopex.ApplicationServer.WebServices;
using Hopex.Common;
using Hopex.Common.JsonMessages;
using Mega.Macro.API;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL
{
    [HopexWebService(WebServiceRoute)]
    [HopexMacro(MacroId = "AAC8AB1E5D25678E")]
    public class WebSiteEntryPoint : HopexWebService<WebSiteArguments>
    {
        private const string WebServiceRoute = "generatewebsite";
        private static int _logMacroId;
        
        public override Task<HopexResponse> Execute(WebSiteArguments args)
        {
            _logMacroId = Logger.InitMacroId("WEBSITEGENERATE");
            MegaRoot root = MegaWrapperObject.Cast<MegaRoot>(HopexContext.NativeRoot);
            Logger.LogInformation("GraphQL website generation start");
            ErrorMacroResponse result = new ErrorMacroResponse();

            var location = Assembly.GetExecutingAssembly().Location;
            var shadowfilesFolderPath = Path.GetFullPath(Path.Combine(location, @"..\..\..\..\"));
            DirectoryInfo shadowfilesDirectory = new DirectoryInfo(shadowfilesFolderPath);
            if(!shadowfilesDirectory.Exists)
            {
                Logger.LogInformation("This feature is only available for a version of HOPEX V5 and above.");
                result = new ErrorMacroResponse(HttpStatusCode.InternalServerError, "This feature is only available for a version of HOPEX V5 and above.");
                return Task.FromResult(HopexResponse.Json(JsonConvert.SerializeObject(result)));
            }
            var hotInstallFolderPath = Path.GetFullPath(Path.Combine(location, @"..\..\..\..\..\Modules\.hot-install"));
            var contentModuleFolderPath = Path.GetFullPath(Path.Combine(shadowfilesFolderPath, "website.static.content"));
            DirectoryInfo contentModuleVersion = new DirectoryInfo(contentModuleFolderPath).GetDirectories().OrderByDescending(d => d.LastWriteTimeUtc).First();

            string webSiteId = args.webSiteId;
            string languagesCode = args.languagesCode;
            bool forceContinuOnError = args.forceContinuOnError;

            MegaObject website = root.GetObjectFromId<MegaObject>(webSiteId);
            if(website.Id == null)
            {
                var errorIDResult = new ErrorMacroResponse(HttpStatusCode.InternalServerError, $"Website with id {webSiteId} doesn't exist. Please check the id of your website object");
                Logger.LogInformation($"Website with id {webSiteId} doesn't exist. Please check the id of your website object", _logMacroId);
                return Task.FromResult(HopexResponse.Json(JsonConvert.SerializeObject(errorIDResult)));
            }

            //Original properties of the environment and website
            MegaLanguage originalEnvironmentMegaLanguage = root.CurrentEnvironment.CurrentLanguage;
            MegaObject originalEnvironmentLanguage = root.GetObjectFromId<MegaObject>(originalEnvironmentMegaLanguage.Id);
            string websiteOriginalPath = website.GetPropertyValue("~dAChvzAqqq00[Web Site Path]");
            string fullWebsiteOriginalPath = websiteOriginalPath.Replace("%ENV%", root.CurrentEnvironment.Path);

            //Clear the website generation folder
            ClearFolder(fullWebsiteOriginalPath);
            
            foreach(string languageCode in languagesCode.Split(';'))
            {
                //Get language from language code
                MegaCollection languageCollection = root.GetSelection($"Select [Language] Where [Language Code] ='{languageCode}'");
                if(languageCollection.Count != 1)
                {
                    var languageError = new ErrorMacroResponse(HttpStatusCode.InternalServerError, $"Please check language code : '{languageCode}'");
                    Logger.LogInformation($"Please check language code : '{languageCode}'", _logMacroId);
                    return Task.FromResult(HopexResponse.Json(JsonConvert.SerializeObject(languageError)));
                }
                MegaObject language = languageCollection.Item(1);
                root.CurrentEnvironment.NativeObject.SetCurrentLanguage(language.MegaUnnamedField.Substring(0, 13));
                try
                {
                    website.SetPropertyValue("~dAChvzAqqq00[Web Site Path]", $"{fullWebsiteOriginalPath}\\{languageCode}");
                }
                catch(Exception e)
                {
                    var errorLockResult = new ErrorMacroResponse(HttpStatusCode.InternalServerError, $"Website with id {webSiteId} is locked");
                    Logger.LogError(e);
                    return Task.FromResult(HopexResponse.Json(JsonConvert.SerializeObject(errorLockResult)));
                }
                
                try
                {
                    //Website generation
                    //website.CallFunction("~bPk4JF1Fzu00[GenerateWebSitewithAPI]");
                    //website.CallFunction("~cRk4WD1Fz800[GenerateWebSite]"); 
                    Logger.LogInformation($"Start generation of website in '{languageCode}'", _logMacroId);
                    website.CallMethod("~bPk4JF1Fzu00[GenerateWebSitewithAPI]");
                    Logger.LogInformation("Enf of generation", _logMacroId);
                }
                catch
                {
                    Logger.LogInformation("Error during the website generation. Check megaerr file for more detail", _logMacroId);
                    result = new ErrorMacroResponse(HttpStatusCode.InternalServerError, $"Error during the website's generation");
                    if(!forceContinuOnError)
                    {
                        return Task.FromResult(HopexResponse.Json(JsonConvert.SerializeObject(result)));
                    }
                }
                finally
                {
                    SetBackToDefaultEnvValues(root, website, originalEnvironmentLanguage, websiteOriginalPath);
                    Logger.LogInformation("Initial environment's values have been reset. Transaction has been published", _logMacroId);
                    if(forceContinuOnError && languageCode.Equals(languagesCode.Split(';').Last()))
                    {
                        Logger.LogInformation("Generation errors will not prevent module to package", _logMacroId);
                        CopyAndPackageWebSiteModule(hotInstallFolderPath, contentModuleVersion, fullWebsiteOriginalPath);
                    }
                } 
            }
            if(!forceContinuOnError)
            {
                CopyAndPackageWebSiteModule(hotInstallFolderPath, contentModuleVersion, fullWebsiteOriginalPath);
                result = new ErrorMacroResponse(HttpStatusCode.OK, $"Website was successfully generated and packaged in '{languagesCode}'.");
            }
            Logger.LogInformation("GraphQL website generation end");
            return Task.FromResult(HopexResponse.Json(JsonConvert.SerializeObject(result)));
        }

        private void CopyAndPackageWebSiteModule(string hotInstallFolderPath, DirectoryInfo contentModuleVersion, string fullWebsiteOriginalPath)
        {
            Logger.LogInformation("Start copying website(s) files to the module's folder", _logMacroId);
            CopyWebSiteFilesToModule(contentModuleVersion.FullName, fullWebsiteOriginalPath);
            Logger.LogInformation("End copying website(s) files to the module's folder", _logMacroId);
            string newModuleVersion = $"15.{DateTime.Now.Year}.{DateTime.Now.Month}+{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}";
            Logger.LogInformation($"Start packaging the module version {newModuleVersion}", _logMacroId);
            PackageModule(contentModuleVersion.FullName, hotInstallFolderPath, newModuleVersion);
            Logger.LogInformation("End packaging the module", _logMacroId);
        }

        //Reassign original environment and website properies
        private static void SetBackToDefaultEnvValues(MegaRoot root, MegaObject website, MegaObject originalEnvironmentLanguage, string websiteOriginalPath)
        {
            root.CurrentEnvironment.NativeObject.SetCurrentLanguage(originalEnvironmentLanguage.MegaUnnamedField.Substring(0, 13));
            website.SetPropertyValue("~dAChvzAqqq00[Web Site Path]", websiteOriginalPath);
            root.CallFunction("~lcE6jbH9G5cK", "{\"instruction\":\"POSTPUBLISHINSESSION\"}");
        }

        public static void PackageModule(string ModuleFolderPath, string DestinationFolderPath, string ModuleVersion)
        {
            //TODO
            //For .Net 6.0 use the ModulePackger instead of dotnet-has
            /*var packager = new ModulePackager(ModuleFolderPath, "debug");
            packager.CreatePackageAsync("16.0.0", false, false, DestinationFolderPath);*/
            
            try
            {
                var info = new ProcessStartInfo

                {
                    UseShellExecute = false,
                    FileName = @"dotnet-has.exe",
                    Arguments = $"module create -p \"{ModuleFolderPath}\" --copy-to \"{DestinationFolderPath}\" --version \"{ModuleVersion}\"",
                    ErrorDialog = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                var sb = new StringBuilder();
                var process = Process.Start(info);
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.OutputDataReceived += (p, e) =>
                {
                    sb.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (p, e) =>
                {
                    sb.AppendLine(e.Data);
                };
                process.WaitForExit();
                var output = sb.ToString();
            }
            catch(Exception e)
            {
                
            }
        }
        public static void CopyWebSiteFilesToModule(string ModuleFolderPath, string WebSiteFolderPath)
        {
            DirectoryInfo wwwrootTemplate = new DirectoryInfo($"{ModuleFolderPath}\\wwwrootTemplate\\");
            DirectoryInfo wwwroot = new DirectoryInfo($"{ModuleFolderPath}\\wwwroot\\");
            ClearFolder(wwwroot.FullName);
            //copy content of the website(s) and wwwrootTemplate folders
            CopyFilesRecursively(WebSiteFolderPath, wwwroot.FullName);
            CopyFilesRecursively(wwwrootTemplate.FullName, wwwroot.FullName);

        }
        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach(string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach(string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }
        private static void ClearFolder(string FolderPath)
        {
            DirectoryInfo directory = new DirectoryInfo(FolderPath);
            if(directory.Exists)
            {
                FileInfo [] files = directory.GetFiles();
                //Clear all files and directories inside the wwwroot folder
                foreach(FileInfo file in files)
                {
                    file.Delete();
                }
                DirectoryInfo [] subDirectories = directory.GetDirectories();
                foreach(DirectoryInfo subDirectory in subDirectories)
                {
                    subDirectory.Delete(true);
                }
            }
        }
    }
}

