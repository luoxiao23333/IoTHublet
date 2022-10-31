using System;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Message = Microsoft.Azure.Devices.Client.Message;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

using Microsoft.Extensions.Logging;

namespace IoTHublet
{
    class Program
    {
        private async static Task receiveMessage()
        {
            ILogger logger = LoggerFactory.GetLogger("C2D Message");
            logger.LogInformation("Start listening cloud to device messages from service");
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString("HostName=IoTHubForPCL.azure-devices.net;DeviceId=233;SharedAccessKey=jDhmMzal2ZZuZ3pkhWgKqAij1+0DhfMe8L8ltb84iyM=");

            while (true)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                logger.LogInformation("Received message: {0}",
                Encoding.ASCII.GetString(receivedMessage.GetBytes()));

                await deviceClient.CompleteAsync(receivedMessage);
            }
        }
        static void Main(string[] args)
        {
            receiveMessage();
            new Program();
        }

        private readonly ILogger logger = LoggerFactory.GetLogger<Program>();


        private Program()
        {
            try
            {
                string connctionStr = "HostName=IoTHubForPCL.azure-devices.net;DeviceId=233;SharedAccessKey=jDhmMzal2ZZuZ3pkhWgKqAij1+0DhfMe8L8ltb84iyM=";

                DeviceClient deviceClient = DeviceClient.
                    CreateFromConnectionString(connctionStr, TransportType.Mqtt);

                if (Configuration.Instance == null)
                {
                    throw new Exception("Configuraiton init failed!");
                }
                Sensor.DL11MC_TemperatureSensor sensor = 
                    new Sensor.DL11MC_TemperatureSensor(Configuration.Instance.DeviceName);

                sensor.Open();
                while(true)
                {
                    float? t = sensor.GetTemperature();
                    logger.LogInformation($"Temperature is {(t == null ? "Temperature Invalid!" : t)}");
                    if(t != null)
                    {
                        Encoding encoding = Encoding.UTF8;
                        string text = $"{{\"temperature\": {t}}}";
                        byte[] toSend = encoding.GetBytes(text);
                        logger.LogInformation(text);

                        deviceClient.SendEventAsync(new Message(toSend));
                    }
                    string? k = Console.ReadLine();
                    if (k == "Q")
                    {
                        break;
                    }
                }
                sensor.Close();
                deviceClient.CloseAsync();
            }catch(FileNotFoundException e)
            {
                logger.LogError($"File not found {e.Message}");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
        }
    }
}

