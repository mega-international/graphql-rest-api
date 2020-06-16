using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.IntegrationTests.DTO
{

    public class CurrentContextResponse
    {
        public CurrentContext _currentContext { get; set; }
        public class CurrentContext
        {
            public string UserId { get; set; }
            public string DatabaseId { get; set; }
            public string LibraryId { get; set; }
            public string ProfileId { get; set; }
            public string CurrencyCode { get; set; }
            public string CurrencyId { get; set; }
            public string Language { get; set; }
            public string LanguageName { get; set; }
            public string LanguageId { get; set; }
            public string DatabaseLanguage { get; set; }
            public string DatabaseLanguageName { get; set; }
            public string DatabaseLanguageId { get; set; }
            public string SystemLanguage { get; set; }
            public string SystemLanguageName { get; set; }
            public string SystemLanguageId { get; set; }
            public string WorkingEnvironmentTemplate { get; set; }
            public string WorkingEnvironmentGroupTemplate { get; set; }
            public string WorkingEnvironmentTopicTemplate { get; set; }
            public string WorkingEnvironment { get; set; }
            public string WorkingEnvironmentGroup { get; set; }
            public string WorkingEnvironmentTopic { get; set; }
            public string WorkingEnvironmentEntryPoint { get; set; }
        }
    }
}
