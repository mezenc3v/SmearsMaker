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
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "image files (*.bmp)|*.bmp|All files (*.*)|*.*";
            fileDialog.FilterIndex = 2;
            fileDialog.RestoreDirectory = true;
            BitmapImage image = new BitmapImage();
            if (fileDialog.ShowDialog() == true)
            {
                try
                {
                    image = new BitmapImage(new Uri(fileDialog.FileName));
                    //imageDisplay.Source = image;
                    KMeans kmeans = new KMeans(5);
                    imageDisplay.Source = kmeans.Compute(image);
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }
    }
}
