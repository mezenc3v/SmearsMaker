using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SmearTracer.BLL;
using SmearTracer.Model;
using SmearTracer.Model.Abstract;

namespace SmearTracer.AL
{
    public class SmearsMap
    {
        private readonly BitmapImage _image;
        private Segmentation _data;
        private readonly int _countClusters;
        private readonly double _tolerance;
        private readonly int _maxIter;
        private readonly int _rankFilter;
        private readonly int _dataFormatSize;
        private readonly int _minSize;
        private readonly int _maxSize;

        public SmearsMap(BitmapImage image)
        {
            _image = image;
            var countSmears = 500;
            var smearSize = image.PixelWidth * image.PixelHeight / countSmears;
            _countClusters = (int)Math.Sqrt(image.PixelWidth + image.PixelHeight) + 1;
            _tolerance = 0.1;
            _maxIter = 100;
            _rankFilter = (int)Math.Sqrt((double)(image.PixelWidth + image.PixelHeight) / 100 / 2);
            _dataFormatSize = image.Format.BitsPerPixel / 8;
            _minSize = smearSize / 10;
            _maxSize = smearSize;
        }

        public void Compute()
        {
            var converter = new Converters();
            var filter = new MedianFilter(_rankFilter, _image.PixelWidth, _image.PixelHeight);
            var kmeans = new Kmeans(_countClusters, _tolerance, _dataFormatSize);
            var imageData = converter.ConvertBitmapImageToPixels(_image);
            var filteredData = filter.Compute(imageData);
            var listSegments = new List<Segmentation>();
            //calculate clusters
            kmeans.Compute(filteredData, _maxIter);
            //creating segmentsList from clusters
            Parallel.ForEach(kmeans.Clusters,
                cluster =>
                {
                    var clusterSegmentsList = new Segmentation();
                    clusterSegmentsList.Compute(cluster.Data);
                    listSegments.Add(clusterSegmentsList);
                });
            //creating single list of complex segmentsList 
            _data = new Segmentation(listSegments);
            //concatenation small segments
            _data.Concat(_minSize);
            //spliting big segmentsList into superpixels
            _data.ComputeSuperPixels(_minSize, _maxSize, _tolerance);
        }

        public BitmapSource Initial()
        {
            return _image;
        }

        public BitmapSource SuperPixels()
        {
            var converter = new Converters();
            var pixels = new List<IUnit>();
            foreach (var segment in _data.Segments)
            {
                foreach (var superPixel in segment.SuperPixels)
                {
                    var seg = superPixel;
                    seg.Update();
                    superPixel.Data.ForEach(d => d.ArgbArray = seg.Center.ArgbArray);
                    pixels.AddRange(superPixel.Data);
                }
            }
            var image = converter.ConvertPixelsToBitmapImage(_image, pixels);
            return image;
        }

        public BitmapSource Circles()
        {
            var converter = new Converters();
            var pixels = new List<IUnit>();
            foreach (var segment in _data.Segments)
            {
                foreach (var pixel in segment.Data)
                {
                    foreach (var circle in segment.CirclesList)
                    {
                        if (circle.Contains(pixel))
                        {
                            pixel.ArgbArray = segment.Center.ArgbArray;
                            //pixel.ArgbArray = circle.Color;
                            pixels.Add(pixel);
                        }
                    }
                }
            }
            var image = converter.ConvertPixelsToBitmapImage(_image, pixels);
            return image;
        }

        public BitmapSource CirclesRndColor()
        {
            var converter = new Converters();
            var pixels = new List<IUnit>();
            foreach (var segment in _data.Segments)
            {
                foreach (var pixel in segment.Data)
                {
                    foreach (var circle in segment.CirclesList)
                    {
                        if (circle.Contains(pixel))
                        {
                            pixel.ArgbArray = circle.Color;
                            pixels.Add(pixel);
                        }
                    }
                }
            }
            var image = converter.ConvertPixelsToBitmapImage(_image, pixels);
            return image;
        }

        public BitmapSource SuperPixelsColor()
        {
            var converter = new Converters();
            var pixels = new List<IUnit>();
            foreach (var segment in _data.Segments)
            {
                foreach (var superPixel in segment.SuperPixels)
                {
                    var seg = new SuperPixel(superPixel);
                    seg.Update();
                    superPixel.Data.ForEach(d => d.ArgbArray = seg.Color);
                    pixels.AddRange(superPixel.Data);
                }
            }
            var image = converter.ConvertPixelsToBitmapImage(_image, pixels);
            return image;
        }

        public BitmapSource VectorMap()
        {
            var converter = new Converters();
            var pixels = new List<IUnit>();
            //
            var image = converter.ConvertPixelsToBitmapImage(_image, pixels);
            return image;
        }

    }
}
