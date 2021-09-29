using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using sample.graph.fnApp.config;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using sample.aad.usergroup.http.fnApp;

namespace sample.aad.user.http.fnApp
{
    public static class fnAccessOneDrive
    {
        [FunctionName("fnAccessOneDrive")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string token = req.Query["token"];            
            var httpClient = new HttpClient();
            var apiCaller = new ProtectedApiCallHelper(httpClient);           
            
            var drives= await apiCaller.CallWebApiAndProcessResultASync(
                                                       $"https://graph.microsoft.com/v1.0/me/drive/root/children",
                                                       token,
                                                       log); 
            return new OkObjectResult(drives);
        } 
    }
}
