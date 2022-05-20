using System;

using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;
using NBomber.Plugins.Network.Ping;

namespace NBomberTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var step = Step.Create("fetch_html_page",
                                   clientFactory: HttpClientFactory.Create(),
                                   execute: context =>
                                   {
                                       var request = Http.CreateRequest("GET", "https://user-service.sebananasprod.nl/api/usercontroller")
                                                         .WithHeader("Authorization", "Bearer eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICI4UC04eXpYUmhOaUpiUU84QXhxVXg2SW1icDBZZVg3Tm1ueF9rWmV5bjFvIn0.eyJleHAiOjE2NTMwNzI4MTQsImlhdCI6MTY1MzA2OTIxNCwiYXV0aF90aW1lIjoxNjUzMDY5MjE0LCJqdGkiOiI1MjVhZGM4NC1mMjMxLTQxYzktYmQ2Mi1mNWE1NWYyMDdhNzYiLCJpc3MiOiJodHRwczovL2tleWNsb2FrLnNlYmFuYW5hc3Byb2QubmwvYXV0aC9yZWFsbXMvS3dldHRlciIsImF1ZCI6ImFjY291bnQiLCJzdWIiOiI3Y2MzNWZjNi0wZWFmLTRkZjgtYWFlZi03NzMwNzdiNGYzYzkiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJLd2V0dGVyLWZyb250ZW5kIiwic2Vzc2lvbl9zdGF0ZSI6IjJjZjBkNmUyLTdiMmEtNGI4Mi04OWUxLTNjOWQ0NTQ5NGQ5NiIsImFjciI6IjEiLCJhbGxvd2VkLW9yaWdpbnMiOlsiaHR0cDovL2xvY2FsaG9zdDo0MjAwL3Byb2ZpbGUiLCJodHRwczovL2t3ZXR0ZXIuc2ViYW5hbmFzcHJvZC5ubCIsImh0dHBzOi8va3dldHRlci5zZWJhbmFuYXNwcm9kLm5sLyoiLCJodHRwOi8vbG9jYWxob3N0OjQyMDAvKiIsImh0dHBzOi8va2V5Y2xvYWsuc2ViYW5hbmFzcHJvZC5ubC8qIiwiaHR0cDovL2xvY2FsaG9zdDo0MjAwIiwiaHR0cHM6Ly9rd2V0dGVyLnNlYmFuYW5hc3Byb2QubmwvIiwiaHR0cHM6Ly9rZXljbG9hay5zZWJhbmFuYXNwcm9kLm5sIl0sInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJvZmZsaW5lX2FjY2VzcyIsImRlZmF1bHQtcm9sZXMta3dldHRlciIsInVtYV9hdXRob3JpemF0aW9uIl19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJwcm9maWxlIGVtYWlsIiwic2lkIjoiMmNmMGQ2ZTItN2IyYS00YjgyLTg5ZTEtM2M5ZDQ1NDk0ZDk2IiwiZW1haWxfdmVyaWZpZWQiOmZhbHNlLCJuYW1lIjoic2ViYXMgYmFra2VyIiwicHJlZmVycmVkX3VzZXJuYW1lIjoic2ViYXMiLCJnaXZlbl9uYW1lIjoic2ViYXMiLCJmYW1pbHlfbmFtZSI6ImJha2tlciIsImVtYWlsIjoic2ViYXNiYWtrZXI4QGhvdG1haWwuY29tIn0.CPTJXmFeh9BGHhwP1D3VXjuPBiF8IVOfuuu26qY595r4kp5eMr_4smwwXpPuK0GRZ4Gu81xjprz9rvdv9OF3wpYyX3yoEVi56MQSt2c454AcWPS7KFmx4ssVhheYNnFcYNVImgwm2DCFhB5m5q6G_Dhlj_Og1YEWbxTNGfp6BQRyJLYX54OLTwkvfp1CZvCQszXl1ca_SFgFKnkWV-oD-qOlNpZ6wGzGBnOdeYuomtZj9myVBSphubQkn31ZPutDVja-faqrch5pPqa1hQGqLsAtmaEU3pVoP3ViI5TrjLZpeWWPFhFJjVtikOEhj_WIusKcuZxf10JGxczFtoydug");
                                       ;

                                       return Http.Send(request, context);
                                   });


            var scenario = ScenarioBuilder
                .CreateScenario("simple_http", step)
                .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                .WithLoadSimulations(
                    Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromSeconds(30))
                );

            // creates ping plugin that brings additional reporting data
            var pingPluginConfig = PingPluginConfig.CreateDefault(new[] { "https://user-service.sebananasprod.nl/api/usercontroller" });
            var pingPlugin = new PingPlugin(pingPluginConfig);

            NBomberRunner
                .RegisterScenarios(scenario)
                .WithWorkerPlugins(pingPlugin)
                .Run();
        }
    }
}