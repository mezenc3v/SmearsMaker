using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmearTracer
{
    public class SmearsList
    {
        public List<Smear> Smears { get; set; }
        public List<Circle> Circles { get; set; }
        public int MinRadius { get; set; }
        public int MaxRadius { get; set; }

        public SmearsList(int min, int max)
        {
            Smears = new List<Smear>();
            Circles = new List<Circle>();
            MinRadius = min;
            MaxRadius = max;
        }
        public void Compute(Segment segment)
        {
            Circles.Add(ComputeCircle(segment));
        }

        public void Compute(List<Segment> segments)
        {
            Parallel.ForEach(segments, segment =>
            {
                Circle circle = ComputeCircle(segment);
                lock (Circles)
                {
                    Circles.Add(circle);
                }
            });
        }

        private Circle ComputeCircle(Segment segment)
        {
            var seg = new Segment(segment);

            double xCenter = seg.CentroidPixel.X;
            double yCenter = seg.CentroidPixel.Y;
            double radius = MinRadius;
            Circle circle = new Circle(new Point(xCenter, yCenter), radius);
            Circle newCircle = new Circle(new Point(xCenter, yCenter), circle.Radius);

            double points = 0;
            double newPoints = newCircle.Contains(seg.Data);

            while (newPoints >= points)
            {
                while (newPoints > points)
                {
                    circle = new Circle(new Point(xCenter, yCenter), radius);
                    xCenter--;
                    newCircle = new Circle(new Point(xCenter, yCenter), circle.Radius);
                    points = circle.Contains(seg.Data);
                    newPoints = newCircle.Contains(seg.Data);
                }
                xCenter++;

                circle = new Circle(new Point(xCenter, yCenter), radius);
                yCenter--;
                newCircle = new Circle(new Point(xCenter, yCenter), circle.Radius);
                points = circle.Contains(seg.Data);
                newPoints = newCircle.Contains(seg.Data);
            }
            yCenter++;

            circle = new Circle(new Point(xCenter, yCenter), radius);
            newCircle = new Circle(new Point(xCenter, yCenter), circle.Radius + 1);
            points = circle.Contains(seg.Data);
            radius++;
            newPoints = newCircle.Contains(seg.Data);

            while (newPoints > points && radius < MaxRadius)
            {
                radius++;
                circle = new Circle(new Point(xCenter, yCenter), radius);
                newCircle = new Circle(new Point(xCenter, yCenter), circle.Radius + 1);
                points = circle.Contains(seg.Data);
                newPoints = newCircle.Contains(seg.Data);
            }
            radius--;

            var optimumCircle = new Circle(new Point(xCenter, yCenter), radius);

            return optimumCircle;
        }

    }
}
