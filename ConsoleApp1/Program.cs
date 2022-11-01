using System;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Message = Microsoft.Azure.Devices.Client.Message;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid.SystemEvents;
using YamlDotNet.Core.Tokens;

namespace IoTHublet
{
    class Program
    {
        private static Sensor.DL11MC_TemperatureSensor sensor = 
            new Sensor.DL11MC_TemperatureSensor(Configuration.Instance.DeviceName);

        private static IoTHubCommunicator ioTHubCommunicator =
            IoTHubCommunicator.CreateFromConnectionString(Configuration.Instance.IotHubConnectionString);

        private static ILogger logger = LoggerFactory.GetLogger<Program>();

        static void Main(string[] args)
        {
            if(sensor.Open()==false)
            {
                logger.LogCritical("sensor {0} open failed!", sensor.GetDeviceName());
            }
            logger.LogInformation("sensor {0} open successfully!", sensor.GetDeviceName());

            //Warning: The first two samples always invalid, so discard them. Not know why
            sensor.GetTemperature();
            sensor.GetTemperature();

            Console.WriteLine(@"
IoTHublet Interaction Start!
Input 'S' to sample and send temperature to cloud
Input 'R' to try to receive a message from cloud in 10 seconds
Input 'Q' to exit!");

            while (true)
            {
                string? key = Console.ReadLine();
                if(key == null)
                {
                    continue;
                }
                else if(key == "S")
                {
                    float? temperature = sensor.GetTemperature();
                    if(temperature == null)
                    {
                        continue;
                    }
                    ioTHubCommunicator.SendMessage($"{{\"temperature\": {temperature}}}"); 
                }
                else if(key == "R")
                {
                    _ = ioTHubCommunicator.ReceiveMessage();
                }
                else if(key == "Q")
                {
                    break;
                }
            }

            sensor.Close();
        }


        private Program()
        {
        }
    }
}

