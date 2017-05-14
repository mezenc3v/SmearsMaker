using System.Windows.Media.Imaging;

namespace SmearTracer.AL
{
    public interface IImageLayers
    {
        void Compute();
        BitmapSource Initial();
        BitmapSource SuperPixels();
        BitmapSource SuperPixelsColor();
        BitmapSource Circles();
        BitmapSource CirclesRndColor();
        BitmapSource VectorMap();
    }
}
