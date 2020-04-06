using Mega.Macro.API.Enums;

namespace Hopex.Model.Abstractions
{
    public interface IMegaDrawing : IMegaWrapperObject
    {
        string Name { get; }
        void SaveAsPicture(string fileName, MegaFilePictureFormat filePictureFormat, int bitsPerPixel, int quality, int resolution, int height, int width, ref int imageHeight, ref int imageWidth, object vDispatch);
    }

    public interface IMegaDrawingFactory
    {
        IMegaDrawing CreateFromDiagram(IMegaObject diagram, string accessMode);
    }    
}
