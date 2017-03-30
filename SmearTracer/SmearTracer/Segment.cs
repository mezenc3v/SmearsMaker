using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;

namespace SmearTracer
{
    public class Segment
    {
        public List<Pixel> Data { get; set; }
        public List<SuperPixel> SuperPixels { get; set; }
        public List<Circle> CirclesList { get; set; }
        public Pixel CentroidPixel { get; set; }
        public Point MinXPoint { get; set; }
        public Point MinYPoint { get; set; }
        public Point MaxXPoint { get; set; }
        public Point MaxYPoint { get; set; }
        public double[] Color { get; set; }

        public Segment()
        {
            Data = new List<Pixel>();
            SuperPixels = new List<SuperPixel>();
            CirclesList = new List<Circle>();
            Color = Generate();
            MinXPoint = new Point();
            MaxXPoint = new Point();
            MinYPoint = new Point();
            MaxYPoint = new Point();
        }

        public Segment(Segment inputSegment)
        {
            Data = inputSegment.Data;
            Color = Generate();
            MinXPoint = inputSegment.MinXPoint;
            MaxXPoint = inputSegment.MaxXPoint;
            MinYPoint = inputSegment.MinYPoint;
            MaxYPoint = inputSegment.MaxYPoint;
            CentroidPixel = inputSegment.CentroidPixel;
            SuperPixels = inputSegment.SuperPixels;
            CirclesList = inputSegment.CirclesList;
        }

        public bool CompareTo(Pixel data)
        {
            for (int i = Data.Count - 1; i >= 0; i--)
            {
                if (Math.Abs(data.X - Data[i].X) < 2 && Math.Abs(data.Y - Data[i].Y) < 2)
                {
                    return true;
                }
            }
            return false;
        }

        public void Update()
        {
            if (Data.Count > 0)
            {
                //coorditates for calculate centroid
                double x = 0;
                double y = 0;
                double[] averageData = new double[Data.First().Data.Length];
                //coordinates for calculate vector
                double minX = Data[0].X;
                double minY = Data[0].Y;
                double maxX = minX;
                double maxY = minY;
                MinXPoint = new Point(Data[0].X, Data[0].Y);
                MaxXPoint = new Point(Data[0].X, Data[0].Y);
                MinYPoint = new Point(Data[0].X, Data[0].Y);
                MaxYPoint = new Point(Data[0].X, Data[0].Y);
                foreach (var data in Data)
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
                        MinXPoint = new Point(data.X, data.Y);
                    }
                    if (data.Y < minY)
                    {
                        minY = data.Y;
                        MinYPoint = new Point(data.X, data.Y);
                    }
                    if (data.X > maxX)
                    {
                        maxX = data.X;
                        MaxXPoint = new Point(data.X, data.Y);
                    }
                    if (data.Y > maxY)
                    {
                        maxY = data.Y;
                        MaxYPoint = new Point(data.X, data.Y);
                    }
                }
                x /= Data.Count;
                y /= Data.Count;
                for (int i = 0; i < averageData.Length; i++)
                {
                    averageData[i] /= Data.Count;
                }
                Pixel centroid = new Pixel(averageData, (int)x, (int)y);
                CentroidPixel = centroid;

                Data = Data.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            }
        }

        private static double[] Generate()
        {
            RNGCryptoServiceProvider c = new RNGCryptoServiceProvider();
            byte[] randomNumber = new byte[3];
            c.GetBytes(randomNumber);

            byte r = randomNumber[0];
            byte g = randomNumber[1];
            byte b = randomNumber[2];

            double[] color = { r, g, b, 255 };

            return color;
        }
    }
}
