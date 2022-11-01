using System.IO.Ports;
using System;
using IoTHublet;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using IoTHublet.SerialComm;

namespace IoTHublet.Sensor
{
    public class DL11MC_TemperatureSensor
    {
        private byte[] read1TempratureCommand = { 0x01, 0x04, 0x04, 0x00, 0x00, 0x01, 0x30, 0xFA };
        private const int buad = 9600;
        private const int outputLength = 7;
        private SerialPort port;

        private static readonly ILogger logger = LoggerFactory.GetLogger<DL11MC_TemperatureSensor>();

        public DL11MC_TemperatureSensor(string? deviceName)
        {
            if(deviceName == null)
            {
                logger.LogError("Device name is Null");
                throw new Exception("Device name is null!");
            }
            else
            {
                port = new SerialPort(deviceName, buad);
            }
            
        }

        public bool Open()
        {
            port.Open();
            return port.IsOpen;
        }

        public void Close()
        {
            port.Close();
        }

        //@Warning This method will wait sensor until it receive temperature info
        //So you should create a thread to run it
        public float? GetTemperature()
        {

            byte[]? outputs = 
                SerialComm.ModbusCommunication.ModbusQuery(read1TempratureCommand, ref port, outputLength);
            if(outputs == null)
            {
                logger.LogError($"Read device {port.PortName} failed!");
                return null;
            }
            return computeTemperature(outputs[3], outputs[4]);
        }

        public string GetDeviceName()
        {
            return port.PortName;
        }

        private float? computeTemperature(byte highByte, byte lowByte)
        {
            if (highByte == 0x7F && lowByte == 0xFF)
            {
                logger.LogError("Temprature Sensor may broke or non-exist!");
                return null;
            }

            float temperature = (highByte * 256 + lowByte) / (float)10;

            if (temperature >= 200)
            {
                temperature = (((highByte * 256 + lowByte) - 0xFFFF - 0x01) / (float)10);
            }

            return temperature;
        }

    }
}
