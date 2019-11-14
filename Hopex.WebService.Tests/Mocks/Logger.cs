using System;
using System.Diagnostics;
using Hopex.ApplicationServer.WebServices;

namespace Hopex.WebService.Tests.Mocks
{
    internal class Logger : ILogger
    {
        public void LogError(Exception ex, string msg = null)
        {
            if (msg == null)
            {
                LogInformation(ex.Message);
            }
            else
            {
                LogInformation(msg + " - " + ex.Message);
            }
        }

        public void LogInformation(string msg)
        {
            Trace.WriteLine(msg);
        }
    }
}
