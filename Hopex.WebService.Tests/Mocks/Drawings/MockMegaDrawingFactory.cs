using Hopex.Model.Abstractions;

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
