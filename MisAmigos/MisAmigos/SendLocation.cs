using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MisAmigos.Helpers;
using System.Xml.Linq;
using System.Net.Http;
using BingMapsRESTToolkit;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace MisAmigos
{
    
    public static class SendLocation
    {
        private static HttpClientWrapper httpClient = new HttpClientWrapper(
            new Uri(Environment.GetEnvironmentVariable("BingVirtualEarthUrl")));

        [FunctionName("SendLocation")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string from = req.Query["From"];
            string to = req.Query["To"];
            HttpClient client = new HttpClient();
            string BingMapsAppKey = Environment.GetEnvironmentVariable("BingMapsAPIKey");
            string bingUrl = $"http://dev.virtualearth.net/REST/V1/Routes/Walking?wp.0={from}&wp.1={to}&optmz=distance&output=json&key={BingMapsAppKey}";

            client.BaseAddress = new Uri(bingUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Response bingResponse = await httpClient.GetAsync<Response>(new Uri(bingUrl));

            //dynamic root = JsonConvert.DeserializeObject(response);
            foreach (ResourceSet r in bingResponse.ResourceSets)
            {
                long estTotal = r.EstimatedTotal;
                log.LogInformation(estTotal.ToString());
                
                foreach (Resource resource in r.Resources)
                {
                    Route route = resource as Route;

                    log.LogInformation($"Total Distance: {route.TravelDistance}{route.DistanceUnitType} TotalTime: {route.TravelDuration}{route.DistanceUnit}");
                    RouteLeg[] routeLegs = route.RouteLegs;
                    foreach (RouteLeg leg in routeLegs)
                    {
                        log.LogInformation($"{leg.StartLocation} {leg.EndLocation}");
                        foreach (ItineraryItem itin in leg.ItineraryItems)
                        {
                            log.LogInformation($"{itin.TowardsRoadName}");
                        }
                    }

                }
            }

            return from != null
                ? (ActionResult)new OkObjectResult($"Hello, {from}")
                : new BadRequestObjectResult("Please provide a valid destination");
        }
    }
}
