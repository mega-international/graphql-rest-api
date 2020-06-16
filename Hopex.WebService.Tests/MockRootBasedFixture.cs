using Hopex.ApplicationServer.WebServices;
using Hopex.Common;
using Hopex.Model.Abstractions;
using Hopex.Modules.GraphQL;
using Hopex.WebService.Tests.Mocks;
using Mega.Macro.API;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using static Hopex.WebService.Tests.Assertions.MegaIdMatchers;

namespace Hopex.WebService.Tests
{
    public class MockRootBasedFixture
    {

        protected TestableEntryPoint ws;

        protected static Mock<MockMegaRoot> CreateSpyRootWithPublish()
        {
            var spyRoot = new Mock<MockMegaRoot>() { CallBase = true };
            spyRoot
                .Setup(r => r.CallFunctionString(IsId("~lcE6jbH9G5cK[PublishStayInSessionWizard Command Launcher]"), "{\"instruction\":\"POSTPUBLISHINSESSION\"}", null, null, null, null, null))
                .Returns("SESSION_PUBLISH")
                .Verifiable();
            return spyRoot;
        }

        protected async Task<HopexResponse> ExecuteQueryAsync(IMegaRoot root, string query, string schema = "ITPM", object variables = null)
        {
            ws = new TestableEntryPoint(root, schema);
            var args = new InputArguments
            {
                Query = query
            };
            args.Variables = ConvertAnonymousToDictionary(variables);
            return await ws.Execute(args);
        }

        private static Dictionary<string, object> ConvertAnonymousToDictionary(object values)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (values != null)
            {
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(values))
                {
                    object obj = propertyDescriptor.GetValue(values);
                    dict.Add(propertyDescriptor.Name, obj);
                }
            }
            return dict;
        }

        protected class TestableEntryPoint : EntryPoint
        {
            private readonly MockraphQLRequest _request;
            private readonly IMegaRoot _megaRoot;

            public TestableEntryPoint(IMegaRoot root, string schema)
                    : base(new TestableSchemaManagerProvider(), new TestableLanguageProvider())
            {
                _request = new MockraphQLRequest(schema);
                _megaRoot = root;
                (this as IHopexWebService).SetHopexContext(_megaRoot, _request, new Logger());
            }

            protected override MegaRoot GetNativeMegaRoot()
            {
                return null;
            }

            public override IMegaRoot GetMegaRoot()
            {
                return _megaRoot;
            }
        }
    }

}
