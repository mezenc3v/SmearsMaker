using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SmearTracer.Model;

namespace SmearTracer.BLL
{
    public class Segmentation
    {
        public List<Segment> Segments;

        public Segmentation()
        {
            Segments = new List<Segment>();
        }

        public Segmentation(IEnumerable<Segmentation> segments)
        {
            Segments = new List<Segment>();
            foreach (var segmentList in segments)
            {
                Segments.AddRange(segmentList.Segments);
            }
        }

        public void Compute(List<Pixel> inputData)
        {
            var segmentData = inputData.OrderBy(d => d.Position.X).ToList();
            while (segmentData.Count > 0)
            {
                var data = segmentData;
                int countPrevious, countNext;
                var segment = new Segment();
                segment.AddData(data[0]);
                data.RemoveAt(0);
                do
                {
                    segmentData = new List<Pixel>();
                    countPrevious = data.Count;
                    foreach (var pixel in data)
                    {
                        if (segment.Contains(pixel))
                        {
                            segment.AddData(pixel);
                        }
                        else
                        {
                            segmentData.Add(pixel);
                        }
                    }
                    data = segmentData;
                    countNext = segmentData.Count;
                } while (countPrevious != countNext);

                segment.Update();
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
                    if (Segments[i].Data.Count < minCount)
                    {
                        int index = Concat(Segments[i]);
                        Segments[index].ArrDataRange(Segments[i].Data);
                        Segments[index].Update();
                        Segments.RemoveAt(i);
                    }
                }
                countNext = Segments.Count;
            } while (countNext != countPrevious);
        }

        private int Concat(Segment inputSegment)
        {
            var indexes = new List<int>();
            foreach (var segment in Segments)
            {
                foreach (Pixel pixel in inputSegment.Data)
                {
                    if (segment.Contains(pixel) && Segments.IndexOf(segment) != Segments.IndexOf(inputSegment))
                    {
                        indexes.Add(Segments.IndexOf(segment));
                        break;
                    }
                }
            }

            return indexes.OrderBy(i => Distance(Segments[i].Center.ArgbArray, inputSegment.Center.ArgbArray)).First();
        }

        public void ComputeSuperPixels(int minSize, int maxSize, double tolerance)
        {
            var maxDiameter = Math.Sqrt(maxSize);
            var minDiameter = Math.Sqrt(minSize);
            //spliting each complex segment into superPixels
            foreach (var segment in Segments)
            {
                var superPixels = new List<SuperPixel>();
                superPixels.AddRange(SegmentToSuperPixels(segment, minDiameter, maxDiameter));
                if (superPixels.Count == 0)
                {
                    var diameter = maxDiameter;
                    do
                    {
                        superPixels.AddRange(SegmentToSuperPixels(segment, minDiameter, --diameter));
                    } while (superPixels.Count == 0 && diameter < minDiameter);
                }
                segment.SuperPixels = superPixels;

                var circles = new List<Circle>();

                foreach (var superPixel in segment.SuperPixels)
                {
                    var radius = Math.Sqrt(superPixel.Data.Count) / 2;
                    var centerX = superPixel.Data.Average(p => p.Position.X);
                    var centerY = superPixel.Data.Average(p => p.Position.Y);

                    Circle circle;
                    int countCircle;
                    int countCircleNew;
                    do
                    {
                        circle = new Circle(new Point(centerX, centerY), radius);
                        radius++;
                        var circleNew = new Circle(new Point(centerX, centerY), radius);
                        countCircle = circle.Contains(superPixel.Data);
                        countCircleNew = circleNew.Contains(superPixel.Data);
                    } while (countCircle <= countCircleNew && radius <= maxDiameter);

                    circles.Add(circle);
                }

                segment.CirclesList = circles;
            }

            foreach (var segment in Segments)
            {
                segment.Update();
            }
        }

        private static IEnumerable<SuperPixel> SegmentToSuperPixels(Segment complexSegment, double minDiameter, double maxDiameter)
        {
            var data = complexSegment.Data;
            var superPixelsList = new List<SuperPixel>();

            var firstPoint = new Point(complexSegment.MinX.X, complexSegment.MinY.Y);
            var secondPoint = new Point(complexSegment.MaxX.X, complexSegment.MaxY.Y);
            var widthCount = (int)(complexSegment.MaxX.X - complexSegment.MinX.X);
            var heightCount = (int)(complexSegment.MaxY.Y - complexSegment.MinY.Y);
            var samples = InitilalCentroids(widthCount, heightCount, firstPoint, secondPoint, maxDiameter);
            var smallData = new List<Pixel>();
            var superPixels = samples.Select(centroid => new SuperPixel(centroid)).ToList();
            //Search for winners and distribution of data
            foreach (var pixel in data)
            {
                var winner = NearestCentroid(pixel, superPixels);
                superPixels[winner].AddData(pixel);
            }
            //Deleting empty cells and cells with small data count
            foreach (var superPixel in superPixels)
            {
                if (superPixel.Data.Count > 0)
                {
                    /*if (superPixel.Data.Count < minDiameter * minDiameter)
                    {
                        smallData.AddRange(superPixel.Data);
                    }
                    else
                    {*/
                    superPixelsList.Add(superPixel);
                    //}

                }
            }

            if (superPixelsList.Count > 0)
            {
                foreach (var pixel in smallData)
                {
                    var winner = NearestCentroid(pixel, superPixelsList);
                    superPixelsList[winner].AddData(pixel);
                }
            }

            return superPixelsList;
        }

        private static int NearestCentroid(Pixel pixel, List<SuperPixel> superPixels)
        {
            var index = 0;
            var min = Distance(superPixels[0], pixel);
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
            var sum = Math.Pow(pixel.Position.X - superPixel.Center.Position.X, 2);
            sum += Math.Pow(pixel.Position.Y - superPixel.Center.Position.Y, 2);
            return Math.Sqrt(sum);
        }

        private static IEnumerable<Point> InitilalCentroids(int widthCount, int heightCount, Point firstPoint, Point secondPoint, double diameter)
        {
            var samplesData = new List<Point>();

            if (widthCount > diameter && heightCount > diameter)
            {
                for (int i = (int)firstPoint.X; i < (int)secondPoint.X; i += (int)diameter)
                {
                    for (int j = (int)firstPoint.Y; j < (int)secondPoint.Y; j += (int)diameter)
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
