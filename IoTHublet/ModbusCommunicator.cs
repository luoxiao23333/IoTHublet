using System.IO.Ports;
using System;
using IoTHublet;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace IoTHublet.SerialComm
{

    public static class CRC
    {
        public static bool Verify(byte[]checksum)
        {
            return ComputeCRCCode(checksum) == 0;
        }
        public static uint ComputeCRCCode(byte[] snd)
        {
            byte i, j;
            uint c, crc = 0xFFFF;
            for (i = 0; i < snd.Length; i++)
            {
                c = (uint)(snd[i] & 0x00FF);
                crc ^= c;
                for (j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return (ushort)crc;
        }
    }



    public static class ModbusCommunication
    {
        private static readonly ILogger logger = LoggerFactory.GetLogger(nameof(ModbusCommunication));

        //@brief put command via Modbus protocol to the hardware, and retrieve data
        //@temparam inputLength the byte length of command
        //@tamparam outputLength the byte length of retrieve data
        //@param command the byte command
        //@param fd file descriptor of the socket between hardware and system
        //@return the reply byte of the hardware
        public static byte[]? ModbusQuery(byte[] command, ref SerialPort port, int outputLength)
        {
            try
            {
                port.Write(command, 0, command.Length);

                byte[] outputs = new byte[outputLength];
                port.Read(outputs, 0, outputLength);
                
                if (CRC.Verify(outputs) == false)
                {
                    string logText = "Output Code is: ";
                    for (int i = 0; i < outputs.Length; ++i)
                    {
                        logText += $"{outputs[i]} ";
                    }
                    logger.LogInformation(logText);
                    throw new Exception("CRC Check failed!");
                }

                return outputs;

            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return null;
            }
        }
    }




}

