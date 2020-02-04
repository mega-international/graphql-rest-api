using FluentAssertions;
using Mega.WebService.GraphQL.V3.UnitTests.Assertions;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mega.WebService.GraphQL.V3.UnitTests
{
    public class GreedyStreamReader_should
    {
        [Fact]
        public async Task Read_all_at_once_if_small_enough()
        {
            var fs = new FragmentStream("whatever");
            var gbs = new GreedyStreamReader(fs);
            byte[] buffer = new byte[16];
            var bytesRead = await gbs.ReadAsync(buffer, 16);
            buffer.Should().Be("whatever");
            bytesRead.Should().Be(8);
        }

        [Theory]
        [InlineData("even", "more")]
        [InlineData("even", "more", "data")]
        public async Task Call_underlying_stream_multiple_times_to_fill_buffer(params string[] content)
        {
            var expected = string.Join("", content);
            var fs = new FragmentStream(content);
            var gbs = new GreedyStreamReader(fs);
            byte[] buffer = new byte[expected.Length];
            var bytesRead = await gbs.ReadAsync(buffer, expected.Length);
            buffer.Should().Be(expected);
            bytesRead.Should().Be(expected.Length);
        }

        [Fact]
        public async Task Do_not_read_more_than_its_buffer()
        {
            var fs = new FragmentStream("whatever");
            var gbs = new GreedyStreamReader(fs);
            byte[] buffer = new byte[4];
            var bytesRead = await gbs.ReadAsync(buffer, 4);
            buffer.Should().Be("what");
            bytesRead.Should().Be(4);
        }

        [Fact]
        public async Task Do_not_read_more_than_its_buffer_even_when_calling_stream_multiple_times()
        {
            var fs = new FragmentStream("1234", "5678", "90");
            var gbs = new GreedyStreamReader(fs);
            byte[] buffer = new byte[6];
            var bytesRead = await gbs.ReadAsync(buffer, 6);
            buffer.Should().Be("123456");
            bytesRead.Should().Be(6);
        }
    }


    public class FragmentStream : Stream
    {
        string[] _strings;

        public FragmentStream(params string[] strings)
        {
            _strings = strings;
        }

        public override bool CanRead => true;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_strings.Length == 0) return 0;

            var lineBytes = Encoding.UTF8.GetBytes(_strings[0]);
            var bytesToRead = Math.Min(lineBytes.Length, count);
            Array.Copy(lineBytes, 0, buffer, offset, bytesToRead);
            _strings = _strings.Skip(1).ToArray();
            return bytesToRead;
        }

        #region Not Implemented
        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        #endregion
    }    
}
