using System.Collections.Generic;
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

        private Circle ComputeCircle(Segment inputSegment)
        {
            var segment = new Segment(inputSegment);

            double xCenter = segment.Information.GetCenter.PixelPosition.X;
            double yCenter = segment.Information.GetCenter.PixelPosition.Y;
            double radius = MinRadius;
            var oldCircle = new Circle(new Point(xCenter, yCenter), radius);
            var newCircle = new Circle(new Point(xCenter, yCenter), oldCircle.Radius);

            double points = 0;
            double newPoints = newCircle.Contains(segment.Information.GetData);

            while (newPoints >= points)
            {
                while (newPoints > points)
                {
                    oldCircle = new Circle(new Point(xCenter, yCenter), radius);
                    xCenter--;
                    newCircle = new Circle(new Point(xCenter, yCenter), oldCircle.Radius);
                    points = oldCircle.Contains(segment.Information.GetData);
                    newPoints = newCircle.Contains(segment.Information.GetData);
                }
                xCenter++;

                oldCircle = new Circle(new Point(xCenter, yCenter), radius);
                yCenter--;
                newCircle = new Circle(new Point(xCenter, yCenter), oldCircle.Radius);
                points = oldCircle.Contains(segment.Information.GetData);
                newPoints = newCircle.Contains(segment.Information.GetData);
            }
            yCenter++;

            oldCircle = new Circle(new Point(xCenter, yCenter), radius);
            newCircle = new Circle(new Point(xCenter, yCenter), oldCircle.Radius + 1);
            points = oldCircle.Contains(segment.Information.GetData);
            radius++;
            newPoints = newCircle.Contains(segment.Information.GetData);

            while (newPoints > points && radius < MaxRadius)
            {
                radius++;
                oldCircle = new Circle(new Point(xCenter, yCenter), radius);
                newCircle = new Circle(new Point(xCenter, yCenter), oldCircle.Radius + 1);
                points = oldCircle.Contains(segment.Information.GetData);
                newPoints = newCircle.Contains(segment.Information.GetData);
            }
            radius--;

            var optimumCircle = new Circle(new Point(xCenter, yCenter), radius);

            return optimumCircle;
        }

    }
}
