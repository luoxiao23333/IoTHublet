using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Message = Microsoft.Azure.Devices.Message;
using DotNetty.Common.Utilities;

namespace DTToIoTHub
{
    public static class DTToIoTHub
    {
        [FunctionName("DTToIoTHub")]
        public static async Task Run([EventHubTrigger("dtiothubeventhub", Connection = "EventHubConnectionString")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    // Replace these two lines with your processing logic.
                    log.LogInformation($"C# Event Hub trigger function processed a message: {eventData.EventBody.ToString()}");

                    ServiceClient serviceClient =
                        ServiceClient.CreateFromConnectionString(
                            "HostName=IoTHubForPCL.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=ri054JKq+nvJ3CW2AZ9cyVWY6xzsgmOD68kxf1/CBFY=");

                    byte[] byteMessage = Encoding.ASCII.GetBytes($"Receive update from DT: {eventData.EventBody.ToString()}");
                    Message message = new Message(byteMessage);

                    await serviceClient.SendAsync("233", message);
                    message.Dispose();
                    await serviceClient.CloseAsync();

                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
