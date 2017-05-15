using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SmearTracer.AL;


namespace SmearTracer.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly List<ImageSource> _images;
        private int _currentImageIndex;
        private SmearsMap _layers;
        public MainWindow()
        {
            InitializeComponent();
            _images = new List<ImageSource>();
            _currentImageIndex = 0;
            //LabelStatus.Content = "Select the path to the image";
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

                _layers = new SmearsMap(image);
                _layers.Compute();
                _images.Add(_layers.Initial());
                _images.Add(_layers.SuperPixels());
                _images.Add(_layers.VectorMap());
                _images.Add(_layers.SuperPixelsColor());
                _images.Add(_layers.Circles());
                Image.Source = _images[0];

                /*}
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening image: " + ex.Message);
                } */
            }
        }

        private void ButtonRightImage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentImageIndex < _images.Count - 1)
            {
                Image.Source = _images[++_currentImageIndex];
            }
            else
            {
                _currentImageIndex = 0;
                Image.Source = _images[_currentImageIndex];
            }
        }

        private void ButtonLeftImage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentImageIndex > 0)
            {
                Image.Source = _images[--_currentImageIndex];
            }
            else
            {
                _currentImageIndex = _images.Count - 1;
                Image.Source = _images[_currentImageIndex];
            }
        }
    }
}
