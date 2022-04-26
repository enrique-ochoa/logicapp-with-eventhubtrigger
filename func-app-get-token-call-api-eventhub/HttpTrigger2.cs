using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Company.Function
{
    public static class HttpTrigger2
    {

        private static string Token = null;
        private static string output = null;
        private static int graphrecords;

        [FunctionName("HttpTrigger2")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Call API with Access Token function processed a request.");

            string cachedToken = req.Query["cachedToken"];
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            Token = cachedToken ?? data?.cachedToken;

            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            string responseMessage = string.IsNullOrEmpty(Token)
                ? "This HTTP triggered function executed successfully. Pass a token in the query string or in the request body for a personalized response."
//                : $"Token used to call API: {Token}. This HTTP triggered function executed successfully.";
                : $"MS Graph API returned {graphrecords} Users";

            return new OkObjectResult(responseMessage);
        }
        private static async Task RunAsync()
        {

//            AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

// Get API url to be invoked
            AuthenticationConfig config = new AuthenticationConfig();
            config.ApiUrl = Environment.GetEnvironmentVariable("ApiUrl");

// If there is a cached Access Token
            if (Token != null)
            {
                var httpClient = new HttpClient();
                var apiCaller = new ProtectedApiCallHelper(httpClient);
    // Invoke the MS Graph API
                await apiCaller.CallWebApiAndProcessResultASync($"{config.ApiUrl}v1.0/users", Token, Display);
            }
        }

        /// <summary>
        /// Display the result of the Web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private static void Display(JObject result)
        {
            //Console.WriteLine(result);
            Console.WriteLine("MS Graph returned count of " + result.Count + " users");
//            output+= "\n\n" + result;
            graphrecords=result.Count;
            output+= "Called Graph";
            //foreach (JProperty child in result.Properties().Where(p => !p.Name.StartsWith("@")))
            //{
            //    Console.WriteLine($"{child.Name} = {child.Value}");
            //}
        }
    }
}
