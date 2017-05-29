using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using SmearTracer.UI.Abstract;
using SmearTracer.UI.Models;

namespace SmearTracer.UI
{
    public static class Utils
    {
        private const int DataFormatSize = 4;

        public static List<Unit> ConvertBitmapImageToPixels(BitmapSource source)
        {
            var inputData = new List<Unit>();
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
                    inputData.Add(new Unit(dataPixel, new Point(x, y)));
                }
            }
            return inputData;
        }

        public static BitmapSource ConvertPixelsToBitmapImage(BitmapSource source, List<Unit> listPixel)
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

        public static void Concat(int minCount, List<Cluster> clusters)
        {
            foreach (var cluster in clusters)
            {
                int countPrevious;
                int countNext;

                var segments = cluster.Segments;
                do
                {
                    countPrevious = segments.Count;
                    for (int i = 0; i < segments.Count; i++)
                    {
                        if (segments[i].Units.Count < minCount)
                        {
                            foreach (var c in clusters.OrderBy(c => Distance(c.Centroid, segments[i].Center.ArgbArray)))
                            {
                                if (Concat(c.Segments, segments[i]))
                                {
                                    segments.RemoveAt(i);
                                    break;
                                }
                            }

                            
                        }
                    }
                    countNext = segments.Count;
                } while (countNext != countPrevious);
            }
        }

        private static bool Concat(List<Segment> segments, Segment segment)
        {
            foreach (var seg in segments)
            {
                if (segment.Units.Any(unit => seg.Contains(unit) && seg.Center != segment.Center))
                {
                    seg.Units.AddRange(segment.Units);
                    seg.Update();
                    return true;
                }
            }
            return false;
        }


        private static double Distance(double[] colorFirst, double[] colorSecond)
        {
            double distance = 0;

            for (int i = 0; i < colorFirst.Length; i++)
            {
                distance += Math.Abs(colorFirst[i] - colorSecond[i]);
            }

            return distance;
        }
    }
}
