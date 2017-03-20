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
                /*try
                {*/
                    Converters converter = new Converters();

                    BitmapImage image = new BitmapImage(new Uri(fileDialog.FileName));

                    double[][] imageData = converter.BitmapImageToDoubleArray(image);

                    MedianFilter filter = new MedianFilter(1,image.PixelWidth,image.PixelHeight);

                    double[][] filterImageData = filter.Compute(imageData);

                    int size = (int)Math.Sqrt(image.PixelWidth + image.PixelHeight) + 5;
                    //imageDisplay.Source = image;
                    KMeans kmeans = new KMeans(size, 0.1);

                    List<Pixel> filterData = converter.DoubleArrayToDataPixels(filterImageData, image.PixelWidth,
                        image.PixelHeight);
                    
                    List<Pixel> clusteredDataImage = kmeans.Compute(filterData, 100);

                    
                    //List<NeuronNetwork> networks = new List<NeuronNetwork>();

                    foreach (var cluster in kmeans.Clusters)
                    {
                        int networkSize = cluster.Data.Count / (size * size * size) + 4;
                        KohonenNetwork network = new KohonenNetwork(networkSize, networkSize, 0);
                        network.Learning(cluster.Data, 1);                      
                        imageData = network.PaintDataImage(cluster.Data, imageData, image.PixelHeight);   
                        //networks.Add(network.Network);
                    }           

                    imageDisplay.Source = converter.DoubleArrayToBitmapImage(image, imageData);

                /*}
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }*/
            }
        }
    }
}
