using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SmearTracer
{
    public class ClustersSegmentation
    {
        public List<Segment> Segments { get; set; }

        public ClustersSegmentation()
        {
            Segments = new List<Segment>();
        }

        public ClustersSegmentation(IEnumerable<ClustersSegmentation> segments)
        {
            Segments = new List<Segment>();
            foreach (var segmentList in segments)
            {
                Segments.AddRange(segmentList.Segments);
            }
        }

        public void Compute(List<Pixel> inputData)
        {
            var segmentData = inputData.OrderBy(d => d.PixelPosition.X).ToList();
            while (segmentData.Count > 0)
            {
                var data = segmentData;
                int countPrevious, countNext;
                var segment = new Segment();
                segment.Information.AddData(data[0]);
                data.RemoveAt(0);
                do
                {
                    segmentData = new List<Pixel>();
                    countPrevious = data.Count;
                    foreach (var pixel in data)
                    {
                        if (segment.Information.CompareTo(pixel))
                        {
                            segment.Information.AddData(pixel);
                        }
                        else
                        {
                            segmentData.Add(pixel);
                        }
                    }
                    data = segmentData;
                    countNext = segmentData.Count;
                } while (countPrevious != countNext);

                segment.Information.Update();
                Segments.Add(segment);
            }
        }

        public void Concat(int minCount)
        {
            int countPrevious;
            int countNext;
            do
            {
                countPrevious = Segments.Count;
                for (int i = 0; i < Segments.Count; i++)
                {
                    if (Segments[i].Information.GetData.Count < minCount)
                    {
                        int index = Concat(Segments[i]);
                        Segments[index].Information.AddRangeData(Segments[i].Information.GetData);
                        Segments[index].Information.Update();
                        Segments.RemoveAt(i);
                    }
                }
                countNext = Segments.Count;
            } while (countNext != countPrevious);
        }

        private int Concat(Segment inputSegment)
        {
            List<int> indexes = new List<int>();
            foreach (var segment in Segments)
            {
                foreach (Pixel pixel in inputSegment.Information.GetData)
                {
                    if (segment.Information.CompareTo(pixel) && Segments.IndexOf(segment) != Segments.IndexOf(inputSegment))
                    {
                        indexes.Add(Segments.IndexOf(segment));
                        break;
                    }
                }
            }

            return indexes.OrderBy(i => Distance(Segments[i].Information.GetCenter.ArgbArray, inputSegment.Information.GetCenter.ArgbArray)).First();
        }

        public void ComputeSuperPixels(int minSize, int maxSize, double tolerance)
        {
            int maxDiameter = (int)Math.Sqrt(maxSize);
            int minDiameter = (int)Math.Sqrt(minSize);
            //spliting each complex segment into superPixels
            foreach (var segment in Segments)
            {
                var superPixels = new List<SuperPixel>();
                superPixels.AddRange(SegmentToSuperPixels(segment, minDiameter, maxDiameter));
                if (superPixels.Count == 0)
                {
                    int diameter = maxDiameter;
                    do
                    {
                        superPixels.AddRange(SegmentToSuperPixels(segment, minDiameter, --diameter));
                    } while (superPixels.Count == 0 && diameter < minDiameter);
                }
                segment.SuperPixels = superPixels;

                /*var circles = new List<Circle>();

                foreach (var superPixel in segment.SuperPixels)
                {
                    var radius = Math.Sqrt(superPixel.ArgbArray.Count) / 2;
                    var center = ComputeCenter(superPixel.ArgbArray);
                    var circle = new Circle(center, radius);
                    circles.Add(circle);
                }

                segment.CirclesList = circles*/;
            }


            foreach (var segment in Segments)
            {
                segment.Information.Update();
            }
        }

        private static IEnumerable<SuperPixel> SegmentToSuperPixels(Segment complexSegment, int minDiameter, int maxDiameter)
        {
            var data = complexSegment.Information.GetData;
            var superPixelsList = new List<SuperPixel>();

            var firstPoint = new Point(complexSegment.Information.GetMinXPoint.X, complexSegment.Information.GetMinYPoint.Y);
            var secondPoint = new Point(complexSegment.Information.GetMaxXPoint.X, complexSegment.Information.GetMaxYPoint.Y);
            var widthCount = (int)(complexSegment.Information.GetMaxXPoint.X - complexSegment.Information.GetMinXPoint.X);
            var heightCount = (int)(complexSegment.Information.GetMaxYPoint.Y - complexSegment.Information.GetMinYPoint.Y);
            var samples = InitilalCentroids(widthCount, heightCount, firstPoint, secondPoint, maxDiameter);
            var smallData = new List<Pixel>();
            var superPixels = samples.Select(centroid => new SuperPixel(centroid)).ToList();
            //Search for winners and distribution of data
            foreach (var pixel in data)
            {
                var winner = NearestCentroid(pixel, superPixels);
                superPixels[winner].Information.AddData(pixel);
            }
            //Deleting empty cells and cells with small data count
            foreach (var superPixel in superPixels)
            {
                if (superPixel.Information.GetData.Count > 0)
                {
                    if (superPixel.Information.GetData.Count < minDiameter * minDiameter)
                    {
                        smallData.AddRange(superPixel.Information.GetData);
                    }
                    else
                    {
                        superPixelsList.Add(superPixel);
                    }

                }
            }

            if (superPixelsList.Count > 0)
            {
                foreach (var pixel in smallData)
                {
                    var winner = NearestCentroid(pixel, superPixelsList);
                    superPixelsList[winner].Information.AddData(pixel);
                }
            }

            return superPixelsList;
        }

        private static int NearestCentroid(Pixel pixel, List<SuperPixel> superPixels)
        {
            int index = 0;
            double min = Distance(superPixels[0], pixel);
            for (int i = 0; i < superPixels.Count; i++)
            {
                var distance = Distance(superPixels[i], pixel);
                if (min > distance)
                {
                    min = distance;
                    index = i;
                }
            }
            return index;
        }

        private static double Distance(SuperPixel superPixel, Pixel pixel)
        {
            var sum = Math.Pow(pixel.PixelPosition.X - superPixel.Information.GetCenter.PixelPosition.X, 2);
            sum += Math.Pow(pixel.PixelPosition.Y - superPixel.Information.GetCenter.PixelPosition.Y, 2);
            return Math.Sqrt(sum);
        }

        private static IEnumerable<Point> InitilalCentroids(int widthCount, int heightCount, Point firstPoint, Point secondPoint, int diameter)
        {
            var samplesData = new List<Point>();

            if (widthCount > diameter && heightCount > diameter)
            {
                for (int i = (int)firstPoint.X; i < (int)secondPoint.X; i += diameter)
                {
                    for (int j = (int)firstPoint.Y; j < (int)secondPoint.Y; j += diameter)
                    {
                        samplesData.Add(new Point(i + diameter / 2, j + diameter / 2));
                    }
                }
            }
            else
            {
                samplesData.Add(new Point((firstPoint.X + secondPoint.X) / 2, (firstPoint.Y + secondPoint.Y) / 2));
            }

            return samplesData;
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
