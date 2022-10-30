using System;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Message = Microsoft.Azure.Devices.Client.Message;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

namespace IoTHublet
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program();
        }
        
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
                    new Sensor.DL11MC_TemperatureSensor
                    (Configuration.Instance.DeviceName);

                sensor.Open();
                while(true)
                {
                    float? t = sensor.GetTemperature();
                    Console.WriteLine("Temp is {0}", t?? -2000);
                    if(t != null)
                    {


                        Encoding encoding = Encoding.UTF8;
                        string text = "{\"temperature\": " + t + " }";
                        byte[] toSend = encoding.GetBytes(text);

                        Console.WriteLine(text);

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
                Console.WriteLine("File not found {0}", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}

