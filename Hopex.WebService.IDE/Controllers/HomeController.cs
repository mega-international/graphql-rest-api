using System.Collections.Generic;
using System.Linq;
using Hopex.WebService.IDE.Models;
using IdentityModel.Client;
using Mega.Has.Commons;
using Mega.Has.WebSite;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hopex.WebService.IDE.Controllers
{
    public class HomeController : HopexSessionController
    {
        private readonly IHASClient _hopex;
        private ILogger<HopexSessionController> _logger;
        private readonly IClusterConfiguration _configurations;
        private readonly List<string> _schemas;
        private readonly string _selectedSchema;
        private readonly bool _isVoyagerEnabled;

        public HomeController(IHASClient hopex, ILogger<HopexSessionController> logger, IClusterConfiguration configurations, HopexGraphQlSettings hopexGraphQlSettings) : base(hopex, logger)
        {
            _hopex = hopex;
            _logger = logger;
            _configurations = configurations;
            _schemas = hopexGraphQlSettings.Schemas.Split(',').Select(p => p.Trim()).ToList();
            _selectedSchema = hopexGraphQlSettings.SelectedSchema;
            _isVoyagerEnabled = hopexGraphQlSettings.IsVoyagerEnabled;
        }

        public override async Task<IActionResult> Index()
        {
            var result = await ProcessOpenSessionAsync();
            if (result == null)
            {
                ViewBag.Schemas = new SelectList(_schemas, _selectedSchema);
                ViewBag.IsVoyagerEnabled = _isVoyagerEnabled;
                result = View();
            }
            return result;
        }

        [HttpGet("home/index/{selectedSchema}")]
        public async Task<IActionResult> Index(string selectedSchema)
        {
            var result = await ProcessOpenSessionAsync();
            if (result == null)
            {
                ViewBag.Schemas = new SelectList(_schemas, selectedSchema);
                ViewBag.IsVoyagerEnabled = _isVoyagerEnabled;
                result = View();
            }
            return result;
        }

        [HttpGet("home/voyager/{selectedSchema}")]
        public async Task<IActionResult> Voyager(string selectedSchema = "")
        {
            var result = await ProcessOpenSessionAsync();
            if (result == null)
            {
                ViewBag.Schemas = new SelectList(_schemas, string.IsNullOrEmpty(selectedSchema) ? _selectedSchema : selectedSchema);
                result = View();
            }
            return result;
        }

        [Authorize(Policy = "Hopex")]
        [HttpPost("graphql/{schema}")]
        public async Task<IActionResult> CallGraphQl(string schema)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            using (var client = new HttpClient())
            {
                client.SetBearerToken(token);

                var msg = new HttpRequestMessage(HttpMethod.Post, $"{_configurations.RuntimeClusterSettings.InternalAddress}/api/graphql/{schema}");
                if (Request.Cookies.TryGetValue(HopexHeaders.SessionToken, out var sv))
                {
                    msg.Headers.Add(HopexHeaders.SessionToken, sv);
                }
                msg.Content = new StreamContent(Request.Body);
                using (var response = await client.SendAsync(msg))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        return new ContentResult { Content = res, ContentType = "application/json", StatusCode = 200 };
                    }
                    return new StatusCodeResult((int)response.StatusCode);
                }
            }
        }
    }
}
