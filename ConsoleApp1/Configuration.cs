﻿using System;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Net;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace IoTHublet
{

    //Wanring: the firsr letter of field in yaml could not be upper case
    public static class Configuration
    {
        private static string configFileName = "IoTHublet.conf";

        private static readonly ILogger logger = LoggerFactory.GetLogger(nameof(Configuration));

        public class Info
        {
            public string DeviceName;
            public string IotHubConnectionString;
        }

        public static Info Instance;

        // Instance can never be null, since null will invike LogCritical, that quit the process
#pragma warning disable CS8618
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
                    logger.LogCritical("Read config file {0} failed", configFileName);
                    throw new Exception("Config file read failed!");
                }
            }
            catch(FileNotFoundException e)
            {
                logger.LogCritical("Config File Not Found! Not Fond in {0}", e);
            }
            catch(Exception e)
            {
                logger.LogCritical(e.Message);
            }
        }
    }
}
