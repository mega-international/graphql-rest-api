using IdentityModel.Client;
using Mega.Bridge.Models;
using Mega.Bridge.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Mega.WebService.GraphQL.Filters
{
    public class HopexAuthenticationFilter : ActionFilterAttribute, IAuthenticationFilter
    {
        private string UasUrl { get; } = ConfigurationManager.AppSettings["AuthenticationUrl"].TrimEnd('/');

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;
            var authorization = request.Headers.Authorization;

            if (authorization == null || string.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                return;
            }

            switch (authorization.Scheme)
            {
                case "Basic":
                    var authorizationParameter = Convert.FromBase64String(authorization.Parameter);
                    var userNameAndPassword = Encoding.UTF8.GetString(authorizationParameter).Split(':');
                    var userName = userNameAndPassword[0];
                    var password = userNameAndPassword[1];
                    await AuthenticateBasic(context, userName, password, request);
                    break;
                case "Bearer":
                    await AuthenticateBearer(context, authorization.Parameter, request);
                    break;
                default:
                    context.ErrorResult = new AuthenticationFailureResult("Unsupported scheme", request);
                    break;
            }
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            var challenge = new AuthenticationHeaderValue("Basic");
            context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
            return Task.FromResult(0);
        }

        private async Task AuthenticateBasic(HttpAuthenticationContext context, string userName, string password, HttpRequestMessage request)
        {
            var clientId = ConfigurationManager.AppSettings["ClientId"];
            var clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
            var scopes =  ConfigurationManager.AppSettings["Scopes"];

            string environmentId;
            if (context.Request.Headers.Contains("x-hopex-environment-id"))
            {
                environmentId = context.Request.Headers.GetValues("x-hopex-environment-id").FirstOrDefault();
            }
            else if (context.Request.Headers.TryGetValues("x-hopex-context", out var hopexContextHeader) && HopexServiceHelper.TryGetHopexContext(hopexContextHeader.FirstOrDefault(), out var hopexContext))
            {
                environmentId = hopexContext.EnvironmentId;
            }
            else
            {
                environmentId = ConfigurationManager.AppSettings["EnvironmentId"];
            }

            using (var tokenClient = new TokenClient($"{UasUrl}/connect/token", clientId, clientSecret, null, AuthenticationStyle.PostValues))
            {
                var result = await tokenClient.RequestResourceOwnerPasswordAsync(userName, password, scopes, new Dictionary<string, string> { { "environmentId", environmentId } });
                if (result.IsError)
                {
                    context.ErrorResult = new AuthenticationFailureResult(result.Error, request);
                    return;
                }
                await AuthenticateBearer(context, result.AccessToken, request);
            }
        }

        private async Task AuthenticateBearer(HttpAuthenticationContext context, string accessToken, HttpRequestMessage request)
        {
            using(var userInfoClient = new UserInfoClient($"{UasUrl}/connect/userinfo"))
            {
                var result = await userInfoClient.GetAsync(accessToken);
                if (result.IsError)
                {
                    context.ErrorResult = new AuthenticationFailureResult(result.Error, request);
                    return;
                }
                var userInfo =  result.Json.ToObject<UserInfo>();
                context.Request.Properties.Add("UserInfo", userInfo);
            }
        }
    }    
}
