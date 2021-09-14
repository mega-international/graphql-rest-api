using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Mega.WebService.GraphQL.Tests.Sources.Requesters
{
    public class HASGraphQLRequester : BaseRequester
    {
        private static readonly string _apiKey = "x-api-key";
        private string _apiKeyValue = "";

        public HASGraphQLRequester(Uri uri)
        {
            EndPoint = uri;
        }

        public HASGraphQLRequester(string uri) : this(new Uri(uri)) { }

        public override void SetConfig(ISessionInfos sessionInfos)
        {
            if(sessionInfos is HASSessionInfos hasSessionInfos)
            {
                _apiKeyValue = hasSessionInfos.ApiKey;
            }
        }

        protected override void PrepareHeadersForExecute(HttpClient client)
        {
            SetHeadersField(client, _apiKey, _apiKeyValue);
            SetHeadersField(client, _hopexWait, "500");
        }

        protected override void PrepareHeadersForResult(HttpClient client, IEnumerable<string> session, IEnumerable<string> task)
        {
            SetHeadersField(client, _apiKey, _apiKeyValue);
            SetHeadersField(client, _hopexWait, "500");
            //SetHeadersField(client, _hopexSession, session);
            SetHeadersField(client, _hopexTask, task);
        }
    }
}
