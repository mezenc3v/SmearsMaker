using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;


namespace SmearTracer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            LabelStatus.Content = "Select the path to the image";
        }

        private BitmapSource GetImage(BitmapSource image, int countClusters, int dataFormatSize, int maxIter, int rankFilter, double tolerance, int minSize, int maxSize)
        {          
            var converter = new Converters();
            var filter = new MedianFilter(rankFilter, image.PixelWidth, image.PixelHeight);
            var kmeans = new KMeans(countClusters, tolerance, dataFormatSize);
            var imageData = converter.BitmapImageToListPixels(image);
            var filteredData = filter.Compute(imageData);
            var smearsData = new List<Pixel>();
            var listSegments = new List<SegmentsList>();
            //calculate clusters
            kmeans.Compute(filteredData, maxIter);
            //creating segmentsList from clusters
            Parallel.ForEach(kmeans.Clusters,
                cluster =>
                {
                    var clusterSegmentsList = new SegmentsList();
                    clusterSegmentsList.Compute(cluster.Data);
                    listSegments.Add(clusterSegmentsList);
                });
            //creating single list of complex segmentsList 
            var segmentsList = new SegmentsList(listSegments);
            //concatenation small segmentsList
            segmentsList.Concat(minSize);
            //spliting big segmentsList
            segmentsList.Split(maxSize, tolerance);

            foreach (var segment in segmentsList.Segments)
            {
                segment.Data.ForEach(d => d.Data = segment.Color);
                smearsData.AddRange(segment.Data);
            }
            //output results
            var imageResult = converter.ListPixelsToBitmapImage(image, smearsData);
            LabelStatus.Content = "Done! created " + segmentsList.Segments.Count + " segmentsList into " + kmeans.Clusters.Count + " clusters.";
            return imageResult;
        }

        private void buttonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            //считывание с файла
            var fileDialog = new OpenFileDialog
            {
                Filter =
                    "JPG Files (*.jpg)|*.jpg|bmp files (*.bmp)|*.bmp|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)" +
                    "|*.png|GIF Files (*.gif)|*.gif|All files (*.*)|*.*",
                RestoreDirectory = true
            };
            if (fileDialog.ShowDialog() == true)
            {
                //try
                //{
                    var image = new BitmapImage(new Uri(fileDialog.FileName));

                    var countSmears = 500;
                    var smearSize = image.PixelWidth * image.PixelHeight / countSmears;
                    var countClusters = (int)Math.Sqrt(image.PixelWidth + image.PixelHeight) + 1;
                    var tolerance = 0.1;
                    var maxIter = 100;
                    var rankFilter = (int)Math.Sqrt((double)(image.PixelWidth + image.PixelHeight) / 100 / 2);
                    var dataFormatSize = image.Format.BitsPerPixel / 8;
                    var minSize = smearSize / 4;
                    var maxSize = smearSize;
                    ImageDisplay.Source = GetImage(image, countClusters, dataFormatSize, maxIter, rankFilter, tolerance, minSize, maxSize);
                /*}
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening image: " + ex.Message);
                } */    
            }
        }
    }
}
