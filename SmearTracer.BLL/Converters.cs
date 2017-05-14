using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using SmearTracer.Model;

namespace SmearTracer.BLL
{
    public class Converters
    {
        private const int DataFormatSize = 4;

        public List<Pixel> ConvertBitmapImageToPixels(BitmapSource source)
        {
            var inputData = new List<Pixel>();
            var stride = source.PixelWidth * DataFormatSize;
            var size = source.PixelHeight * stride;
            var data = new byte[size];
            source.CopyPixels(data, stride, 0);

            for (int x = 0; x < source.PixelWidth; x++)
            {
                for (int y = 0; y < source.PixelHeight; y++)
                {
                    var indexPixel = y * stride + DataFormatSize * x;
                    var dataPixel = new double[DataFormatSize];
                    for (int i = 0; i < DataFormatSize; i++)
                    {
                        dataPixel[i] = data[indexPixel + i];
                    }
                    inputData.Add(new Pixel(dataPixel, new Point(x, y)));
                }
            }
            return inputData;
        }

        public BitmapSource ConvertPixelsToBitmapImage(BitmapSource source, List<Pixel> listPixel)
        {
            var stride = source.PixelWidth * DataFormatSize;
            var size = source.PixelHeight * stride;
            var data = new byte[size];

            foreach (var pixel in listPixel)
            {
                var indexPixel = (int)(pixel.Position.Y * stride + DataFormatSize * pixel.Position.X);
                for (int i = 0; i < DataFormatSize; i++)
                {
                    data[indexPixel + i] = (byte)pixel.ArgbArray[i];
                }
            }
            var image = BitmapSource.Create(source.PixelWidth, source.PixelHeight, source.DpiX,
                source.DpiY, source.Format, source.Palette, data, stride);

            return image;
        }
    }
}
