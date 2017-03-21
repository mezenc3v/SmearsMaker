using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmearTracer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
            int countSmears = 500;
            int smearSize = image.PixelWidth * image.PixelHeight / countSmears;
            int countClusters = (int)Math.Sqrt(image.PixelWidth + image.PixelHeight + smearSize) + 3;
            double tolerance = 0.1;
            int maxIter = 100;
            int rankFilter = 1;
            int rankNetworkLearning = 0;
            int iterLearning = 1;
            int networkSize;
            Converters converter = new Converters();      
            MedianFilter filter = new MedianFilter(rankFilter, image.PixelWidth, image.PixelHeight);
            KMeans kmeans = new KMeans(countClusters, tolerance);
            List<NeuronNetwork> networks = new List<NeuronNetwork>();

            List<Pixel> imageData = converter.BitmapImageToListPixels(image);
            List<Pixel> filteredData = filter.Compute(imageData);
            List<Pixel> clusteredDataImage = kmeans.Compute(filteredData, maxIter);
            List<Pixel> neuronsData = new List<Pixel>();
            foreach (var cluster in kmeans.Clusters)
            {
                networkSize = (int)Math.Sqrt(cluster.Data.Count / smearSize) + 1;
                KohonenNetwork network = new KohonenNetwork(networkSize, networkSize, rankNetworkLearning);
                network.Learning(cluster.Data, iterLearning);
                networks.Add(network.Network);
            }

            foreach (var network in networks)
            {
                foreach (var arrNeurons in network.NeuronsMap)
                {
                    foreach (var neuron in arrNeurons)
                    {
                        neuron.Data.ForEach(p => p.Data = neuron.AverageData);
                        neuronsData.AddRange(neuron.Data);
                    }
                }
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
