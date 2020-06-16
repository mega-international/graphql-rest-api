using FluentAssertions;
using MegaMapp;
using System;
using System.IO;

namespace Mega.WebService.GraphQL.IntegrationTests.Utils
{
    public class MgrImporter
    {
        static string _rejectFile = Path.GetTempFileName();
        private MegaDatabase _megaDatabase;       

        public MgrImporter(MegaDatabase megaDatabase)
        {
            _megaDatabase = megaDatabase;
        }

        public virtual void Import(string file)
        {
            var directory = Directory.GetCurrentDirectory();
            var fullPath = Path.Combine(directory, file);
            File.Exists(fullPath).Should().BeTrue();
            _megaDatabase.Import(fullPath, _rejectFile, "ServerMode=On");
        }
    }

    public class NullMgrImporter : MgrImporter
    {
        public NullMgrImporter() : base(null)
        {
        }

        public override void Import(string file)
        {
            Console.WriteLine($"Skipping import of {file}");
        }
    }
}
