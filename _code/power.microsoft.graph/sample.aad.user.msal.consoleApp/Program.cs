using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
//https://briantjackett.com/2018/12/13/introduction-to-calling-microsoft-graph-from-a-c-net-core-application/
//https://github.com/microsoftgraph/dotnetcore-console-sample/blob/main/base-console-app/Program.cs
namespace ConsoleGraphTest
{
    class Program
    {
        private static GraphServiceClient _graphServiceClient;

        static void Main(string[] args)
        {
            // Load appsettings.json
            var config = LoadConfig.LoadAppSettings();
            if (null == config)
            {
                Console.WriteLine("Missing or invalid appsettings.json file. Please see README.md for configuration instructions.");
                return;
            }
            //Query using Graph SDK (preferred when possible)
            GraphServiceClient graphClient = GetAuthenticatedGraphClient(config);     


            var AADUserResult = graphClient.Users.Request().GetAsync().Result;

            Console.ForegroundColor = ConsoleColor.Red;            
            Console.WriteLine("========Graph APi AAD Users==========");

            foreach (User data in AADUserResult)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{data.GivenName} - {data.UserPrincipalName}");
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("=====================================");


            

            Console.ForegroundColor = ConsoleColor.White;
        }

        private static GraphServiceClient GetAuthenticatedGraphClient(IConfigurationRoot config)
        {
            var authenticationProvider = CreateAuthorizationProvider(config);
            _graphServiceClient = new GraphServiceClient(authenticationProvider);
            return _graphServiceClient;
        }


        private static IAuthenticationProvider CreateAuthorizationProvider(IConfigurationRoot config)
        {
            var clientId = config["applicationId"];
            var clientSecret = config["applicationSecret"];
            var redirectUri = config["redirectUri"];
            var authority = $"https://login.microsoftonline.com/{config["tenantId"]}/v2.0";

            //this specific scope means that application will default to what is defined in the application registration rather than using dynamic scopes
            List<string> scopes = new List<string>();
            scopes.Add("https://graph.microsoft.com/.default");

            var cca = ConfidentialClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .WithRedirectUri(redirectUri)
                                                    .WithClientSecret(clientSecret)
                                                    .Build();
            return new MsalAuthenticationProvider(cca, scopes.ToArray());
        }
    }
}
