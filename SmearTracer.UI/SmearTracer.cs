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

        public readonly BitmapSource Image;
        public readonly int CountClusters;
        private readonly double _tolerance;
        private readonly int _maxIter;
        private readonly int _rankFilter;
        private readonly int _width;
        private readonly int _height;
        private readonly int _dataFormatSize;
        public readonly int MinSize;
        public readonly int MaxLength;

        public SmearTracer(BitmapSource image)
        {
            Image = image;
            _width = image.PixelWidth;
            _height = image.PixelHeight;
            const int countSmears = 100;
            var smearSize = _width * _height / countSmears;
            CountClusters = (int)(Math.Sqrt(_width + _height) + Math.Sqrt(_width + _height) / 2) + 3;
            _tolerance = 0.001;
            _maxIter = 100;
            _rankFilter = (int)Math.Sqrt((double)(_width + _height) / 80 / 2) - 1;
            _dataFormatSize = image.Format.BitsPerPixel / 8;
            MinSize = smearSize / 100;
            MaxLength = int.MaxValue - 1;
        }

        public SmearTracer(BitmapSource image, int smearHeight, int countColors, int maxSmearlength)
        {
            Image = image;
            _width = image.PixelWidth;
            _height = image.PixelHeight;

            CountClusters = countColors;
            _tolerance = 0.001;
            _maxIter = 100;
            _rankFilter = (int)Math.Sqrt((double)(_width + _height) / 80 / 2) - 1;
            _dataFormatSize = image.Format.BitsPerPixel / 8;

            MinSize = smearHeight * smearHeight;

            MaxLength = maxSmearlength;
        }

        public void Split()
        {
            var dataSource = Utils.ConvertBitmapImageToPixels(Image);

            Canvas = new Canvas();
            IFilter filter = new MedianFilter(_rankFilter, _width, _height);
            var data = filter.Filtering(dataSource);
            var layer = new Layer(data);
            Canvas.Layers.Add(layer);

            foreach (var canvasLayer in Canvas.Layers)
            {
                var cs = new Kmeans(CountClusters, _tolerance, _dataFormatSize, canvasLayer.Data, _maxIter);
                canvasLayer.Clusters = cs.Clustering();

                Parallel.ForEach(layer.Clusters, cluster =>
                {
                    ISegmentsSplitter ss = new Segmentation(cluster.Data);
                    cluster.Segments = ss.Segmenting();
                });

                Utils.Concat(MinSize, canvasLayer.Clusters);

                Parallel.ForEach(layer.Clusters, cluster =>
                {
                    Parallel.ForEach(cluster.Segments, segment =>
                    {
                        //IPartsSplitter up = new SuperpixelSplitter(segment, _minSize, _minSize + 1, 1);
                        IPartsSplitter up = new KmeansSplitter(segment, MinSize);
                        segment.GraphicUnits = up.Splitting();
                        foreach (var unit in segment.GraphicUnits)
                        {
                            unit.Update();
                        }
                        //ISequenceOfPartMaker maker = new Bsm(segment, _maxLength);
                        ISequenceOfPartMaker maker = new BsmPair(segment, MinSize * 1.5);
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
            return Utils.ConvertPixelsToBitmapImage(Image, units);
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
            return Utils.ConvertPixelsToBitmapImage(Image, units);
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
            return Utils.ConvertPixelsToBitmapImage(Image, units);
        }

        public BitmapSource BrushStrokesLines()
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(Image));
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
                            pen.Width = (MinSize) / 2 + 1;
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
                enc.Frames.Add(BitmapFrame.Create(Image));
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
                pen.Width = (MinSize) / 7;
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
            var heightImage = Image.Height;

            const int height = 5200;
            const int width = 7600;

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

            var widthImage = Image.Width * dy;

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
                foreach (var cluster in layer.Clusters.OrderByDescending(c => c.Centroid.Sum()))
                {
                    plt += "PC" + 1 + "," + (uint)cluster.Centroid[2] + "," + (uint)cluster.Centroid[1] + "," + (uint)cluster.Centroid[0] + ";";

                    foreach (var segment in cluster.Segments.OrderByDescending(s => s.Center.Data.Sum()))
                    {
                        foreach (var brushStroke in segment.BrushStrokes.OrderByDescending(b=>b.Points.Count))
                        {
                            plt += "PU" + (uint)brushStroke.Points.First().Position.X + "," + (uint)(height - brushStroke.Points.First().Position.Y) + ";";

                            for (int i = 1; i < brushStroke.Points.Count - 1; i++)
                            {
                                plt += "PD" + (uint)brushStroke.Points[i].Position.X + "," + (uint)(height - brushStroke.Points[i].Position.Y) + ";";
                            }
                            plt += "PU" + (uint)brushStroke.Points.Last().Position.X + "," + (uint)(height - brushStroke.Points.Last().Position.Y) + ";";
                        }
                    }
                }
            }

            return plt;
        }
    }
}

