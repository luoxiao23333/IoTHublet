using System;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Net;
using System.Linq.Expressions;

namespace IoTHublet
{

    //Wanring: the firsr letter of field in yaml could not be upper case
    public static class Configuration
    {
        private static string configFileName = "IoTHublet.conf";

        public class Info
        {
            public string? DeviceName;
        }

        public static Info? Instance;

        static Configuration()
        {
            try
            {
                string yamlString = System.IO.File.ReadAllText(configFileName);

                var input = new StringReader(yamlString);
                var deserializer = new DeserializerBuilder()
                   .WithNamingConvention(CamelCaseNamingConvention.Instance)
                 .Build();

                Instance = deserializer.Deserialize<Info>(input);

                if (Instance == null)
                {
                    Console.WriteLine("Read config file {0} failed", configFileName);
                    throw new Exception("Config file read failed!");
                }
            }
            catch(FileNotFoundException e)
            {
                Console.WriteLine("Config File Not Found! Not Fond in {0}", e);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            

        }
    }
}
