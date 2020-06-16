using System.IO;

namespace Hopex.Modules.GraphQL
{
    public interface IFileSource
    {
        byte[] ReadAllBytes(string path);
        void Delete(string fileName);
    }

    internal class PhysicalFileSource : IFileSource
    {
        public void Delete(string fileName)
        {
            File.Delete(fileName);
        }

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }
    }
}
