using Hopex.ApplicationServer.WebServices;
using System;
using Xunit.Abstractions;

namespace Hopex.WebService.Tests.Mocks
{
    /* Warning: will fail violently if WriteLine is called when the test corresponding
        to the ITestOutpuHelper instance is completed.
        SchemaBuilder cache its logger instance so it can cause a lot of problem
    */
    class XUnitLogger : ILogger
    {
        private ITestOutputHelper _output;

        internal XUnitLogger(ITestOutputHelper output)
        {
            _output = output;
        }

        public int InitMacroId(string macroName)
        {
            if (macroName == "")
            {
                return -1;
            }
            throw new NotImplementedException();
        }

        public void LogError(Exception ex, string msg = null)
        {
            _output.WriteLine(ex.ToString());
            if (msg != null) _output.WriteLine(msg);
        }

        public void LogError(Exception ex, int macroId, string msg = null)
        {
            LogError(ex, msg);
        }

        public void LogInformation(string msg)
        {
            _output.WriteLine(msg);
        }

        public void LogInformation(string msg, int macroId)
        {
            LogInformation(msg);
        }
    }
}
