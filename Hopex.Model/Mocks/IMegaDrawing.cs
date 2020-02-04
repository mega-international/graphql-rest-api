using Mega.Macro.API;
using Mega.Macro.API.Drawings;
using Mega.Macro.API.Enums;

namespace Hopex.Model.Mocks
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

    internal class RealMegaDrawing : RealMegaWrapperObject, IMegaDrawing
    {
        protected MegaDrawing _realDrawing => (MegaDrawing)_realWrapperObject;

        public string Name => _realDrawing.Name;

        internal RealMegaDrawing(MegaDrawing realDrawing) : base(realDrawing) { }

        public void SaveAsPicture(string fileName, MegaFilePictureFormat filePictureFormat, int bitsPerPixel, int quality, int resolution, int height, int width, ref int imageHeight, ref int imageWidth, object vDispatch)
        {
            _realDrawing.SaveAsPicture(fileName, filePictureFormat, bitsPerPixel, quality, resolution, height, width, ref imageHeight, ref imageWidth, vDispatch);
        }
    }

    public class RealMegaDrawingFactory : IMegaDrawingFactory
    {
        public IMegaDrawing CreateFromDiagram(IMegaObject diagram, string accessMode)
        {
            var wrappedObject = (RealMegaObject)diagram;
            var comDrawing = wrappedObject.RealObject.CallFunction<MegaWrapperObject>("drawing", accessMode);
            if (comDrawing == null) return null;
            var drawing = new MegaDrawing { NativeObject = comDrawing.NativeObject };
            return new RealMegaDrawing(drawing);
        }
    }
}
