using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SmearTracer
{
    public class Converters
    {
        private int _dataFormatSize;

        public double[][] BitmapImageToDoubleArray(BitmapSource source)
        {
            double[][] inputData = new double[source.PixelWidth * source.PixelHeight][];
            _dataFormatSize = 4;
            int stride = source.PixelWidth * _dataFormatSize;
            int size = source.PixelHeight * stride;
            int counter = 0;
            byte[] data = new byte[size];
            source.CopyPixels(data, stride, 0);

            for (int x = 0; x < source.PixelWidth; x++)
            {
                for (int y = 0; y < source.PixelHeight; y++)
                {
                    int indexPixel = y * stride + _dataFormatSize * x;
                    inputData[counter] = new double[_dataFormatSize];
                    for (int i = 0; i < _dataFormatSize; i++)
                    {
                        inputData[counter][i] = data[indexPixel + i];
                    }
                    counter++;
                }
            }

            return inputData;
        }

        public BitmapSource DoubleArrayToBitmapImage(BitmapSource source, double[][] inputData)
        {
            int stride = source.PixelWidth * _dataFormatSize;
            int size = source.PixelHeight * stride;
            int counter = 0;
            byte[] data = new byte[size];

            for (int x = 0; x < source.PixelWidth; x++)
            {
                for (int y = 0; y < source.PixelHeight; y++)
                {
                    int indexPixel = y * stride + _dataFormatSize * x;
                    for (int i = 0; i < _dataFormatSize; i++)
                    {
                        data[indexPixel + i] = (byte)inputData[counter][i];
                    }
                    counter++;
                }
            }

            BitmapSource image = BitmapSource.Create(source.PixelWidth, source.PixelHeight, source.DpiX,
                source.DpiY, source.Format, source.Palette, data, stride);

            return image;
        }

        public List<Pixel> DoubleArrayToDataPixels(double[][] data, int width, int height)
        {
            List<Pixel> dataPixelList = new List<Pixel>();
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int index = i * height + j;
                    dataPixelList.Add(new Pixel(data[index], i, j));
                }
            }
            return dataPixelList;
        }

        public List<Pixel> BitmapImageToListPixels(BitmapSource source)
        {
            List<Pixel> inputData = new List<Pixel>();
            _dataFormatSize = 4;
            int stride = source.PixelWidth * _dataFormatSize;
            int size = source.PixelHeight * stride;
            byte[] data = new byte[size];
            source.CopyPixels(data, stride, 0);

            for (int x = 0; x < source.PixelWidth; x++)
            {
                for (int y = 0; y < source.PixelHeight; y++)
                {
                    int indexPixel = y * stride + _dataFormatSize * x;
                    double[] dataPixel = new double[_dataFormatSize];
                    for (int i = 0; i < _dataFormatSize; i++)
                    {
                        dataPixel[i] = data[indexPixel + i];
                    }
                    inputData.Add(new Pixel(dataPixel, x, y));
                }
            }
            return inputData;
        }

        public BitmapSource ListPixelsToBitmapImage(BitmapSource source, List<Pixel> listPixel)
        {
            int stride = source.PixelWidth * _dataFormatSize;
            int size = source.PixelHeight * stride;
            byte[] data = new byte[size];

            foreach (var pixel in listPixel)
            {
                int indexPixel = (int)(pixel.Coordinates[1] * stride + _dataFormatSize * pixel.Coordinates[0]);
                for (int i = 0; i < _dataFormatSize; i++)
                {
                    data[indexPixel + i] = (byte)pixel.Data[i];
                }
            }

            BitmapSource image = BitmapSource.Create(source.PixelWidth, source.PixelHeight, source.DpiX,
                source.DpiY, source.Format, source.Palette, data, stride);

            return image;
        }
    }
}
