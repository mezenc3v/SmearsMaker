using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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
            //concatenation small segments
            segmentsList.Concat(minSize);
            //spliting big segmentsList into superpixels
            segmentsList.ComputeSuperPixels(minSize,maxSize, tolerance);

            foreach (var segment in segmentsList.Segments)
            {
                foreach (var superPixel in segment.SuperPixels)
                {
                    var seg = new Segment {Data = superPixel.Data};
                    seg.Update();
                    superPixel.Data.ForEach(d => d.Data = seg.CentroidPixel.Data);
                    smearsData.AddRange(superPixel.Data);
                }
            }

            /*foreach (var segment in segmentsList.Segments)
            {
                foreach (var pixel in segment.Data)
                {
                    foreach (var circle in segment.CirclesList)
                    {
                        if (circle.Contains(pixel))
                        {
                            smearsData.Add(pixel);
                        }
                    }
                }
                foreach (var superPixel in segment.SuperPixels)
                {
                    foreach (var circle in segment.CirclesList)
                    {
                        for (int i = 0; i < circle.Radius * 2; i++)
                        {
                            for (int j = 0; j < circle.Radius * 2; j++)
                            {
                                var x = circle.Center.X - circle.Radius + i;
                                var y = circle.Center.Y - circle.Radius + j;
                                if (circle.Contains(new Point(x,y)))
                                {
                                    var pixel = new Pixel(superPixel.Color, new Point(x, y));
                                    smearsData.Add(pixel);
                                };
                            }
                        }
                    }

                    foreach (var pixel in superPixel.Data)
                    {
                        pixel.Data = superPixel.Color;
                        smearsData.Add(pixel);
                    }
                }
            }*/

            //output results
            var imageResult = converter.ListPixelsToBitmapImage(image, smearsData);
            LabelStatus.Content = "Done! created " + segmentsList.Segments.Count + " segmentsList into " + kmeans.Clusters.Count + " clusters.";
            return imageResult;
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
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
                    var minSize = smearSize / 10;
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
