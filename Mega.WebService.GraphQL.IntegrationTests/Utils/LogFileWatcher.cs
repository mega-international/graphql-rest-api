using System;
using System.IO;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests.Utils
{
    internal class LogFileWatcher
    {
        private readonly string _path;
        private long _lastLength;        

        public LogFileWatcher(string fileName)
        {
            string todaySuffix = DateTime.Today.Date.ToString("yyyyMMdd");
            var ext = Path.GetExtension(fileName).Replace(".","");
            _path = $@"C:\ProgramData\MEGA\Logs\{Path.GetFileNameWithoutExtension(fileName)}{todaySuffix}.{ext}";            
        }

        internal void Reset()
        {
            var info = new FileInfo(_path);
            if (info.Exists)
                _lastLength = info.Length;
        }

        internal async Task WriteNewLinesAsync(ITestOutputHelper output)
        {
            var info  = new FileInfo(_path);
            if (info.Exists)
            {
                var newLength = info.Length;
                if (newLength > _lastLength)
                {
                    output.WriteLine(_path);
                    using (var streamReader = new StreamReader(_path))
                    {
                        streamReader.BaseStream.Seek(_lastLength, SeekOrigin.Begin);
                        var newLines = await streamReader.ReadToEndAsync();
                        output.WriteLine(newLines);
                    }
                    output.WriteLine("");
                }
            }
        }
    }
}
