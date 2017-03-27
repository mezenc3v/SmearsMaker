using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
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

        private BitmapSource GetImage(BitmapSource image)
        {
            int countSmears = 700;
            int smearSize = image.PixelWidth * image.PixelHeight / countSmears;
            int countClusters = (int)Math.Sqrt(image.PixelWidth + image.PixelHeight) + (int)Math.Sqrt(smearSize) + 3;
            double tolerance = 0.1;
            int maxIter = 100;
            int rankFilter = (int)Math.Sqrt((double)(image.PixelWidth + image.PixelHeight) / 100 / 2);
            int dataFormatSize = image.Format.BitsPerPixel / 8;
            Converters converter = new Converters();
            MedianFilter filter = new MedianFilter(rankFilter, image.PixelWidth, image.PixelHeight);
            KMeans kmeans = new KMeans(countClusters, tolerance, dataFormatSize);
            List<Pixel> imageData = converter.BitmapImageToListPixels(image);
            List<Pixel> filteredData = filter.Compute(imageData);
            List<Pixel> smearsData = new List<Pixel>();
            List<ListSegments> listSegments = new List<ListSegments>();
            //calculate clusters
            kmeans.Compute(filteredData, maxIter, smearSize);
            //creating segments from clusters
            Parallel.ForEach(kmeans.Clusters,
                cluster =>
                {
                    ListSegments clusterSegments = new ListSegments();
                    clusterSegments.Compute(cluster.Data);
                    listSegments.Add(clusterSegments);
                });
            //creating single list of segments 
            ListSegments segments = new ListSegments(listSegments);
            //concatenation small segments
            segments.Concat(smearSize/3);
            foreach (var segment in segments.Segments)
            {
                segment.Data.ForEach(d => d.Data = segment.CentroidPixel.Data);
                smearsData.AddRange(segment.Data);
            }
            //output results
            BitmapSource imageResult = converter.ListPixelsToBitmapImage(image, smearsData);
            LabelStatus.Content = "Done! created " + segments.Segments.Count + " segments into " + kmeans.Clusters.Count + " clusters.";
            return imageResult;
        }

        private void buttonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            //считывание с файла
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter =
                    "JPG Files (*.jpg)|*.jpg|bmp files (*.bmp)|*.bmp|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)" +
                    "|*.png|GIF Files (*.gif)|*.gif|All files (*.*)|*.*",
                RestoreDirectory = true
            };
            if (fileDialog.ShowDialog() == true)
            {
                BitmapImage image = null;
                try
                {
                    image = new BitmapImage(new Uri(fileDialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening image: " + ex.Message);
                }

                ImageDisplay.Source = GetImage(image);
            }
        }
    }
}
