using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using SmearTracer.Core.Abstract;

namespace SmearTracer.Core.Models
{
    public class Superpixel : Part
    {
        public Superpixel(Point point)
        {
            Center = new Pixel(point);
            Units = new List<IUnit>();
            Color = Generate();
        }

        public override void Update()
        {
            if (Units.Count > 0)
            {
                //coorditates for compute centroid
                int x = 0;
                int y = 0;
                var averageData = new double[Units.First().Data.Length];
                //coordinates for compute vector
                var minX = Units[0].Position.X;
                var minY = Units[0].Position.Y;
                var maxX = minX;
                var maxY = minY;
                MinX = new Point(Units[0].Position.X, Units[0].Position.Y);
                MaxX = new Point(Units[0].Position.X, Units[0].Position.Y);
                MinY = new Point(Units[0].Position.X, Units[0].Position.Y);
                MaxY = new Point(Units[0].Position.X, Units[0].Position.Y);
                foreach (var data in Units)
                {
                    x += (int)data.Position.X;
                    y += (int)data.Position.Y;
                    for (int i = 0; i < averageData.Length; i++)
                    {
                        averageData[i] += data.Data[i];
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
                x /= Units.Count;
                y /= Units.Count;
                for (int i = 0; i < averageData.Length; i++)
                {
                    averageData[i] /= Units.Count;
                }
                var centroid = new Pixel(averageData, new Point(x, y));
                Center = centroid;

                Units = Units.OrderBy(p => p.Position.X).ThenBy(p => p.Position.Y).ToList();
            }
        }

        public override void AddData(IUnit unut)
        {
            Units.Add(unut);
        }

        public override bool Contains(IUnit pixel)
        {
            for (int i = Units.Count - 1; i >= 0; i--)
            {
                if (Math.Abs(pixel.Position.X - Units[i].Position.X)
                    < 2 && Math.Abs(pixel.Position.Y - Units[i].Position.Y) < 2)
                {
                    return true;
                }
            }
            return false;
        }

        public override void AddDataRange(IEnumerable<IUnit> units)
        {
            Units.AddRange(units);
        }

        public sealed override double[] Generate()
        {
            var c = new RNGCryptoServiceProvider();
            var randomNumber = new byte[3];
            c.GetBytes(randomNumber);

            var r = randomNumber[0];
            var g = randomNumber[1];
            var b = randomNumber[2];

            double[] color = {r, g, b, 255};

            return color;
        }
    }
}
