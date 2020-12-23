using GraphQL.Types;
using System.Linq;

namespace Hopex.Modules.GraphQL.Schema.Types
{

    public class CurrentContextType : ObjectGraphType
    {
        public CurrentContextType()
        {
            Field<StringGraphType>("userId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.CurrentUserId);
            });
            Field<StringGraphType>("databaseId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.Toolkit.GetString64FromId(root.Id);
            });
            Field<StringGraphType>("libraryId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.CurrentLibrary == null ? null : root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.CurrentLibrary);
            });
            Field<StringGraphType>("profileId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.CurrentProfileId);
            });
            Field<StringGraphType>("currencyCode", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.Currency.GetUserCurrencyCode();
            });
            Field<StringGraphType>("currencyId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.Currency.GetUserCurrencyId());
            });
            Field<StringGraphType>("language", resolve: context =>
            {
                var userContext = (UserContext)context.UserContext;
                var root = userContext.MegaRoot;
                var languageId = root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.CurrentLanguageId);
                return userContext.Languages.FirstOrDefault(x => x.Value.MegaUnnamedField.Substring(1, 12) == languageId).Key;
            });
            Field<StringGraphType>("languageName", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.CurrentLanguageName;
            });
            Field<StringGraphType>("languageId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.CurrentLanguageId);
            });
            Field<StringGraphType>("databaseLanguage", resolve: context =>
            {
                var userContext = (UserContext)context.UserContext;
                var root = userContext.MegaRoot;
                var languageId = root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.DatabaseLanguage.Id);
                return userContext.Languages.FirstOrDefault(x => x.Value.MegaUnnamedField.Substring(1, 12) == languageId).Key;
            });
            Field<StringGraphType>("databaseLanguageName", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.DatabaseLanguage.Name;
            });
            Field<StringGraphType>("databaseLanguageId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.DatabaseLanguage.Id);
            });
            Field<StringGraphType>("systemLanguage", resolve: context =>
            {
                var userContext = (UserContext)context.UserContext;
                var root = userContext.MegaRoot;
                var languageId = root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.SystemLanguage.Id);
                return userContext.Languages.FirstOrDefault(x => x.Value.MegaUnnamedField.Substring(1, 12) == languageId).Key;
            });
            Field<StringGraphType>("systemLanguageName", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.SystemLanguage.Name;
            });
            Field<StringGraphType>("systemLanguageId", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                return root.CurrentEnvironment.Toolkit.GetString64FromId(root.CurrentEnvironment.SystemLanguage.Id);
            });
            Field<StringGraphType>("workingEnvironmentTemplate", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var obj = root.NativeObject.WorkingEnvironmentManager.GetCurrentWorkingEnvironmentTemplate;
                return obj == null ? null : root.CurrentEnvironment.Toolkit.GetString64FromId(obj.Id);
            });
            Field<StringGraphType>("workingEnvironmentGroupTemplate", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var obj = root.NativeObject.WorkingEnvironmentManager.GetCurrentWorkingEnvironmentGroupTemplate;
                return obj == null ? null : root.CurrentEnvironment.Toolkit.GetString64FromId(obj.Id);
            });
            Field<StringGraphType>("workingEnvironmentTopicTemplate", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var obj = root.NativeObject.WorkingEnvironmentManager.GetCurrentWorkingEnvironmentTopicTemplate;
                return obj == null ? null : root.CurrentEnvironment.Toolkit.GetString64FromId(obj.Id);
            });
            Field<StringGraphType>("workingEnvironment", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var obj = root.NativeObject.WorkingEnvironmentManager.GetCurrentWorkingEnvironment;
                return obj == null ? null : root.CurrentEnvironment.Toolkit.GetString64FromId(obj.Id);
            });
            Field<StringGraphType>("workingEnvironmentGroup", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var obj = root.NativeObject.WorkingEnvironmentManager.GetCurrentWorkingEnvironmentGroup;
                return obj == null ? null : root.CurrentEnvironment.Toolkit.GetString64FromId(obj.Id);
            });
            Field<StringGraphType>("workingEnvironmentTopic", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var obj = root.NativeObject.WorkingEnvironmentManager.GetCurrentWorkingEnvironmentTopic;
                return obj == null ? null : root.CurrentEnvironment.Toolkit.GetString64FromId(obj.Id);
            });
            Field<StringGraphType>("workingEnvironmentEntryPoint", resolve: context =>
            {
                var root = ((UserContext)context.UserContext).MegaRoot;
                var obj = root.NativeObject.WorkingEnvironmentManager.GetCurrentWorkingEnvironmentEntryPoint;
                return obj == null ? null : root.CurrentEnvironment.Toolkit.GetString64FromId(obj.Id);
            });
        }
    }
}
