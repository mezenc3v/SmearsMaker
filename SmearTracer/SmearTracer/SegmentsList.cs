using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SmearTracer
{
    public class SegmentsList
    {
        public List<Segment> Segments { get; set; }

        public List<Segment> SuperPixels { get; set; }

        public SegmentsList()
        {
            Segments = new List<Segment>();
        }

        public SegmentsList(IEnumerable<SegmentsList> segments)
        {
            Segments = new List<Segment>();
            foreach (var segmentList in segments)
            {
                Segments.AddRange(segmentList.Segments);
            }
        }

        public void Compute(List<Pixel> inputData)
        {
            var segmentData = inputData.OrderBy(d => d.X).ToList();
            while (segmentData.Count > 0)
            {
                var data = segmentData;
                int countPrevious, countNext;
                var segment = new Segment();
                segment.Data.Add(data[0]);
                data.RemoveAt(0);
                do
                {
                    segmentData = new List<Pixel>();
                    countPrevious = data.Count;
                    foreach (var pixel in data)
                    {
                        if (segment.CompareTo(pixel))
                        {
                            segment.Data.Add(pixel);
                        }
                        else
                        {
                            segmentData.Add(pixel);
                        }
                    }
                    data = segmentData;
                    countNext = segmentData.Count;
                } while (countPrevious != countNext);

                segment.UpdateSegment();
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
                        Segments[index].Data.AddRange(Segments[i].Data);
                        Segments[index].UpdateSegment();
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
                foreach (Pixel pixel in inputSegment.Data)
                {
                    if (segment.CompareTo(pixel) && Segments.IndexOf(segment) != Segments.IndexOf(inputSegment))
                    {
                        indexes.Add(Segments.IndexOf(segment));
                        break;
                    }
                }
            }

            return indexes.OrderBy(i => Distance(Segments[i].CentroidPixel.Data, inputSegment.CentroidPixel.Data)).First();
        }

        public void Split(int size, double tolerance)
        {
            int diameter = (int)Math.Sqrt(size);
            var elementarySegments = new List<Segment>();
            //spliting each complex segment on elementary segments
            foreach (var segment in Segments)
            {
                elementarySegments.AddRange(SegmentToElementarySegments(segment, diameter));
            }
            Segments = elementarySegments;
            foreach (var segment in Segments)
            {
                segment.UpdateSegment();
            }
        }

        private static IEnumerable<Segment> SegmentToElementarySegments(Segment complexSegment, int diameter)
        {
            complexSegment.UpdateSegment();

            var data = complexSegment.Data;
            var superPixels = new List<SuperPixel>();
            var newElementarySegments = new List<Segment>();

            var firstPoint = new Point(complexSegment.MinXPoint.X, complexSegment.MinYPoint.Y);
            var secondPoint = new Point(complexSegment.MaxXPoint.X, complexSegment.MaxYPoint.Y);
            var widthCount = (int)(complexSegment.MaxXPoint.X - complexSegment.MinXPoint.X);
            var heightCount = (int)(complexSegment.MaxYPoint.Y - complexSegment.MinYPoint.Y);
            var samples = InitilalCentroids(widthCount, heightCount, firstPoint, secondPoint, diameter);

            foreach (var centroid in samples)
            {
                var superPixel = new SuperPixel(centroid);
                superPixels.Add(superPixel);
            }

            foreach (var pixel in data)
            {
                var winner = NearestCentroid(pixel, superPixels);
                superPixels[winner].Data.Add(pixel);
            }

            foreach (var superPixel in superPixels)
            {
                if (superPixel.Data.Count > 0)
                {
                    Segment seg = new Segment { Data = superPixel.Data };
                    seg.UpdateSegment();
                    newElementarySegments.Add(seg);
                }
            }
            return newElementarySegments;
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
            double sum = Math.Pow(pixel.X - superPixel.Centroid.X, 2);
            sum += Math.Pow(pixel.Y - superPixel.Centroid.Y, 2);
            return Math.Sqrt(sum);
        }

        private static IEnumerable<Point> InitilalCentroids(int widthCount, int heightCount, Point firstPoint, Point secondPoint, int diameter)
        {
            var samplesData = new List<Point>();

            if (widthCount > 0 && heightCount > 0)
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
