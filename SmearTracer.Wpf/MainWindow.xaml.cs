using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SmearTracer.Core;
using SmearTracer = SmearTracer.Core.SmearTracer;


namespace SmearTracer.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly List<ImageSource> _images;
        private int _currentImageIndex;
        private Core.SmearTracer _smearTracer;
        private BitmapImage _image;

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
                _image = new BitmapImage(new Uri(fileDialog.FileName));
;
                _smearTracer = new Core.SmearTracer(_image);
                TextBoxSmearHeight.Text = ((int)Math.Sqrt(_smearTracer.MinSize)).ToString();
                TextBoxColorCount.Text = _smearTracer.CountClusters.ToString();
                TextBoxMaxLength.Text = _smearTracer.MaxLength.ToString();

                Image.Source = _smearTracer.Image;

                /*}
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening image: " + ex.Message);
                } */

            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
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
            else if (e.Key == Key.Left)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_image != null)
                {
                    _smearTracer = new Core.SmearTracer(_image, Convert.ToInt32(TextBoxSmearHeight.Text),
                        Convert.ToInt32(TextBoxColorCount.Text), Convert.ToInt32(TextBoxMaxLength.Text));
                }
            }
            catch
            {
                MessageBox.Show("Некорректные значения!");
            }
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_smearTracer != null)
            {
                TextBoxSmearHeight.Text = ((int)Math.Sqrt(_smearTracer.MinSize)).ToString();
                TextBoxColorCount.Text = _smearTracer.CountClusters.ToString();
                TextBoxMaxLength.Text = _smearTracer.MaxLength.ToString();
            }
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            if (_smearTracer != null)
            {
                _smearTracer.Split();
                _images.Add(_smearTracer.SuperPixels());
                _images.Add(_smearTracer.SuperPixelsColor());
                _images.Add(_smearTracer.BrushStrokes());
                _images.Add(_smearTracer.BrushStrokesLines());
                _images.Add(_smearTracer.BrushStroke());

                Image.Source = _images.Last();

                SaveFile(_smearTracer.GetPlt());
            }
        }
    }
}
