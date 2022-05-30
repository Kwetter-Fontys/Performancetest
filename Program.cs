using System;
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
        static void Main(string[] args)
        {
            StressTestPostTweet();
        }

        public static string GetAccessToken()
        {
            var client = new RestClient("https://keycloak.sebananasprod.nl/auth/realms/kwetter/protocol/openid-connect/token");
            var request = new RestRequest("https://keycloak.sebananasprod.nl/auth/realms/kwetter/protocol/openid-connect/token", Method.Post);
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
                var request = Http.CreateRequest("GET", "https://user-service.sebananasprod.nl/api/usercontroller").WithHeader("Authorization", "Bearer " + Accesstoken);
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
                var request = Http.CreateRequest("POST", "https://tweet-service.sebananasprod.nl/api/tweetcontroller").WithHeader("Authorization", "Bearer " + Accesstoken)
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
    }
}