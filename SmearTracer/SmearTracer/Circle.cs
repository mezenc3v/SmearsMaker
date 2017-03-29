using System;
using System.Collections.Generic;
using System.Windows;

namespace SmearTracer
{
    public class Circle
    {
        public Point Center { get; set; }
        public double Radius { get; set; }

        public Circle(Point center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public bool Contains(Pixel pixel)
        {      
            if (Math.Pow(pixel.X - Center.X, 2) + Math.Pow(pixel.Y - Center.Y, 2) <= Radius)
            {
                return true;
            }
            return false;
        }

        public int Contains(List<Pixel> data)
        {
            int counter = 0;
            foreach (var pixel in data)
            {
                if (Contains(pixel))
                {
                    counter++;
                }
            }
            return counter;
        }
    }
}
