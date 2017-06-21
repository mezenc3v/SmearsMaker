using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using SmearTracer.Core.Abstract;
using SmearTracer.Core.Models;

namespace SmearTracer.Core
{
    public class SuperpixelSplitter:IPartsSplitter
    {
        private List<Part> _graphicUnits;
        private readonly Segment _segment;
        private readonly int _minSize;
        private readonly int _maxSize;
        public SuperpixelSplitter(Segment segment, int minSize, int maxSize, double tolerance)
        {
            _minSize = minSize;
            _maxSize = maxSize;
            _segment = segment;
            _graphicUnits = new List<Part>();
        }

        public List<Part> Splitting()
        {
            var maxDiameter = Math.Sqrt(_maxSize);
            var minDiameter = Math.Sqrt(_minSize);
            //spliting complex segment into superPixels
            _graphicUnits = SegmentToSuperPixels(_segment, minDiameter, maxDiameter).ToList();

            /*if (_graphicUnits.Count == 0)
            {
                var diameter = maxDiameter;
                do
                {
                    _graphicUnits.AddRange(SegmentToSuperPixels(_segment, minDiameter, --diameter));
                }
                while (_graphicUnits.Count == 0 && diameter < minDiameter);
            }*/
            _segment.Update();

            return _graphicUnits;
        }

        private static IEnumerable<Part> SegmentToSuperPixels(Part complexSegment, double minDiameter, double maxDiameter)
        {
            var data = complexSegment.Units;
            var superPixelsList = new List<Part>();

            var firstPoint = new Point(complexSegment.MinX.X, complexSegment.MinY.Y);
            var secondPoint = new Point(complexSegment.MaxX.X, complexSegment.MaxY.Y);
            var widthCount = (int)(complexSegment.MaxX.X - complexSegment.MinX.X);
            var heightCount = (int)(complexSegment.MaxY.Y - complexSegment.MinY.Y);
            var samples = InitilalCentroids(widthCount, heightCount, firstPoint, secondPoint, maxDiameter);

            //var smallData = new List<IUnit>();

            var superPixels = samples.Select(centroid => new Superpixel(centroid)).ToList<Part>();
            //Search for winners and distribution of data
            foreach (var unit in data)
            {
                var winner = NearestCentroid(unit, superPixels);
                superPixels[winner].AddData(unit);
            }
            //Deleting empty cells and cells with small data count
            foreach (var superPixel in superPixels)
            {
                if (superPixel.Units.Count > 0)
                {
                    /*if (superPixel.Units.Count < minDiameter * minDiameter)
                    {
                        smallData.AddRange(superPixel.Units);
                    }
                    else
                    {*/
                    superPixelsList.Add(superPixel);
                    //}

                }
            }

            /*if (superPixelsList.Count > 0)
            {
                foreach (var pixel in smallData)
                {
                    var winner = NearestCentroid(pixel, superPixelsList);
                    superPixelsList[winner].AddData(pixel);
                }
            }*/

            return superPixelsList;
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

        private static double Distance(Part superPixel, IUnit pixel)
        {
            var sum = Math.Pow(pixel.Position.X - superPixel.Center.Position.X, 2);
            sum += Math.Pow(pixel.Position.Y - superPixel.Center.Position.Y, 2);
            return Math.Sqrt(sum);
        }

        private static int NearestCentroid(IUnit pixel, IReadOnlyList<Part> superPixels)
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
    }
}
