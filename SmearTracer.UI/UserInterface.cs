using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SmearTracer.UI.Abstract;
using SmearTracer.UI.Models;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace SmearTracer.UI
{
    public class UserInterface
    {
        public Canvas Canvas;

        private readonly BitmapSource _image;
        private List<Unit> _data;
        private readonly int _countClusters;
        private readonly double _tolerance;
        private readonly int _maxIter;
        private readonly int _rankFilter;
        private readonly int _width;
        private readonly int _height;
        private readonly int _dataFormatSize;
        private readonly int _minSize;
        private readonly int _maxSize;
        private readonly int _maxLength;

        public UserInterface(BitmapSource image)
        {
            _image = image;
            _data = Utils.ConvertBitmapImageToPixels(image);

            Canvas = new PictureCanvas();

            _width = image.PixelWidth;
            _height = image.PixelHeight;
            const int countSmears = 10000;
            var smearSize = _width * _height / countSmears;
            _countClusters = (int)Math.Sqrt(_width + _height) + 1;
            _tolerance = 0.1;
            _maxIter = 100;
            _rankFilter = (int)Math.Sqrt((double)(_width + _height) / 100 / 2);
            _dataFormatSize = image.Format.BitsPerPixel / 8;
            _minSize = smearSize / 20;
            _maxSize = smearSize;

            _maxLength = _maxSize * 3;

        }

        public void Split()
        {
            UtilsInterface.Filter = new MedianFilter(_rankFilter, _width, _height);
            _data = UtilsInterface.Filter.Filtering(_data);

            Canvas.Layers.Add(new PictureLayer());

            foreach (var layer in Canvas.Layers)
            {
                UtilsInterface.CSplitter = new Kmeans(_countClusters, _tolerance, _dataFormatSize, _data, _maxIter);

                foreach (var cluster in layer.Clusters)
                {
                    UtilsInterface.SSplitter = new Segmentation(cluster.Data, _maxSize);
                    var segments = cluster.Segments;
                }

                Utils.Concat(_minSize, layer.Clusters);

                foreach (var cluster in layer.Clusters)
                {
                    foreach (var segment in cluster.Segments)
                    {
                        UtilsInterface.USplitter = new SuperpixelSplitter(segment, _minSize, _maxSize, _tolerance);
                        foreach (var unit in segment.GraphicUnits)
                        {
                            unit.Update();
                        }
                    }
                }

                Parallel.ForEach(layer.Clusters, cluster =>
                {
                    foreach (var seg in cluster.Segments)
                    {
                        var maker = new BrushStrokesMaker(seg, _maxLength);
                        seg.BrushStrokes = maker.Make();
                    }
                });
            }
        }

        public BitmapSource SuperPixels()
        {
            var units = new List<Unit>();

            foreach (var layer in Canvas.Layers)
            {
                foreach (var cluster in layer.Clusters)
                {
                    foreach (var segment in cluster.Segments)
                    {
                        foreach (var superpixel in segment.GraphicUnits)
                        {
                            superpixel.Update();
                            superpixel.Units.ForEach(d => d.ArgbArray = superpixel.Center.ArgbArray);
                            units.AddRange(superpixel.Units);
                        }
                    }
                }
            }
            return Utils.ConvertPixelsToBitmapImage(_image, units);
        }

        public BitmapSource SuperPixelsColor()
        {
            var units = new List<Unit>();

            foreach (var layer in Canvas.Layers)
            {
                foreach (var cluster in layer.Clusters)
                {
                    foreach (var segment in cluster.Segments)
                    {
                        foreach (var superpixel in segment.GraphicUnits)
                        {
                            superpixel.Update();
                            superpixel.Units.ForEach(d => d.ArgbArray = superpixel.Color);
                            units.AddRange(superpixel.Units);
                        }
                    }
                }
            }
            return Utils.ConvertPixelsToBitmapImage(_image, units);
        }

        public BitmapSource BrushStrokes()
        {
            var units = new List<Unit>();

            var p1 = 0;
            var p2 = 0;

            foreach (var layer in Canvas.Layers)
            {
                foreach (var cluster in layer.Clusters)
                {
                    foreach (var segment in cluster.Segments)
                    {
                        p1 += segment.GraphicUnits.Count;
                        foreach (var brushStroke in segment.BrushStrokes)
                        {
                            units.AddRange(brushStroke.Points.Select(point => new Unit(brushStroke.Color, point)));
                            p2 += brushStroke.Points.Count;
                        }
                    }
                }
            }
            return Utils.ConvertPixelsToBitmapImage(_image, units);
        }

        public BitmapSource BrushStrokesLines()
        {

            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(_image));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }

            var g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);

            foreach (var layer in Canvas.Layers)
            {
                foreach (var cluster in layer.Clusters)
                {
                    foreach (var segment in cluster.Segments)
                    {
                        foreach (var brushStroke in segment.BrushStrokes)
                        {
                            var pen = new Pen(Color.FromArgb((byte) cluster.Centroid[3], (byte) cluster.Centroid[2],
                                (byte) cluster.Centroid[1], (byte) cluster.Centroid[0]));

                            var brush = new SolidBrush(Color.FromArgb((byte) cluster.Centroid[3],
                                (byte) cluster.Centroid[2],
                                (byte) cluster.Centroid[1], (byte) cluster.Centroid[0]));

                            //var pen = new Pen(Color.FromArgb((byte)segment.Center.ArgbArray[0], (byte)segment.Center.ArgbArray[1], (byte)segment.Center.ArgbArray[2], (byte)segment.Center.ArgbArray[3]));
                            //var pen = new Pen(Color.FromArgb(255, (byte)segment.Color[1], (byte)segment.Color[2], (byte)segment.Color[3]));
                            var pointsF = brushStroke.Points.Select(point => new PointF((float)point.X, (float)point.Y)).ToArray();
                            pen.Width = _minSize + 9;
                            if (pointsF.Length > 1)
                            {
                                //g.DrawLines(pen, pointsF);
                                g.DrawCurve(pen, pointsF);
                                /*for (int i = 0; i < pointsF.Length; i++)
                                {
                                    if (i + 1 < pointsF.Length)
                                    {
                                        g.DrawLine(pen, pointsF[i], pointsF[i + 1]);
                                    }
                                }*/
                            }
                            else
                            {
                                g.FillEllipse(brush, pointsF.First().X, pointsF.First().Y, pen.Width, pen.Width);
                            }
                        }
                    }
                }
            }

            g.Dispose();

            var bmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return bmp;
        }
    }
}

