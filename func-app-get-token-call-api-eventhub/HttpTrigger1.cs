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
using Microsoft.Identity.Web;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Company.Function
{
    public static class HttpTrigger1
    {
        private static IConfidentialClientApplication app = null;
        private static string CachedToken = null;
        private static DateTime CachedTokenExpiration;
        private static string output = null;

        [FunctionName("HttpTrigger1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            output = "";
            log.LogInformation("Get Access Token function processed a request.");

            try
            {
                RunAsync().GetAwaiter().GetResult();
//            return new OkObjectResult(responseMessage);
                return (ActionResult)new OkObjectResult(new { CachedToken });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
//            Forcing a function forbidden response
//            return new UnauthorizedObjectResult(responseMessage);
//            return (ActionResult)new UnauthorizedObjectResult(new { ex.Message });

//            Forcing a function server 500 response
                return new StatusCodeResult(500);
            }
        }

        private static async Task RunAsync()
        {

//            AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

// Get Client Credentials
            AuthenticationConfig config = new AuthenticationConfig();
            config.ClientId = Environment.GetEnvironmentVariable("CLIENT_ID");
            config.ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
            config.Tenant = Environment.GetEnvironmentVariable("TENANT");
            config.ApiUrl = Environment.GetEnvironmentVariable("ApiUrl");

// If a confidential client has not been created yet
            if (app == null)
            {
                // You can run this sample using ClientSecret or Certificate. The code will differ only when instantiating the IConfidentialClientApplication
                bool isUsingClientSecret = AppUsesClientSecret(config);

                if (isUsingClientSecret)
                {
    // Create a Confidential Client
                    app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                        .WithClientSecret(config.ClientSecret)
                        .WithAuthority(new Uri(config.Authority))
                        .Build();
                }
            }

// Calculating Token Expiration
            TimeSpan ts = CachedTokenExpiration - DateTime.Now; 
            AuthenticationResult result = null;

// If an Access Token has not been created or if it is about to expire
            if (CachedToken == null ||
                ts.TotalMinutes<57) 
            {
                try
                {
    // Acquire a new Access Token
                    string[] scopes = new string[] { $"{config.ApiUrl}.default" }; 
                    result = await app.AcquireTokenForClient(scopes)
                        .ExecuteAsync();
                    Console.ForegroundColor = ConsoleColor.Green;

                    TimeZoneInfo systemTimeZone = TimeZoneInfo.Local;
                    if(CachedToken == result.AccessToken)
                    {
                        Console.WriteLine("Token reused from cache, expires at " + CachedTokenExpiration);
                        output+="Token used from cache, expires at " + CachedTokenExpiration;
                    }
                    else
                    {
                        Console.WriteLine("Token acquired from IdP, expires at " + TimeZoneInfo.ConvertTimeFromUtc(result.ExpiresOn.DateTime, systemTimeZone));
                        output+="Token acquired, expires at " + TimeZoneInfo.ConvertTimeFromUtc(result.ExpiresOn.DateTime, systemTimeZone);
                    }

    // Cache the new Access Token and Expiration
                    if (result != null)
                    {    
                        CachedToken = result.AccessToken;
                        CachedTokenExpiration = TimeZoneInfo.ConvertTimeFromUtc(result.ExpiresOn.DateTime, systemTimeZone);
                    }

                    Console.ResetColor();
                }
                catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
                {
                    // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                    // Mitigation: change the scope to be as expected
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Scope provided is not supported");
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Checks if the sample is configured for using ClientSecret or Certificate. This method is just for the sake of this sample.
        /// You won't need this verification in your production application since you will be authenticating in AAD using one mechanism only.
        /// </summary>
        private static bool AppUsesClientSecret(AuthenticationConfig config)
        {
            string clientSecretPlaceholderValue = "[Enter here a client secret for your application]";
            string certificatePlaceholderValue = "[Or instead of client secret: Enter here the name of a certificate (from the user cert store) as registered with your application]";

            if (!String.IsNullOrWhiteSpace(config.ClientSecret) && config.ClientSecret != clientSecretPlaceholderValue)
            {
                return true;
            }

            else if (!String.IsNullOrWhiteSpace(config.CertificateName) && config.CertificateName != certificatePlaceholderValue)
            {
                return false;
            }

            else
                throw new Exception("You must choose between using client secret or certificate. Please update appsettings.json file.");
        }
    }
}
