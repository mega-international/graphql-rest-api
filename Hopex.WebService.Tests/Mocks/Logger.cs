using System;
using System.Diagnostics;
using Hopex.ApplicationServer.WebServices;

namespace Hopex.WebService.Tests.Mocks
{
    internal class Logger : ILogger
    {
        public int InitMacroId(string macroName)
        {
            if(macroName == "")
            {
                return -1;
            }
            throw new NotImplementedException();
        }

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

        public void LogError(Exception ex, int macroId, string msg = null)
        {
            LogError(ex, msg);
        }

        public void LogInformation(string msg)
        {
            Trace.WriteLine(msg);
        }

        public void LogInformation(string msg, int macroId)
        {
            LogInformation(msg);
        }
    }
}
