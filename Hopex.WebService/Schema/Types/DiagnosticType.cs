using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using GraphQL.Types;
using Hopex.Model.PivotSchema.Models;
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
                var root = ((UserContext)context.UserContext).MegaRoot;
                var site = root.CurrentEnvironment.Site;
                return site.VersionInformation.Name;
            });
            Field<StringGraphType>("platformVersionNumber", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var site = root.CurrentEnvironment.Site;
                return $"{site.VersionInformation.NativeObject.ReleaseMajorNumber}.{site.VersionInformation.NativeObject.ReleaseMinorNumber}.{site.VersionInformation.PatchNumber}.{site.VersionInformation.NativeObject.HFNumber}";
            });
            Field<BooleanGraphType>("isCompiledMetamodel", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.IsCompiled;
            });
            Field<StringGraphType>("systemInformationReport", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.Site.SystemInformation;
            });
            Field<StringGraphType>("metamodelVersionId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var metaModelVersionJson = root.CurrentEnvironment.NativeObject.MetaModelCompilation(true);
                var metaModelVersion = JsonConvert.DeserializeObject<MetaModelVersion>(metaModelVersionJson);
                return metaModelVersion.Version;
            }); 

            // Information Utilisateur
            Field<StringGraphType>("userName", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var currentUser = root.GetCollection(MetaClassLibrary.PersonSystem).Item(root.CurrentEnvironment.CurrentUserId);
                return currentUser.NativeObject.Name;
            });
            Field<StringGraphType>("userLoginId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var currentUser = root.GetCollection(MetaClassLibrary.PersonSystem).Item(root.CurrentEnvironment.CurrentUserId);
                var currentLogin = currentUser.GetCollection("~A20000008880[Login]").Item(1);
                return root.CurrentEnvironment.Toolkit.GetString64FromId(currentLogin.Id);
            });
            Field<StringGraphType>("userPersonSystemId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var currentUser = root.GetCollection(MetaClassLibrary.PersonSystem).Item(root.CurrentEnvironment.CurrentUserId);
                return root.CurrentEnvironment.Toolkit.GetString64FromId(currentUser.Id);
            });
            Field<StringGraphType>("userProfileId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.CurrentProfileId);
            });
            Field<StringGraphType>("userCommandLine", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var currentUser = root.GetCollection(MetaClassLibrary.PersonSystem).Item(root.CurrentEnvironment.CurrentUserId);
                var currentLogin = currentUser.GetCollection("~A20000008880[Login]").Item(1);
                var commandLine = currentLogin.GetPropertyValue(MetaAttributeLibrary.CommandLine);
                return commandLine;
            });
            Field<StringGraphType>("profileCommandLine", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var currentProfile = root.GetCollection(MetaClassLibrary.Profile).Item(root.CurrentEnvironment.CurrentProfileId);
                var commandLine = currentProfile.GetPropertyValue(MetaAttributeLibrary.CommandLine);
                return commandLine;
            });

            // Information Serveur et Installation
            Field<StringGraphType>("serverTimeZone", resolve: context => TimeZone.CurrentTimeZone.StandardName);
            Field<StringGraphType>("serverDateFormat", resolve: context => CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
            Field<StringGraphType>("environmentName", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.SystemRoot.NativeObject.GetProp("Name");
            });
            Field<StringGraphType>("repositoryName", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.NativeObject.GetProp("Name");
            });

            // Schema
            Field<StringGraphType>("schemaStandardMetaModelCompatibility", resolve: context =>
            {
                var configFolder = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\CONFIG";
                var compatibilityListJson = File.ReadAllText($@"{configFolder}\compatibility_list.json");
                return JsonConvert.DeserializeObject(compatibilityListJson);
            });
            Field<DateGraphType>("schemaStandardFileDate", resolve: context =>
            {
                if (context.UserContext is UserContext userContext)
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
            Field<DateGraphType>("schemaCustomFileDate", resolve: context =>
            {
                if (context.UserContext is UserContext userContext)
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
            Field<DateGraphType>("schemaStandardLatestGeneration", resolve: context =>
            {
                if (context.UserContext is UserContext userContext)
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
            Field<DateGraphType>("schemaCustomLatestGeneration", resolve: context =>
            {
                if (context.UserContext is UserContext userContext)
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
            var endOfPath = userContext.SchemaFile.Replace(configFolder, "");
            endOfPath = getCustomFile ? endOfPath.Replace("\\Standard\\", "\\Custom\\") : endOfPath.Replace("\\Custom\\", "\\Standard\\");
            var schemaStandardFile = new FileInfo(configFolder + endOfPath + ".json");
            return schemaStandardFile;
        }
    }
}
