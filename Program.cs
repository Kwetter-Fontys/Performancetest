using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NBomber;
using NBomber.Configuration;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;
using NBomber.Plugins.Network.Ping;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace NBomberTest
{
    class Program
    {
        static string baseUserUrl = "https://user-service.sebananasprod.nl/api/usercontroller";
        static string baseTweetUrl = "https://tweet-service.sebananasprod.nl/api/tweetcontroller";
        static string baseKeycloakUrl = "https://keycloak.sebananasprod.nl/auth/realms/kwetter/protocol/openid-connect/token";
        static string userId = "bf40cabc-3cc7-49bb-aeba-cd1c6ab23dcc";
        static void Main(string[] args)
        {

            BasicStressTestUserService();
            //BasicStressTestTweetService();
            //StressTestPostTweet();
            //SimulateMultipleUsersGoingToTheStartPage();
        }

        public static string GetAccessToken()
        {
            var client = new RestClient(baseKeycloakUrl);
            var request = new RestRequest(baseKeycloakUrl, Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "password");
            request.AddParameter("client_id", "Kwetter-frontend");
            request.AddParameter("username", "sebas");
            request.AddParameter("password", "test");
            RestResponse response = client.ExecuteAsync(request).Result;

            var myJObject = JObject.Parse(response.Content);
            return (string)myJObject["access_token"];
        }

        public static void BasicStressTestUserService()
        {
            string Accesstoken = GetAccessToken();
            var step = Step.Create("simple get users", clientFactory: HttpClientFactory.Create(),
            execute: context =>
            {
                var request = Http.CreateRequest("GET", baseUserUrl).WithHeader("Authorization", "Bearer " + Accesstoken);
                return Http.Send(request, context);
            });


            var scenario = ScenarioBuilder
                .CreateScenario("simple get users", step)
                .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                .WithLoadSimulations(Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromSeconds(30)));


            NBomberRunner.RegisterScenarios(scenario).Run();
        }


        public static void BasicStressTestTweetService()
        {
            string Accesstoken = GetAccessToken();
            var step = Step.Create("simple get tweets", clientFactory: HttpClientFactory.Create(),
            execute: context =>
            {
                var request = Http.CreateRequest("GET", baseTweetUrl + "/" + userId).WithHeader("Authorization", "Bearer " + Accesstoken);
                return Http.Send(request, context);
            });


            var scenario = ScenarioBuilder
                .CreateScenario("simple get users", step)
                .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                .WithLoadSimulations(Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromSeconds(30)));


            NBomberRunner.RegisterScenarios(scenario).Run();
        }

        public static void StressTestPostTweet()
        {
            JsonContent tweet = JsonContent.Create(new { id = 1, content = "Tweet", user = "bf40cabc-3cc7-49bb-aeba-cd1c6ab23dcc", date = "2022-05-28T13:58:05" });
            string Accesstoken = GetAccessToken();
            var step = Step.Create("post a lot of tweets", clientFactory: HttpClientFactory.Create(),
            execute: context =>
            {
                var request = Http.CreateRequest("POST", baseTweetUrl).WithHeader("Authorization", "Bearer " + Accesstoken)
                .WithBody(tweet);
                return Http.Send(request, context);
            });


            var scenario = ScenarioBuilder
                .CreateScenario("post a lot of tweets", step)
                .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                .WithLoadSimulations(Simulation.KeepConstant(copies: 100, during: TimeSpan.FromSeconds(10)));


            NBomberRunner.RegisterScenarios(scenario).WithReportFolder("TweetsPostTest")
                   .WithReportFormats(ReportFormat.Txt, ReportFormat.Csv, ReportFormat.Html, ReportFormat.Md).Run();
        }

        public static void SimulateMultipleUsersGoingToTheStartPage()
        {
                var httpFactory = ClientFactory.Create(
                name: "http_factory",
                clientCount: 5,
                // we need to init our client with our API token
                initClient: (number, context) =>
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                    "Bearer",
                    GetAccessToken());
                    return Task.FromResult(client);
                });

            var loadUserData = Step.Create("GetUserByUserId", clientFactory: httpFactory,
            execute: async context =>
            {
                var response = await context.Client.GetAsync(baseUserUrl + "/" + userId);
                return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: (int)response.StatusCode)
                : Response.Fail(statusCode: (int)response.StatusCode);
            });
           var loadUserTweets = Step.Create("GetTweetsByUserId", clientFactory: httpFactory,
            execute: async context =>
            {
                var response = await context.Client.GetAsync(baseTweetUrl + "/" + userId);
                return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: (int)response.StatusCode)
                : Response.Fail(statusCode: (int)response.StatusCode);
            });

            var loadUserFollowers = Step.Create("GetFollowersByUserId", clientFactory: httpFactory,
             execute: async context =>
             {
                 var response = await context.Client.GetAsync(baseUserUrl + "/followers/" + userId);
                 return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: (int)response.StatusCode)
                : Response.Fail(statusCode: (int)response.StatusCode);
             });

            var loadUserFollowings = Step.Create("GetFollowingsByUserId", clientFactory: httpFactory,
             execute: async context =>
             {
                 var response = await context.Client.GetAsync(baseUserUrl + "/followings/" + userId);
                 return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: (int)response.StatusCode)
                : Response.Fail(statusCode: (int)response.StatusCode);
             });

            var scenario = ScenarioBuilder
                .CreateScenario("Simulate user page requests", loadUserData, loadUserTweets, loadUserFollowers, loadUserFollowings)
                .WithWarmUpDuration(TimeSpan.FromSeconds(10))
                .WithLoadSimulations(new[]
                {
                    // from the nBomber docs:
                    // It's to model an open system.
                    // Injects a random number of scenario copies (threads) per 1 sec 
                    // defined in scenarios per second during a given duration.
                    // Every single scenario copy will run only once.
                    // Use it when you want to maintain a random rate of requests
                    // without being affected by the performance of the system under test.
                    Simulation.InjectPerSecRandom(minRate: 20, maxRate: 50, during: TimeSpan.FromMinutes(30))
                });


            NBomberRunner.RegisterScenarios(scenario).WithReportFolder("UserFlowPerformanceLong")
                   .WithReportFormats(ReportFormat.Txt, ReportFormat.Csv, ReportFormat.Html, ReportFormat.Md).Run();
        }
    }
}