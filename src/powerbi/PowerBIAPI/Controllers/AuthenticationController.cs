using Microsoft.Azure.AppService.ApiApps.Service;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using TRex.Metadata;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Web;
using System.Web.Configuration;
using PowerBIAPI.Models;

namespace PowerBIAPI.Controllers
{
    public class AuthenticationController : ApiController
    {
        private readonly string _clientId = ConfigurationManager.AppSettings["ClientId"];
        private readonly string _redirectUrl = ConfigurationManager.AppSettings["SiteUrl"];
        private readonly string _clientSecret = ConfigurationManager.AppSettings["ClientSecret"];

        [Metadata(Visibility = VisibilityType.Internal)]
        [HttpGet, Route("showRedirect")]
        public string ShowRedirect()
        {
            return "Your redirect URL is: " + _redirectUrl;
        }

        [Metadata(Visibility = VisibilityType.Internal)]
        [HttpGet, Route("authorize")]
        public System.Web.Http.Results.RedirectResult Authorize()
        {
            return Redirect(
                $"https://login.windows.net/common/oauth2/authorize?response_type=code&client_id={_clientId}&resource={HttpUtility.UrlEncode("https://analysis.windows.net/powerbi/api")}&redirect_uri={HttpUtility.UrlEncode(_redirectUrl)}");
        }

        [Metadata(Visibility = VisibilityType.Internal)]
        [HttpGet, Route("redirect")]
        public async Task<HttpResponseMessage> CompleteAuth(string code)
        {
            AuthenticationContext AC = new AuthenticationContext("https://login.windows.net/common/oauth2/authorize/");
            ClientCredential cc = new ClientCredential(_clientId, _clientSecret);
            AuthenticationResult ar = await AC.AcquireTokenByAuthorizationCodeAsync(code, new Uri(_redirectUrl), cc);
            PowerBiController.Authorization = new AuthResult { Expires = ar.ExpiresOn.UtcDateTime, AccessToken = ar.AccessToken, RefreshToken = ar.RefreshToken };
            WriteTokenToWebConfig(PowerBiController.Authorization);
            return Request.CreateResponse(HttpStatusCode.OK, "Successfully Authenticated");
        }

        internal async Task CheckToken()
        {
            if (PowerBiController.Authorization == null)
                //PowerBIController.authorization = await ReadTokenFromStorage();
                PowerBiController.Authorization = ReadTokenFromWebConfig();

            if (PowerBiController.Authorization == null)
                return;

            if (DateTime.UtcNow.CompareTo(PowerBiController.Authorization.Expires) >= 0)
            {
                AuthenticationContext AC = new AuthenticationContext("https://login.windows.net/common/oauth2/authorize/");
                ClientCredential cc = new ClientCredential(_clientId, _clientSecret);
                var ADALResult = await AC.AcquireTokenByRefreshTokenAsync(PowerBiController.Authorization.RefreshToken, cc);
                PowerBiController.Authorization = new AuthResult { Expires = ADALResult.ExpiresOn.UtcDateTime, AccessToken = ADALResult.AccessToken, RefreshToken = ADALResult.RefreshToken };
                //await WriteTokenToStorage(PowerBIController.authorization);
                WriteTokenToWebConfig(PowerBiController.Authorization);
            }
        }

        [HttpGet, Route("api/token")]
        public HttpResponseMessage GetToken()
        {
            return Request.CreateResponse(ReadTokenFromWebConfig());
        }

        private async Task WriteTokenToStorage(AuthResult ar)
        {
            try
            {
                var storage = Runtime.FromAppSettings().IsolatedStorage;
                await storage.WriteAsync("auth", JsonConvert.SerializeObject(ar));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        private void WriteTokenToWebConfig(AuthResult ar)
        {
            Configuration config =
                WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            config.AppSettings.Settings.Remove("auth");
            config.AppSettings.Settings.Add("auth", JsonConvert.SerializeObject(ar));
            config.Save();
        }

        private async Task<AuthResult> ReadTokenFromStorage()
        {
            var storage = Runtime.FromAppSettings().IsolatedStorage;
            var authString = await storage.ReadAsStringAsync("auth");
            return JsonConvert.DeserializeObject<AuthResult>(authString);
        }

        private AuthResult ReadTokenFromWebConfig()
        {
            Configuration config =
                WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            var authString = config.AppSettings.Settings["auth"].Value;
            if (string.IsNullOrEmpty(authString))
            {
                throw new ArgumentNullException(nameof(authString));
            }
            var auth = JsonConvert.DeserializeObject<AuthResult>(authString);
            return auth;
        }
    }
}
;