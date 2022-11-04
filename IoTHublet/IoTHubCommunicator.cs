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

        public delegate Task ReceiveMessageHandler(string message); 

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

        public async Task ListenReceiverAsync(ReceiveMessageHandler callback)
        {
            await Task.Run(() =>
            {
                receiverLogger.LogInformation("Start receive listening session");
                while (true)
                {
                    Message receivedMessage = deviceClient.ReceiveAsync().Result;
                    if (receivedMessage == null)
                    {
                        receiverLogger.LogInformation("Not receive message!");
                        continue;
                    }
                    string receivingString = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    receiverLogger.LogInformation("Received message: {0}", receivingString);
                    callback(receivingString).Wait();
                    deviceClient.CompleteAsync(receivedMessage).Wait();
                    receiverLogger.LogInformation("Message Process Done!");
                }
            });
            return;
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            return await Task.Run(bool () =>
            {
                try
                {
                    Encoding encoding = Encoding.UTF8;
                    byte[] messageByte = encoding.GetBytes(message);
                    senderlogger.LogInformation($"Sending {message}");
                    deviceClient.SendEventAsync(new Message(messageByte)).Wait();
                    senderlogger.LogInformation($"Send {message} Done!");
                    return true;
                }
                catch (Exception e)
                {
                    senderlogger.LogError(e.Message);
                    return false;
                }
            });
        }

        public async Task CloseAsync()
        {
            await deviceClient.CloseAsync();
        }
    }
}
