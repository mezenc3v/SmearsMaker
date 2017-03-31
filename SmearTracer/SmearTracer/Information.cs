using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;

namespace SmearTracer
{
    public class Information
    {
        public Pixel GetCenter
        {
            get { return _center; }
        }

        public double[] GetColor { get; }

        public List<Pixel> GetData
        {
            get { return _data; }
        }

        public Point GetMinXPoint
        {
            get { return _minXPoint; }
        }
        public Point GetMinYPoint
        {
            get { return _minYPoint; }
        }
        public Point GetMaxXPoint
        {
            get { return _maxXPoint; }
        }
        public Point GetMaxYPoint
        {
            get { return _maxYPoint; }
        }

        private List<Pixel> _data;
        private Pixel _center;
        private Point _minXPoint;
        private Point _minYPoint;
        private Point _maxXPoint;
        private Point _maxYPoint;

        public Information()
        {
            _center = new Pixel();
            _data = new List<Pixel>();
            GetColor = Generate();
        }

        public Information(double x, double y)
        {
            _center = new Pixel(new Point(x, y));
            _data = new List<Pixel>();
            GetColor = Generate();
        }

        public Information(Point point)
        {
            _center = new Pixel(point);
            _data = new List<Pixel>();
            GetColor = Generate();
        }

        public Information(Information inf)
        {
            _data = inf.GetData;
            _center = inf._center;
            _minXPoint = inf._minXPoint;
            _minYPoint = inf._minYPoint;
            _maxXPoint = inf._maxXPoint;
            _maxYPoint = inf._maxYPoint;
            GetColor = Generate();
        }

        public bool CompareTo(Pixel data)
        {
            for (int i = _data.Count - 1; i >= 0; i--)
            {
                if (Math.Abs(data.PixelPosition.X - _data[i].PixelPosition.X) < 2 && Math.Abs(data.PixelPosition.Y - _data[i].PixelPosition.Y) < 2)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddData(Pixel pixel)
        {
            _data.Add(pixel);
        }

        public void AddRangeData(List<Pixel> pixels)
        {
            _data.AddRange(pixels);
        }

        public void Update()
        {
            if (_data.Count > 0)
            {
                //coorditates for calculate centroid
                double x = 0;
                double y = 0;
                var averageData = new double[_data.First().ArgbArray.Length];
                //coordinates for calculate vector
                var minX = _data[0].PixelPosition.X;
                var minY = _data[0].PixelPosition.Y;
                var maxX = minX;
                var maxY = minY;
                _minXPoint = new Point(_data[0].PixelPosition.X, _data[0].PixelPosition.Y);
                _maxXPoint = new Point(_data[0].PixelPosition.X, _data[0].PixelPosition.Y);
                _minYPoint = new Point(_data[0].PixelPosition.X, _data[0].PixelPosition.Y);
                _maxYPoint = new Point(_data[0].PixelPosition.X, _data[0].PixelPosition.Y);
                foreach (var data in _data)
                {
                    x += data.PixelPosition.X;
                    y += data.PixelPosition.Y;
                    for (int i = 0; i < averageData.Length; i++)
                    {
                        averageData[i] += data.ArgbArray[i];
                    }
                    //find min and max coordinates in segment
                    if (data.PixelPosition.X < minX)
                    {
                        minX = data.PixelPosition.X;
                        _minXPoint = new Point(data.PixelPosition.X, data.PixelPosition.Y);
                    }
                    if (data.PixelPosition.Y < minY)
                    {
                        minY = data.PixelPosition.Y;
                        _minYPoint = new Point(data.PixelPosition.X, data.PixelPosition.Y);
                    }
                    if (data.PixelPosition.X > maxX)
                    {
                        maxX = data.PixelPosition.X;
                        _maxXPoint = new Point(data.PixelPosition.X, data.PixelPosition.Y);
                    }
                    if (data.PixelPosition.Y > maxY)
                    {
                        maxY = data.PixelPosition.Y;
                        _maxYPoint = new Point(data.PixelPosition.X, data.PixelPosition.Y);
                    }
                }
                x /= _data.Count;
                y /= _data.Count;
                for (int i = 0; i < averageData.Length; i++)
                {
                    averageData[i] /= _data.Count;
                }
                var centroid = new Pixel(averageData, (int)x, (int)y);
                _center = centroid;

                _data = _data.OrderBy(p => p.PixelPosition.X).ThenBy(p => p.PixelPosition.Y).ToList();
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
