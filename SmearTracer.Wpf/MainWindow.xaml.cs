using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SmearTracer.Core;


namespace SmearTracer.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly List<ImageSource> _images;
        private int _currentImageIndex;
        //private SmearsMap _layers;
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
;
                var ui = new Core.SmearTracer(image);
                ui.Split();

                _images.Add(ui.SuperPixels());
                _images.Add(ui.SuperPixelsColor());
                _images.Add(ui.BrushStrokes());
                _images.Add(ui.BrushStrokesLines());
                _images.Add(ui.BrushStroke());

                Image.Source = _images.Last();

                SaveFile(ui.GetPlt());
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

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                ButtonRightImage_Click(sender, e);
            }
            else if (e.Key == Key.Left)
            {
                ButtonLeftImage_Click(sender, e);
            }
        }

        private void SaveFile(string plt)
        {
            var fileDialog = new SaveFileDialog
            {
                FileName = "pltFile",
                DefaultExt = ".plt",
                Filter = "Plt Files |.plt",
                RestoreDirectory = true
            };

            if (fileDialog.ShowDialog() == true)
            {
                File.WriteAllText(fileDialog.FileName, plt, Encoding.ASCII);
            }
        }
    }
}
