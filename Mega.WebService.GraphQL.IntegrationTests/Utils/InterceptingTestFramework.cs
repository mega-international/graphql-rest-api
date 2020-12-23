using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestFramework("Mega.WebService.GraphQL.IntegrationTests.Utils.InterceptingTestFramework", "Mega.WebService.GraphQL.IntegrationTests")]

namespace Mega.WebService.GraphQL.IntegrationTests.Utils
{
    public class InterceptingTestFramework : XunitTestFramework
    {
        public static bool RunAll { get; private set; } = false;
        public static List<ITestCase> TestCases { get; private set; } = new List<ITestCase>();

        public InterceptingTestFramework(IMessageSink messageSink)
          : base(messageSink) { }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            return new InterceptingExecutor(base.CreateExecutor(assemblyName), this);
        }

        internal class InterceptingExecutor : LongLivedMarshalByRefObject, ITestFrameworkExecutor
        {
            private readonly ITestFrameworkExecutor _inner;
            private readonly InterceptingTestFramework _interceptingTestFramework;

            internal InterceptingExecutor(ITestFrameworkExecutor inner, InterceptingTestFramework interceptingTestFramework)
            {
                _inner = inner;
                _interceptingTestFramework = interceptingTestFramework;
            }

            public ITestCase Deserialize(string value)
            {
                return _inner.Deserialize(value);
            }

            public void Dispose()
            {
                _inner.Dispose();
            }

            public void RunAll(IMessageSink executionMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions, ITestFrameworkExecutionOptions executionOptions)
            {
                InterceptingTestFramework.RunAll = true;
                _inner.RunAll(executionMessageSink, discoveryOptions, executionOptions);                
            }

            public void RunTests(IEnumerable<ITestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
            {
                TestCases.AddRange(testCases);
                _inner.RunTests(testCases, executionMessageSink, executionOptions);
            }
        }
    }  
}
