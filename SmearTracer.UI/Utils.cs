using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using SmearTracer.Core.Abstract;
using SmearTracer.Core.Models;

namespace SmearTracer.Core
{
    public static class Utils
    {
        private const int DataFormatSize = 4;

        public static List<IUnit> ConvertBitmapImageToPixels(BitmapSource source)
        {
            var inputData = new List<IUnit>();
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

        public static BitmapSource ConvertPixelsToBitmapImage(BitmapSource source, List<IUnit> listPixel)
        {
            var stride = source.PixelWidth * DataFormatSize;
            var size = source.PixelHeight * stride;
            var data = new byte[size];

            foreach (var pixel in listPixel)
            {
                var indexPixel = (int)(pixel.Position.Y * stride + DataFormatSize * pixel.Position.X);
                for (int i = 0; i < DataFormatSize; i++)
                {
                    data[indexPixel + i] = (byte)pixel.Data[i];
                }
            }
            var image = BitmapSource.Create(source.PixelWidth, source.PixelHeight, source.DpiX,
                source.DpiY, source.Format, source.Palette, data, stride);

            return image;
        }

        public static void Concat(int minCount, List<Cluster> clusters)
        {

                Parallel.ForEach(clusters, cluster =>
                {
                    int countPrevious;
                    int countNext;

                    var segments = cluster.Segments;
                    do
                    {
                        countPrevious = segments.Count;
                        for (int i = 0; i < segments.Count; i++)
                        {
                            var width = (int)(segments[i].MaxX.X - segments[i].MinX.X);
                            var height = (int)(segments[i].MaxY.Y - segments[i].MinY.Y);

                            if (segments[i].Units.Count < minCount * 5
                                || (width / (height + 1) > 3 || height / (width + 1) > 3) && segments[i].Units.Count < minCount * 6
                                || (width > Math.Sqrt(minCount) * 3 || height > Math.Sqrt(minCount) * 3) && segments[i].Units.Count < minCount * 6)
                            {
                                foreach (var c in clusters.OrderBy(c => Distance(c.Centroid, segments[i].Center.Data)))
                                {
                                    lock (clusters)
                                    {
                                        if (Concat(c.Segments, segments[i]))
                                        {
                                            segments.RemoveAt(i);
                                            break;
                                        }
                                    }
                                }


                            }
                        }
                        countNext = segments.Count;
                    } while (countNext != countPrevious);
                });
        }

        private static bool Concat(IEnumerable<Segment> segments, Part segment)
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

        private static double Distance(IReadOnlyList<double> colorFirst, IReadOnlyList<double> colorSecond)
        {
            double distance = 0;

            for (int i = 0; i < colorFirst.Count; i++)
            {
                distance += Math.Abs(colorFirst[i] - colorSecond[i]);
            }

            return distance;
        }
    }
}
