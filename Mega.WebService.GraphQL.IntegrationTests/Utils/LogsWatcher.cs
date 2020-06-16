using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests.Utils
{
    class LogsWatcher
    {
        ITestOutputHelper _output;
        LogFileWatcher[] _fileWatchers;
        

        public LogsWatcher(ITestOutputHelper output)
        {
            
            _fileWatchers = new LogFileWatcher[]
            {
                new LogFileWatcher("megaerr.txt"),
                new LogFileWatcher("ssperr.log"),
                //new LogFileWatcher("Hopex-[Macro]-.log") // Writes are too delayed
            };
            _output = output;
        }

        internal void Reset()
        {
            foreach (var watcher in _fileWatchers)
                watcher.Reset();
        }

        internal async Task WriteNewLinesAsync()
        {            
            foreach (var watcher in _fileWatchers)
                await watcher.WriteNewLinesAsync(_output);
        }
    }
}
