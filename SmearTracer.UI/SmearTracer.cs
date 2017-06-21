using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using SmearTracer.Core.Abstract;
using SmearTracer.Core.Models;

namespace SmearTracer.Core
{
    public class SmearTracer
    {
        public Canvas Canvas;

        private readonly BitmapSource _image;
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

        public SmearTracer(BitmapSource image)
        {
            _image = image;
            _width = image.PixelWidth;
            _height = image.PixelHeight;
            const int countSmears = 100;
            var smearSize = _width * _height / countSmears;
            _countClusters = (int)Math.Sqrt(_width + _height) + (int)Math.Sqrt(_width + _height) / 2 + 3;
            _tolerance = 0.001;
            _maxIter = 100;
            _rankFilter = (int)Math.Sqrt((double)(_width + _height) / 80 / 2) - 1;
            _dataFormatSize = image.Format.BitsPerPixel / 8;
            _minSize = smearSize / 100;
            _maxSize = smearSize;
            _maxLength = _minSize * 100000000;
        }

        public void Split()
        {
            var dataSource = Utils.ConvertBitmapImageToPixels(_image);

            Canvas = new Canvas();
            IFilter filter = new MedianFilter(_rankFilter, _width, _height);
            var data = filter.Filtering(dataSource);
            var layer = new Layer(data);
            Canvas.Layers.Add(layer);

            foreach (var canvasLayer in Canvas.Layers)
            {
                var cs = new Kmeans(_countClusters, _tolerance, _dataFormatSize, canvasLayer.Data, _maxIter);
                canvasLayer.Clusters = cs.Clustering();

                Parallel.ForEach(layer.Clusters, cluster =>
                {
                    ISegmentsSplitter ss = new Segmentation(cluster.Data, _maxSize);
                    cluster.Segments = ss.Segmenting();
                });

                Utils.Concat(_minSize, canvasLayer.Clusters);

                Parallel.ForEach(layer.Clusters, cluster =>
                {
                    Parallel.ForEach(cluster.Segments, segment =>
                    {
                        //IPartsSplitter up = new SuperpixelSplitter(segment, _minSize, _minSize + 1, 1);
                        IPartsSplitter up = new KmeansSplitter(segment, _minSize);
                        segment.GraphicUnits = up.Splitting();
                        foreach (var unit in segment.GraphicUnits)
                        {
                            unit.Update();
                        }
                        //ISequenceOfPartMaker maker = new Bsm(segment, _maxLength);
                        ISequenceOfPartMaker maker = new BsmPair(segment, _minSize * 1.5);
                        segment.BrushStrokes = maker.Execute();
                    });
                });

                /*foreach (var cluster in layer.Clusters)
                {
                    foreach (var segment in cluster.Segments)
                    {
                        //ISequenceOfPartMaker maker = new Bsm(segment, _maxLength);
                        ISequenceOfPartMaker maker = new BsmPair(segment, _minSize * 1.5);
                        segment.BrushStrokes = maker.Execute();
                    }
                }*/
            }
        }

        public BitmapSource SuperPixels()
        {
            var units = new List<IUnit>();

            foreach (var layer in Canvas.Layers)
            {
                foreach (var cluster in layer.Clusters)
                {
                    foreach (var segment in cluster.Segments)
                    {
                        foreach (var superpixel in segment.GraphicUnits)
                        {
                            superpixel.Update();
                            superpixel.Units.ForEach(d => d.Data = superpixel.Center.Data);
                            units.AddRange(superpixel.Units);
                        }
                    }
                }
            }
            return Utils.ConvertPixelsToBitmapImage(_image, units);
        }

        public BitmapSource SuperPixelsColor()
        {
            var units = new List<IUnit>();

            foreach (var layer in Canvas.Layers)
            {
                foreach (var cluster in layer.Clusters)
                {
                    foreach (var segment in cluster.Segments)
                    {
                        foreach (var superpixel in segment.GraphicUnits)
                        {
                            superpixel.Update();
                            superpixel.Units.ForEach(d => d.Data = superpixel.Color);
                            units.AddRange(superpixel.Units);
                        }
                    }
                }
            }
            return Utils.ConvertPixelsToBitmapImage(_image, units);
        }

