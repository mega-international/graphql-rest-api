using Hopex.Model.Mocks;
using Mega.Macro.API.Enums;

namespace Hopex.WebService.Tests.Mocks.Drawings
{
    public class MockMegaDrawing : MockMegaWrapperObject, IMegaDrawing
    {
        internal IMegaObject _diagramObject;
        internal string _accessMode;

        public MockMegaDrawing(IMegaObject diagram, string accessMode)
        {
            _diagramObject = diagram;
            _accessMode = accessMode;
        }

        public virtual string Name => "a mocked diagram";

        public virtual void SaveAsPicture(string fileName, MegaFilePictureFormat filePictureFormat, int bitsPerPixel, int quality, int resolution, int height, int width, ref int imageHeight, ref int imageWidth, object vDispatch)
        {            
        }
    }
}
