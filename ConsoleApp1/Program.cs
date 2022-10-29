using System;

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
                Sensor.DL11MC_TemperatureSensor sensor = 
                    new Sensor.DL11MC_TemperatureSensor
                    (Configuration.Instance.DeviceName);

                sensor.Open();
                while(true)
                {
                    Console.WriteLine("Temp is {0}", sensor.GetTemperature()?? -2000);
                    string? k = Console.ReadLine();
                    if (k == "Q")
                    {
                        break;
                    }
                }
                sensor.Close();
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

