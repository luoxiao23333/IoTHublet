using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using System;
using Azure;
using System.Net.Http;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace IoTHubToDT
{
    public class IoTtoDT
    {
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");

        [FunctionName("IoTtoDT")]
#pragma warning disable AZF0001 // Suppress async void error
        public async void Run([IoTHubTrigger("iothub-ehub-iothubforp-22341088-e04a89a1aa", Connection = "IotHubEventHubString")] Azure.Messaging.EventHubs.EventData message, ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {message.Data.ToString()}");
            var deviceId = message.SystemProperties["iothub-connection-device-id"].ToString();

            if (adtInstanceUrl == null) log.LogError("Application setting \"ADT_SERVICE_URL\" not set");

            try
            {
            
                // Authenticate with Digital Twins
                var cred = new DefaultAzureCredential();
                var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), cred);
                log.LogInformation($"ADT service client connection created.");

                if (message != null)
                {
                    log.LogInformation(message.Body.ToString());

                    // <Find_device_ID_and_temperature>
                    JObject deviceMessage = (JObject)JsonConvert.DeserializeObject(message.Data.ToString());
                    double temperature = deviceMessage["temperature"].Value<double>();
                    // </Find_device_ID_and_temperature>

                    log.LogInformation($"Device:{deviceId} Temperature is:{temperature}");

                    // <Update_twin_with_device_temperature>
                    var updateTwinData = new JsonPatchDocument();
                    updateTwinData.AppendReplace("/Temperature", temperature);
                    await client.UpdateDigitalTwinAsync(deviceId, updateTwinData);
                    // </Update_twin_with_device_temperature>
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error in ingest function: {ex.Message}");
            }
        }
    }
}