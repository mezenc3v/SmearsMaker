using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace SmearTracer
{
    public class Converters
    {
        private const int DataFormatSize = 4;

        public List<Pixel> ConvertBitmapImageToPixels(BitmapSource source)
        {
            List<Pixel> inputData = new List<Pixel>();
            int stride = source.PixelWidth * DataFormatSize;
            int size = source.PixelHeight * stride;
            byte[] data = new byte[size];
            source.CopyPixels(data, stride, 0);

            for (int x = 0; x < source.PixelWidth; x++)
            {
                for (int y = 0; y < source.PixelHeight; y++)
                {
                    int indexPixel = y * stride + DataFormatSize * x;
                    double[] dataPixel = new double[DataFormatSize];
                    for (int i = 0; i < DataFormatSize; i++)
                    {
                        dataPixel[i] = data[indexPixel + i];
                    }
                    inputData.Add(new Pixel(dataPixel, x, y));
                }
            }
            return inputData;
        }

        public BitmapSource ConvertPixelsToBitmapImage(BitmapSource source, List<Pixel> listPixel)
        {
            int stride = source.PixelWidth * DataFormatSize;
            int size = source.PixelHeight * stride;
            byte[] data = new byte[size];

            foreach (var pixel in listPixel)
            {
                int indexPixel = (int)(pixel.PixelPosition.Y * stride + DataFormatSize * pixel.PixelPosition.X);
                for (int i = 0; i < DataFormatSize; i++)
                {
                    data[indexPixel + i] = (byte)pixel.ArgbArray[i];
                }
            }
            BitmapSource image = BitmapSource.Create(source.PixelWidth, source.PixelHeight, source.DpiX,
                source.DpiY, source.Format, source.Palette, data, stride);

            return image;
        }
    }
}
