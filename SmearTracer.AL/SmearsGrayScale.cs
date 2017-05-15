using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SmearTracer.AL.Abstract;
using SmearTracer.BLL;

namespace SmearTracer.AL
{
    public class SmearsGrayScale
    {
        private readonly int _countClusters;
        private readonly BitmapImage _image;
        private const int MaxIter = 100;
        private readonly int _minSize;
        private readonly int _maxSize;
        private readonly int _tolerance;
        private readonly int _rankFilter;
        private readonly int _dataFormatSize;
        private Segmentation _data;
        public SmearsGrayScale(BitmapImage image, int countClusters, int smearSize)
        {
            _tolerance = 1;
            _minSize = smearSize;
            _maxSize = smearSize;
            _countClusters = countClusters;
            _image = image;
            _rankFilter = (int)Math.Sqrt((double)(image.PixelWidth + image.PixelHeight) / 100 / 2);
            _dataFormatSize = image.Format.BitsPerPixel / 8;
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
            kmeans.Compute(filteredData, MaxIter);
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

        public List<ISmear> Smears()
        {
            var list = new List<ISmear>();
            ISmear smear = new Smear();
            smear.Points = new List<Point>
            {
                new Point(1,1),
                new Point(3,4),
                new Point(6,7),
                new Point(3,6),
                new Point(4,5)
            };
            smear.Color = Colors.Black;
            list.Add(smear);
            return list;
        }
    }
}
