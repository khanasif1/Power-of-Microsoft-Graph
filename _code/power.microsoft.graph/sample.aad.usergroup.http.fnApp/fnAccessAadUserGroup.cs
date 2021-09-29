using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using sample.graph.fnApp.config;
namespace sample.aad.usergroup.http.fnApp
{
    public static class fnHTTPAccessAadUser
    {
        [FunctionName("fnHTTPAccessAadUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            

            string groupResponse = string.Empty;            
            try
            {
                JObject _graphresponse = await GetAADUserAsync(log);
                groupResponse = _graphresponse.ToString();
            }
            catch (Exception ex)
            {

                log.LogInformation($" Error: {ex.Message}");
            }

            return new OkObjectResult(groupResponse.ToString());
        }
        private static async Task<JObject> GetAADUserAsync(ILogger log)
        {
            try
            {

                AuthenticationConfig config = new AuthenticationConfig();
                // Even if this is a console application here, a daemon application is a confidential client application
                IConfidentialClientApplication app;

                app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                    .WithClientSecret(config.ClientSecret)
                    .WithAuthority(new Uri(config.Authority))
                    .Build();

                // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
                // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
                // a tenant administrator. 
                string[] scopes = new string[] { $"{config.ApiUrl}.default" };

                JObject _destinationUseresult = null;

                AuthenticationResult result = null;
                try
                {
                    result = await app.AcquireTokenForClient(scopes)
                        .ExecuteAsync();
                }
                catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
                {
                    // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                    // Mitigation: change the scope to be as expected
                    log.LogInformation($" Scope provided is not supported for destination AAD instance");

                }
               
                if (result != null)
                {
                    var httpClient = new HttpClient();
                    var apiCaller = new ProtectedApiCallHelper(httpClient);
                    //Get all users from the destination AAD group for comparison with User with the AAD Source
                    _destinationUseresult = await apiCaller.CallWebApiAndProcessResultASync(
                                                               $"{config.ApiUrl}v1.0/users",                                                               
                                                               result.AccessToken,
                                                               log);
                   
                    log.LogInformation($" Graph API resuqest completed. ");
                }
                return _destinationUseresult;
            }
            catch (Exception ex)
            {
                log.LogError(ex, $" **ERROR IN SyncAADUserAsync***{ex.Message}");
                throw ex;
            }
        }
    }
}
