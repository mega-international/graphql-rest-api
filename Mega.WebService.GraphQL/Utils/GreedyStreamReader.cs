using System.IO;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL
{
    public class GreedyStreamReader
    {
        private Stream _stream;

        public GreedyStreamReader(Stream stream)
        {
            _stream = stream;
        }

        public async Task<int> ReadAsync(byte[] buffer, int count)
        {
            var totalBytesRead = 0;
            int bytesRead;
            do
            {
                bytesRead = await _stream.ReadAsync(buffer, totalBytesRead, count - totalBytesRead);
                totalBytesRead += bytesRead;
            }
            while (totalBytesRead < count && bytesRead > 0);
            return totalBytesRead;
        }
    }
}
