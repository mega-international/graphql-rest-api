using FluentAssertions;
using Mega.WebService.GraphQL.IntegrationTests.Applications.Interfaces;
using MegaMapp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Mega.WebService.GraphQL.IntegrationTests.Utils
{
    internal class MgrImporter
    {
        static string _rejectFile = Path.GetTempFileName();
        private IRepository _megaDatabase;       

        public MgrImporter(IRepository megaDatabase)
        {
            _megaDatabase = megaDatabase;
        }

        public void ImportAll()
        {
            IEnumerable<string> mgrs;
            if (InterceptingTestFramework.RunAll)
            {
                mgrs = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .SelectMany(t => t.GetCustomAttributes<ImportMgrAttribute>())
                    .Select(a => a.File);
            }
            else
            {
                mgrs = InterceptingTestFramework.TestCases.Select(c => c.TestMethod.TestClass)
                    .Distinct(new TestCaseNameComparer())
                    .SelectMany(c => c.Class.GetCustomAttributes(typeof(ImportMgrAttribute).AssemblyQualifiedName))
                    .SelectMany(a => a.GetConstructorArguments())
                    .Cast<string>();
            }
            foreach (var mgr in mgrs)
                Import(mgr);
        }

        private void Import(string file)
        {
            System.Console.WriteLine($"Importing {file}...");
            var directory = Directory.GetCurrentDirectory();
            var fullPath = Path.Combine(directory, file);
            File.Exists(fullPath).Should().BeTrue();
            _megaDatabase.Import(fullPath, _rejectFile, "ServerMode=On");
        }
    }

    internal class TestCaseNameComparer : IEqualityComparer<ITestClass>
    {
        public bool Equals(ITestClass x, ITestClass y)
        {
            return x.Class.Name.Equals(y.Class.Name);
        }

        public int GetHashCode(ITestClass obj)
        {
            return obj.Class.Name.GetHashCode();
        }
    }
}
