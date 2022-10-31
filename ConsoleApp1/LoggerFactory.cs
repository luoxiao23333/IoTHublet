using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IoTHublet
{
    internal class LoggerFactory
    {
        public static ILogger GetLogger<T>()
        {
            return Microsoft.Extensions.Logging.LoggerFactory.
                Create(builder => { builder.AddConsole(); }).CreateLogger(typeof(T).ToString());
        }

        public static ILogger GetLogger(string category)
        {
            return Microsoft.Extensions.Logging.LoggerFactory.
                Create(builder => { builder.AddConsole(); }).CreateLogger(category);
        }
    }
}
