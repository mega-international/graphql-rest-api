using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using GraphQL.Types;
using Hopex.Model.PivotSchema.Models;
using Hopex.Modules.GraphQL.Schema.Types.CustomScalarGraphTypes;
using Mega.Macro.API;
using Mega.Macro.API.Library;
using Newtonsoft.Json;

namespace Hopex.Modules.GraphQL.Schema.Types
{

    public class DiagnosticType : ObjectGraphType
    {
        public DiagnosticType()
        {
            // Platform
            Field<StringGraphType>("platformName", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                var site = root.CurrentEnvironment.Site;
                return site.VersionInformation.Name;
            });
            Field<StringGraphType>("platformVersionNumber", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                var site = root.CurrentEnvironment.Site;
                return $"{site.VersionInformation.NativeObject.ReleaseMajorNumber}.{site.VersionInformation.NativeObject.ReleaseMinorNumber}.{site.VersionInformation.PatchNumber}.{site.VersionInformation.NativeObject.HFNumber}";
            });
            Field<BooleanGraphType>("isCompiledMetamodel", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                return root.CurrentEnvironment.IsCompiled;
            });
            Field<StringGraphType>("systemInformationReport", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                return root.CurrentEnvironment.Site.SystemInformation;
            });
            Field<ListGraphType<MetamodelVersionType>>("metamodelVersion", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                var metaModelVersion = root.GetCollection("_SystemUpdateMarker");
                return metaModelVersion;
            });

            // Information Utilisateur
            Field<StringGraphType>("userLoginId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                var currentUser = root.GetCollection(MetaClassLibrary.PersonSystem).Item(root.CurrentEnvironment.CurrentUserId);
                var currentLogin = currentUser.GetCollection("~A20000008880[Login]").Item(1);
                return root.CurrentEnvironment.Toolkit.GetString64FromId(currentLogin.Id);
            });
            Field<StringGraphType>("userLoginName", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                var currentUser = root.GetCollection(MetaClassLibrary.PersonSystem).Item(root.CurrentEnvironment.CurrentUserId);
                var currentLogin = currentUser.GetCollection("~A20000008880[Login]").Item(1);
                return currentLogin.GetPropertyValue(MetaAttributeLibrary.Name);
            });
            Field<StringGraphType>("userPersonSystemId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                var currentUser = root.GetCollection(MetaClassLibrary.PersonSystem).Item(root.CurrentEnvironment.CurrentUserId);
                return root.CurrentEnvironment.Toolkit.GetString64FromId(currentUser.Id);
            });
            Field<StringGraphType>("userPersonSystemName", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                var currentUser = root.GetCollection(MetaClassLibrary.PersonSystem).Item(root.CurrentEnvironment.CurrentUserId);
                return currentUser.NativeObject.Name;
            });
            Field<StringGraphType>("userProfileId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                return root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.CurrentProfileId);
            });
            Field<StringGraphType>("userProfileName", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                var currentProfile = root.GetObjectFromId<MegaObject>(root.CurrentEnvironment.CurrentProfileId);
                return currentProfile.GetPropertyValue(MetaAttributeLibrary.Name);
            });
            Field<StringGraphType>("userCommandLine", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                var currentUser = root.GetCollection(MetaClassLibrary.PersonSystem).Item(root.CurrentEnvironment.CurrentUserId);
                var currentLogin = currentUser.GetCollection("~A20000008880[Login]").Item(1);
                var commandLine = currentLogin.GetPropertyValue(MetaAttributeLibrary.CommandLine);
                return commandLine;
            });
            Field<StringGraphType>("profileCommandLine", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                var currentProfile = root.GetCollection(MetaClassLibrary.Profile).Item(root.CurrentEnvironment.CurrentProfileId);
                var commandLine = currentProfile.GetPropertyValue(MetaAttributeLibrary.CommandLine);
                return commandLine;
            });

            // Information Serveur et Installation
            Field<StringGraphType>("serverTimeZone", resolve: context => TimeZoneInfo.Local.StandardName);
            Field<StringGraphType>("serverDateFormat", resolve: context => CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
            Field<StringGraphType>("environmentName", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                return root.SystemRoot.CurrentEnvironment.Path.Substring(root.SystemRoot.CurrentEnvironment.Path.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            });
            Field<StringGraphType>("repositoryName", resolve: context =>
            {
                var root = ((UserContext)context.UserContext["usercontext"]).MegaRoot;
                return root.NativeObject.GetProp("Name");
            });

            // Schema
            Field<StringGraphType>("schemaStandardMetaModelCompatibility", resolve: context =>
            {
                var configFolder = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\CONFIG";
                var compatibilityListJson = File.ReadAllText($@"{configFolder}\compatibility_list.json");
                return JsonConvert.DeserializeObject(compatibilityListJson);
            });
            Field<CustomDateGraphType>("schemaStandardFileDate", resolve: context =>
            {
                if (context.UserContext["usercontext"] is UserContext userContext)
                {
                    var schemaFile = GetSchemaFile(userContext);
                    if (schemaFile.Exists)
                    {
                        return schemaFile.LastWriteTime;
                    }
                    return null;
                }
                return null;
            });
            Field<CustomDateGraphType>("schemaCustomFileDate", resolve: context =>
            {
                if (context.UserContext["usercontext"] is UserContext userContext)
                {
                    var schemaFile = GetSchemaFile(userContext, true);
                    if (schemaFile.Exists)
                    {
                        return schemaFile.LastWriteTime;
                    }
                    return null;
                }
                return null;
            });
            Field<CustomDateGraphType>("schemaStandardLatestGeneration", resolve: context =>
            {
                if (context.UserContext["usercontext"] is UserContext userContext)
                {
                    var schemaFile = GetSchemaFile(userContext);
                    if (schemaFile.Exists)
                    {
                        var jsonSchema = File.ReadAllText(schemaFile.FullName);
                        var schema = JsonConvert.DeserializeObject<PivotSchema>(jsonSchema);
                        return schema.Version.LatestGeneration;
                    }
                    return null;
                }
                return null;
            });
            Field<CustomDateGraphType>("schemaCustomLatestGeneration", resolve: context =>
            {
                if (context.UserContext["usercontext"] is UserContext userContext)
                {
                    var schemaFile = GetSchemaFile(userContext, true);
                    if (schemaFile.Exists)
                    {
                        var jsonSchema = File.ReadAllText(schemaFile.FullName);
                        var schema = JsonConvert.DeserializeObject<PivotSchema>(jsonSchema);
                        return schema.Version.LatestGeneration;
                    }
                    return null;
                }
                return null;
            });
        }

        private static FileInfo GetSchemaFile(UserContext userContext, bool getCustomFile = false)
        {
            var configFolder = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\CONFIG";
            var folderType = getCustomFile ? "Custom" : "Standard";
            var fileName = $"{userContext.Schema.SchemaName}.json";
            var schemaStandardFile = new FileInfo($@"{configFolder}\{userContext.Schema.Version}\{folderType}\{fileName}");
            return schemaStandardFile;
        }
    }
}
