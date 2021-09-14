using Mega.WebService.GraphQL.Tests.Sources.FieldModels;
using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    public class Test22 : AbstractTest
    {
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

        private readonly Profile _profile_hopex_customizer = new Profile("757wuc(SGjpJ", "Hopex Customizer");
        private readonly Profile _profile_incident_approver = new Profile("WKMtvDE(HrBV", "Incident approver");
        private readonly Profile _profile_incident_declarer = new Profile("aKMtQ)D(HH6V", "Incident declarer");

        public Test22(Parameters parameters) : base(parameters) { }

        protected override void Initialisation()
        {
            _requester = GenerateRequester($"{_myServiceUrl}/api/{(IsAsyncMode ? "async/" : "")}/{_schemaRisk}");
        }

        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            await ProcessGetIncidents();
        }

        private async Task ProcessGetIncidents()
        {
            var countRef = await GetCountIncidents(_profile_hopex_customizer);
            CountedStep($"Number total of incidents for profile {_profile_hopex_customizer.Name}", countRef);

            var profiles = new Profile[] { _profile_incident_approver, _profile_incident_declarer };
            (string, bool) Compare(int current, int target)
            {
                var success = current < target;
                var message = success ? $"Success: current: {current} is lower than reference: {target}" :
                    $"Error: current: {current} must be lower than reference: {target}";
                return (message, success);
            }
            foreach (var profile in profiles)
            {
                var count = await GetCountIncidents(profile);
                CountedStep($"Number of incidents for profile {profile.Name} compared to reference {_profile_hopex_customizer.Name}",
                    count, countRef, Compare);
            }
        }

        private async Task<int> GetCountIncidents(Profile profile)
        {
            SetConfig(Destination.CloneWithProfile(profile.Id));
            var outputs = new List<Field> { new ScalarField(MetaFieldNames.id, "string") };
            var resultHopexCustomizer = await GetAll("Incident", outputs);
            return resultHopexCustomizer.Count;
        }
    }
}
