using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTHublet
{
    internal class IoTHubCommunicator
    {
        private DeviceClient deviceClient;
        private ILogger receiverLogger = LoggerFactory.GetLogger("Cloud to Device Message");
        private ILogger senderlogger = LoggerFactory.GetLogger("Device to Cloud Message");

        private static readonly TimeSpan defaultReciveTimeout = TimeSpan.FromSeconds(10);

        IoTHubCommunicator(DeviceClient initClient)
        {
            deviceClient = initClient;
        }

        public static IoTHubCommunicator CreateFromConnectionString(string connectStr)
        {
            DeviceClient client = DeviceClient.CreateFromConnectionString(connectStr, TransportType.Mqtt);
            IoTHubCommunicator communicator = new IoTHubCommunicator(client);

            return communicator;
        }

        public string? ReceiveMessage()
        {
            receiverLogger.LogInformation("Start receive session");
            Message receivedMessage = deviceClient.ReceiveAsync(defaultReciveTimeout).Result;
            if (receivedMessage == null)
            {
                receiverLogger.LogInformation("Not receive message after waiting for {0}",
                    defaultReciveTimeout.ToString());
                return null;
            }
            string receivingString = Encoding.ASCII.GetString(receivedMessage.GetBytes());
            receiverLogger.LogInformation("Received message: {0}", receivingString);
            deviceClient.CompleteAsync(receivedMessage).Wait();
            receiverLogger.LogInformation("receive Done!");
            return receivingString;
        }

        public bool SendMessage(string message)
        {
            try
            {
                Encoding encoding = Encoding.UTF8;
                byte[] messageByte = encoding.GetBytes(message);
                senderlogger.LogInformation($"Sending {message}");
                deviceClient.SendEventAsync(new Message(messageByte)).Wait();
                senderlogger.LogInformation("Send Done!");
                return true;
            }
            catch (Exception e)
            {
                senderlogger.LogError(e.Message);
                return false;
            }
        }
    }
}
