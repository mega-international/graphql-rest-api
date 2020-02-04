using Hopex.Model.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hopex.WebService.Tests.Mocks.Drawings
{
    public class MockMegaDrawingFactory : IMegaDrawingFactory
    {
        public IMegaDrawing CreateFromDiagram(IMegaObject diagram, string accessMode)
        {
            var mockObject = (MockMegaObject)diagram;
            if (mockObject._drawing != null) return mockObject._drawing;
            return null;
        }
    }
}
