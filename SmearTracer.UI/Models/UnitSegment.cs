using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using SmearTracer.UI.Abstract;

namespace SmearTracer.UI.Models
{
    public class UnitSegment:Segment
    {
        private Lazy<List<GraphicUnit>> _guLazy = new Lazy<List<GraphicUnit>>(() => UtilsInterface.USplitter.Splitting());

        public UnitSegment()
        {
            Center = new Unit();
            Color = Generate();
            Units = new List<Unit>();
        }

        public override void Update()
        {
            if (Units.Count > 0)
            {
                //coorditates for compute centroid
                double x = 0;
                double y = 0;
                var averageData = new double[Units.First().ArgbArray.Length];
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
                x /= Units.Count;
                y /= Units.Count;
                for (int i = 0; i < averageData.Length; i++)
                {
                    averageData[i] /= Units.Count;
                }
                //var centroid = new Unit(averageData, Units.OrderBy(u=>Math.Sqrt(Math.Pow(u.Position.X - x,2) + Math.Pow(u.Position.Y - y, 2))).First().Position);
                var centroid = new Unit(averageData, new Point(x, y));
                Center = centroid;

                Units = Units.OrderBy(p => p.Position.X).ThenBy(p => p.Position.Y).ToList();
            }
        }

        public override void AddData(Unit unut)
        {
            Units.Add(unut);
        }

        public override bool Contains(Unit unit)
        {
            for (int i = Units.Count - 1; i >= 0; i--)
            {
                if (Math.Abs(unit.Position.X - Units[i].Position.X)
                    < 2 && Math.Abs(unit.Position.Y - Units[i].Position.Y) < 2)
                {
                    return true;
                }
            }
            return false;
        }

        public override void AddDataRange(IEnumerable<Unit> units)
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

            double[] color = { r, g, b, 255 };

            return color;
        }

        public override List<GraphicUnit> GraphicUnits
        {
            get => _guLazy.Value;
            set => _guLazy = new Lazy<List<GraphicUnit>>(() => value);
        }
    }
}