        public BitmapSource BrushStrokes()
        {
            var units = new List<IUnit>();

            foreach (var layer in Canvas.Layers)
            {
                foreach (var cluster in layer.Clusters)
                {
                    foreach (var segment in cluster.Segments)
                    {
                        foreach (var brushStroke in segment.BrushStrokes)
                        {
                            var c = new RNGCryptoServiceProvider();
                            var randomNumber = new byte[3];
                            c.GetBytes(randomNumber);

                            var r = randomNumber[0];
                            var g = randomNumber[1];
                            var b = randomNumber[2];

                            double[] color = { r, g, b, 255 };

                            units.AddRange(brushStroke.Points.Select(point => new Pixel(color, point.Position)));
                            //units.AddRange(brushStroke.Points.Select(point => new Pixel(brushStroke.Color, point.Position)));
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
                foreach (var cluster in layer.Clusters.OrderByDescending(c => c.Data.Count))
                {
                    foreach (var segment in cluster.Segments.OrderByDescending(s => s.Units.Count))
                    {
                        foreach (var brushStroke in segment.BrushStrokes.OrderByDescending(p => p.GetLength()))
                        {
                            var pen = new Pen(Color.FromArgb((byte)brushStroke.Color[3], (byte)brushStroke.Color[2],
                                (byte)brushStroke.Color[1], (byte)brushStroke.Color[0]));

                            var brush = new SolidBrush(Color.FromArgb((byte)brushStroke.Color[3], (byte)brushStroke.Color[2],
                                (byte)brushStroke.Color[1], (byte)brushStroke.Color[0]));

                            /*var pen = new Pen(Color.FromArgb((byte)cluster.Centroid[3], (byte)cluster.Centroid[2],
                                (byte)cluster.Centroid[1], (byte)cluster.Centroid[0]));

                            var brush = new SolidBrush(Color.FromArgb((byte)cluster.Centroid[3],
                                (byte)cluster.Centroid[2],
                                (byte)cluster.Centroid[1], (byte)cluster.Centroid[0]));*/

                            var pointsF = brushStroke.Points.Select(point => new PointF((int)point.Position.X, (int)point.Position.Y)).ToArray();
                            pen.Width = (_minSize) / 7;
                            if (pointsF.Length > 1)
                            {
                                g.FillEllipse(brush, pointsF.First().X, pointsF.First().Y, pen.Width, pen.Width);
                                g.FillEllipse(brush, pointsF.Last().X, pointsF.Last().Y, pen.Width, pen.Width);
                                g.DrawLines(pen, pointsF);
                                //g.DrawCurve(pen, pointsF);
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

        public BitmapSource BrushStroke()
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

            var brushStrokes = new List<SequenceOfParts>();
            foreach (var layer in Canvas.Layers)
            {
                foreach (var cluster in layer.Clusters.OrderByDescending(c => c.Data.Count))
                {
                    foreach (var segment in cluster.Segments.OrderByDescending(s => s.Units.Count))
                    {
                        foreach (var brushStroke in segment.BrushStrokes.OrderByDescending(p => p.GetLength()))
                        {
                            brushStrokes.Add(brushStroke);
                        }
                    }
                }
            }

            for (int i = 0; i < brushStrokes.Count; i++)
            {
                var pen = new Pen(Color.FromArgb((byte)brushStrokes[i].Color[3], (byte)brushStrokes[i].Color[2],
                    (byte)brushStrokes[i].Color[1], (byte)brushStrokes[i].Color[0]));

                var brush = new SolidBrush(Color.FromArgb((byte)brushStrokes[i].Color[3], (byte)brushStrokes[i].Color[2],
                    (byte)brushStrokes[i].Color[1], (byte)brushStrokes[i].Color[0]));

                var pointsF = brushStrokes[i].Points.Select(point => new PointF((int)point.Position.X, (int)point.Position.Y)).ToArray();
                pen.Width = (_minSize) / 7;
                if (pointsF.Length > 1)
                {
                    g.FillEllipse(brush, pointsF.First().X, pointsF.First().Y, pen.Width * 2, pen.Width * 3);
                    g.FillEllipse(brush, pointsF.Last().X, pointsF.Last().Y * 2, pen.Width * 2, pen.Width * 3);
                    g.DrawLines(pen, pointsF);
                    //g.DrawCurve(pen, pointsF);
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

            g.Dispose();

            var bmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return bmp;
        }

        public string GetPlt()
        {
            var heightImage = _image.Height;

            //resize image
            const int height = 130;
            const int width = 190;


            var dy = (float)(height / heightImage);

            foreach (var layer in Canvas.Layers)
            {
                foreach (var cluster in layer.Clusters)
                {
                    foreach (var segment in cluster.Segments)
                    {
                        foreach (var brushStroke in segment.BrushStrokes)
                        {
                            foreach (var point in brushStroke.Points)
                            {
                                point.Position = new System.Windows.Point(point.Position.X * dy, point.Position.Y * dy);
                            }
                        }
                    }
                }
            }

            var widthImage = _image.Width * dy;

            if (widthImage > width)
            {
                var dx = (float)(width / widthImage);

                foreach (var layer in Canvas.Layers)
                {
                    foreach (var cluster in layer.Clusters)
                    {
                        foreach (var segment in cluster.Segments)
                        {
                            foreach (var brushStroke in segment.BrushStrokes)
                            {
                                foreach (var point in brushStroke.Points)
                                {
                                    point.Position = new System.Windows.Point(point.Position.X * dx, point.Position.Y * dx);
                                }
                            }
                        }
                    }
                }
            }


            //building string
            var plt = "IN;";

            foreach (var layer in Canvas.Layers)
            {
                foreach (var cluster in layer.Clusters.OrderBy(c => c.Centroid.Sum()))
                {
                    foreach (var segment in cluster.Segments.OrderBy(s => s.Center.Data.Sum()))
                    {
                        foreach (var brushStroke in segment.BrushStrokes)
                        {
                            plt += "PC" + 1 + (uint)brushStroke.Color[1] + "," + (uint)brushStroke.Color[2] + (uint)brushStroke.Color[3] + ";";
                            plt += "PU" + (uint)brushStroke.Points.First().Position.X + "," + (uint)brushStroke.Points.First().Position.Y + ";";

                            for (int i = 1; i < brushStroke.Points.Count - 1; i++)
                            {
                                plt += "PD" + (uint)brushStroke.Points[i].Position.X + "," + (uint)brushStroke.Points[i].Position.Y + ";";
                            }
                            plt += "PU" + (uint)brushStroke.Points.Last().Position.X + "," + (uint)brushStroke.Points.Last().Position.Y + ";";
                        }
                    }
                }
            }

            return plt;
        }
    }
}

