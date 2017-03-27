using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SmearTracer
{
    public class ListSegments
    {
        public List<Segment> Segments { get; set; }

        public ListSegments()
        {
            Segments = new List<Segment>();
        }

        public ListSegments(List<ListSegments> networks)
        {
            Segments = new List<Segment>();
            foreach (var segmentNetwork in networks)
            {
                Segments.AddRange(segmentNetwork.Segments);
            }
            for (int i = 0; i < Segments.Count; i++)
            {
                UpdateSegment(i);
            }
        }

        public void Compute(List<Pixel> inputData)
        {
            var segmentData = inputData.OrderBy(d=>d.X).ToList();
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
                    foreach (Pixel pixel in data)
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
                Segments.Add(segment);
            }
            for (int i = 0; i < Segments.Count; i++)
            {
                UpdateSegment(i);
            }
        }

        private void UpdateSegment(int index)
        {
            //coorditates for calculate centroid
            double x = 0;
            double y = 0;
            double[] averageData = new double[Segments[index].Data.First().Data.Length];
            //coordinates for calculate vector
            double minX = Segments[index].Data[0].X;
            double minY = Segments[index].Data[0].Y;
            double maxX = minX;
            double maxY = minY;
            Segments[index].MinXPoint = new Point(Segments[index].Data[0].X, Segments[index].Data[0].Y);
            Segments[index].MaxXPoint = new Point(Segments[index].Data[0].X, Segments[index].Data[0].Y);
            Segments[index].MinYPoint = new Point(Segments[index].Data[0].X, Segments[index].Data[0].Y);
            Segments[index].MaxYPoint = new Point(Segments[index].Data[0].X, Segments[index].Data[0].Y);
            foreach (var data in Segments[index].Data)
            {
                x += data.X;
                y += data.Y;
                for (int i = 0; i < averageData.Length; i++)
                {
                    averageData[i] += data.Data[i];
                }
                //find min and max coordinates in segment
                if (data.X < minX)
                {
                    minX = data.X;
                    Segments[index].MinXPoint = new Point(data.X, data.Y);
                }
                if (data.Y < minY)
                {
                    minY = data.Y;
                    Segments[index].MinYPoint = new Point(data.X, data.Y);
                }
                if (data.X > maxX)
                {
                    maxX = data.X;
                    Segments[index].MaxXPoint = new Point(data.X, data.Y);
                }
                if (data.X > maxY)
                {
                    maxY = data.X;
                    Segments[index].MaxYPoint = new Point(data.X, data.Y);
                }
            }
            x /= Segments[index].Data.Count;
            y /= Segments[index].Data.Count;
            for (int i = 0; i < averageData.Length; i++)
            {
                averageData[i] /= Segments[index].Data.Count;
            }
            Pixel centroid = new Pixel(averageData, (int)x, (int)y);
            Segments[index].CentroidPixel = centroid;

            Segments[index].Data = Segments[index].Data.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
        }

        private double Distance(double[] colorFirst, double[] colorSecond)
        {
            double distance = 0;

            for (int i = 0; i < colorFirst.Length; i++)
            {
                distance += Math.Abs(colorFirst[i] - colorSecond[i]);
            }

            return distance;
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
                        UpdateSegment(index);
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
    }
}
