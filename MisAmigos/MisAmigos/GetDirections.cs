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
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using BingMapsRESTToolkit;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Text;

namespace MisAmigos
{
    
    public static class GetDirections
    {
        private static HttpClientWrapper httpClient = new HttpClientWrapper(
            new Uri(Environment.GetEnvironmentVariable("BingVirtualEarthUrl")));

 
        [FunctionName("GetDirections")]
        //public static async Task<IActionResult> Run(
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            bool validFromLat = double.TryParse(req.Query["FromLat"], out double fromLat);
            bool validFromLong = double.TryParse(req.Query["FromLong"], out double fromLong);
            bool validToLat = double.TryParse(req.Query["ToLat"], out double toLat);
            bool validToLong = double.TryParse(req.Query["ToLong"], out double toLong);

            string to = req.Query["To"];
            HttpClient client = new HttpClient();
            string BingMapsKey = Environment.GetEnvironmentVariable("BingMapsAPIKey");

            //string bingUrl = $"http://dev.virtualearth.net/REST/V1/Routes/Walking?wp.0={from}&wp.1={to}&optmz=distance&output=json&key={BingMapsAppKey}";

            var request = new RouteRequest()
            {
                RouteOptions = new RouteOptions()
                {
                    Avoid = new List<AvoidType>()
                    {
                        AvoidType.MinimizeTolls
                    },
                    TravelMode = TravelModeType.Driving,
                    DistanceUnits = DistanceUnitType.Miles,
                    Heading = 45,
                    RouteAttributes = new List<RouteAttributeType>()
                    {
                        RouteAttributeType.RoutePath
                    },
                    Optimize = RouteOptimizationType.TimeWithTraffic
                },
                Waypoints = new List<SimpleWaypoint>()
                {
                    new SimpleWaypoint(){
                        Coordinate = new Coordinate(){Latitude = fromLat, Longitude = fromLong}
                    },
                    //new SimpleWaypoint(){
                    //    Address = "Bellevue, WA",
                    //    IsViaPoint = true
                    //},
                    new SimpleWaypoint(){
                        Coordinate = new Coordinate(){Latitude = toLat, Longitude = toLong}
                    }
                },
                BingMapsKey = BingMapsKey
            };

            //Process the request by using the ServiceManager.
            var response = await ServiceManager.GetResponseAsync(request);
            List<string> routeSteps = new List<string>();
            if (response != null &&
                response.ResourceSets != null &&
                response.ResourceSets.Length > 0 &&
                response.ResourceSets[0].Resources != null &&
                response.ResourceSets[0].Resources.Length > 0)
            {
                var result = response.ResourceSets[0].Resources[0] as BingMapsRESTToolkit.Route;
                log.LogInformation($"Travel: {result.TravelDistance}{result.DistanceUnit} - {result.TravelDuration}{result.TimeUnitType}");
                foreach(var r in result.RouteLegs)
                {
                    foreach(var item in r.ItineraryItems)
                    {
                        log.LogInformation($"{item.Instruction.Text}");
                        routeSteps.Add(item.Instruction.Text);
                    }
                }
            }
            


            string jsonFormatted = Newtonsoft.Json.JsonConvert.SerializeObject(routeSteps, Newtonsoft.Json.Formatting.Indented);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonFormatted, Encoding.UTF8, "application/json")
            };
        }
    }
}
