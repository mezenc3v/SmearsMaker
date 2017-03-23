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
        }

        private void mainForm_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private BitmapSource GetImage(BitmapSource image)
        {
            int countSmears = 400;
            int smearSize = image.PixelWidth * image.PixelHeight / countSmears;
            int countClusters = (int)Math.Sqrt(image.PixelWidth + image.PixelHeight) + (int)Math.Sqrt(smearSize) + 1;
            double tolerance = 0.1;
            int maxIter = 100;
            int rankFilter = 1;
            int iterLearning = 1;
            int dataFormatSize = image.Format.BitsPerPixel / 8;
            Converters converter = new Converters();
            MedianFilter filter = new MedianFilter(rankFilter, image.PixelWidth, image.PixelHeight);
            KMeans kmeans = new KMeans(countClusters, tolerance, dataFormatSize);
            List<Neuron> networkList = new List<Neuron>();
            List<Pixel> imageData = converter.BitmapImageToListPixels(image);
            List<Pixel> filteredData = filter.Compute(imageData);
            kmeans.Compute(filteredData, maxIter, smearSize);
            List<Pixel> neuronsData = new List<Pixel>();
            Parallel.ForEach(kmeans.Clusters,
                cluster =>
                {
                    var networkSize = cluster.Data.Count / smearSize + 3;
                    KohonenNetwork net = new KohonenNetwork();
                    net.Learning(cluster.Data, iterLearning, networkSize);
                    foreach (var neuron in net.Network.NeuronsList)
                    {
                        networkList.Add(neuron);
                    }
                });
            KohonenNetwork network = new KohonenNetwork(networkList);

            foreach (var neuron in network.Network.NeuronsList)
            {
                neuron.Data.ForEach(p => p.Data = neuron.AverageData);
                neuronsData.AddRange(neuron.Data);
            }
            BitmapSource imageResult = converter.ListPixelsToBitmapImage(image, neuronsData);
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

                imageDisplay.Source = GetImage(image);
            }
        }
    }
}
