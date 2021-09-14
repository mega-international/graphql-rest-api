using Mega.WebService.GraphQL.Tests.Sources.FieldModels;
using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using Mega.WebService.GraphQL.Tests.Sources.Requesters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{

    public class Test21 : AbstractTest
    {
        private class User
        {
            public string LoginName { get; }
            public string PersonName { get; }
            public string Password { get; }

            public User(string loginName, string personName, string password)
            {
                LoginName = loginName;
                PersonName = personName;
                Password = password;
            }
        }

        private class Profile
        {
            public string Id { get; }
            public string Name { get; }

            public Profile(string id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        private readonly User[] _users =
        {
            new User("jmv", "VAURY Jean-Marie", "Hopex"),
            new User("scr", "CRONIER Sébastien", "Hopex"),
            new User("lgn", "GIBELIN Laurent", "Hopex"),
            //new User("ogd", "GUIMARD Olivier", "Hopex"),
            new User("sca", "CANONICA Sébastien", "Hopex")
        };

        private readonly Profile[] _profiles =
        {
            new Profile("tWisjijuFnES", "Auditor"),
            new Profile("(aE2vwG6HTH1", "ITPM Functional Administrator"),
            new Profile("7m7F)W4zFPqF", "Application Owner"),
            new Profile("vjjLZ1NuF50H", "Audit Director"),
            new Profile("o0cWJgfOO1VU", "Project Portfolio Manager")
        };

        private readonly string[] _fields =
        {
            "userLoginName",
            "userPersonSystemName",
            "userProfileName"
        };

        public Test21(Parameters parameters) : base(parameters) { }

        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            await ProcessDiagnosticsAsync();
        }

        private async Task ProcessDiagnosticsAsync()
        {
            int count = 0;
            foreach (var user in _users)
            {
                foreach (var profile in _profiles)
                {
                    count += await CheckSessionAsync(user, profile) ? 1 : 0;
                }
            }
            var expected = _users.Length * _profiles.Length;
            CountedStep("Sessions connected successfully", count, expected);
        }

        private async Task<bool> CheckSessionAsync(User user, Profile profile)
        {
            var requester = GenerateRequester($"{_myServiceUrl}/api/{(IsAsyncMode ? "async/" : "")}{_schemaITPM}");
            SetConfig(requester, Destination.CloneWithProfile(profile.Id));
            if (requester is GraphQLRequester graphQLRequester)
            {
                graphQLRequester.Login = user.LoginName;
                graphQLRequester.Password = user.Password;
            }
            var fieldsQuery = "";
            foreach (var field in _fields)
            {
                fieldsQuery += field + " ";
            }
            var query = $"query{{ _APIdiagnostic{{ {fieldsQuery} }} }}";
            JToken result;
            try
            {
                result = (await ProcessRawQuery(requester, query))["_APIdiagnostic"];
            }
            catch (Exception ex)
            {
                DetailedStep($"Error occured when trying to diagnostic for user: {user.LoginName} and profile {profile.Name}:<br>{ex.ToString()}");
                return false;
            }
            var ok = true;
            foreach (var field in _fields)
            {
                var resultField = result[field].ToString();
                var expected = GetValue(user, profile, field);
                if (string.Compare(resultField, expected, true) != 0)
                {
                    ok = false;
                    var details = $"User checked: {user.LoginName} [{user.PersonName}]<br>";
                    details += $"Profile checked: {profile.Id} [{profile.Name}]<br>";
                    details += $"Field: {field}, value returned: {resultField}, expected: {expected}";
                    DetailedStep(details);
                }
            }
            return ok;
        }

        private string GetValue(User user, Profile profile, string field)
        {
            switch (field)
            {
                case "userLoginName":
                    return user.LoginName;

                case "userPersonSystemName":
                    return user.PersonName;

                case "userProfileName":
                    return profile.Name;

                default:
                    throw new ArgumentException($"field {field} does not match any expected field");
            }
        }
    }
}
