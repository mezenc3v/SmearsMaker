using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using SmearTracer.Model.Abstract;

namespace SmearTracer.Model
{
    public class GraphicUnit:IGraphicUnit
    {
        public IUnit Center { get; set; }
        public double[] Color { get; set; }
        public Point MinX { get; set; }
        public Point MinY { get; set; }
        public Point MaxX { get; set; }
        public Point MaxY { get; set; }
        public List<IUnit> Data { get; set; }

        public void Update()
        {
            if (Data.Count > 0)
            {
                //coorditates for compute centroid
                double x = 0;
                double y = 0;
                var averageData = new double[Data.First().ArgbArray.Length];
                //coordinates for compute vector
                var minX = Data[0].Position.X;
                var minY = Data[0].Position.Y;
                var maxX = minX;
                var maxY = minY;
                MinX = new Point(Data[0].Position.X, Data[0].Position.Y);
                MaxX = new Point(Data[0].Position.X, Data[0].Position.Y);
                MinY = new Point(Data[0].Position.X, Data[0].Position.Y);
                MaxY = new Point(Data[0].Position.X, Data[0].Position.Y);
                foreach (var data in Data)
                {
                    x += data.Position.X;
                    y += data.Position.Y;
                    for (int i = 0; i < averageData.Length; i++)
                    {
                        averageData[i] += data.ArgbArray[i];
                    }
                    //find min and max coordinates in segment
                    if (data.Position.X < minX)
                    {
                        minX = data.Position.X;
                        MinX = new Point(data.Position.X, data.Position.Y);
                    }
                    if (data.Position.Y < minY)
                    {
                        minY = data.Position.Y;
                        MinY = new Point(data.Position.X, data.Position.Y);
                    }
                    if (data.Position.X > maxX)
                    {
                        maxX = data.Position.X;
                        MaxX = new Point(data.Position.X, data.Position.Y);
                    }
                    if (data.Position.Y > maxY)
                    {
                        maxY = data.Position.Y;
                        MaxY = new Point(data.Position.X, data.Position.Y);
                    }
                }
                x /= Data.Count;
                y /= Data.Count;
                for (int i = 0; i < averageData.Length; i++)
                {
                    averageData[i] /= Data.Count;
                }
                var centroid = new Pixel(averageData, new Point(x, y));
                Center = centroid;

                Data = Data.OrderBy(p => p.Position.X).ThenBy(p => p.Position.Y).ToList();
            }
        }

        public void AddData(IUnit pixel)
        {
            Data.Add(pixel);
        }

        public bool Contains(IUnit pixel)
        {
            for (int i = Data.Count - 1; i >= 0; i--)
            {
                if (Math.Abs(pixel.Position.X - Data[i].Position.X)
                    < 2 && Math.Abs(pixel.Position.Y - Data[i].Position.Y) < 2)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddDataRange(IEnumerable<IUnit> pixels)
        {
            Data.AddRange(pixels);
        }

        public static double[] Generate()
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
