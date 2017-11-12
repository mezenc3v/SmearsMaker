using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
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
using Microsoft.Win32;
using SmearsMaker.Logic;

namespace SmearsMaker.Wpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly List<ImageView> _images;
		private SmearTracer _tracer;
		private int _currentImageIndex;
		private Settings _settings;
		private BitmapImage _image;

		public MainWindow()
		{
			InitializeComponent();
			_images = new List<ImageView>();
			_currentImageIndex = 0;
			LabelStatus.Content = "Выберите изображение";
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
				_image = new BitmapImage(new Uri(fileDialog.FileName));
				_images.Add(new ImageView(_image, "Оригинал"));
				Image.Source = _image;
				LabelStatus.Content = "Нажмите кнопку старт";
			}
		}

		private void MainForm_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Right)
			{
				if (_currentImageIndex < _images.Count - 1)
				{
					Image.Source = _images[++_currentImageIndex].Source;
				}
				else
				{
					_currentImageIndex = 0;
					Image.Source = _images[_currentImageIndex].Source;
				}
				LabelStatus.Content = _images[_currentImageIndex].Name;
			}
			else if (e.Key == Key.Left)
			{
				if (_currentImageIndex > 0)
				{
					Image.Source = _images[--_currentImageIndex].Source;
				}
				else
				{
					_currentImageIndex = _images.Count - 1;
					Image.Source = _images[_currentImageIndex].Source;
				}
				LabelStatus.Content = _images[_currentImageIndex].Name;
			}
		}

		private static void SaveFile(string plt)
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
				_settings = new Settings
				{
					ClusterMaxIteration = Convert.ToInt32(TextBoxClusterMaxIteration.Text),
					ClustersPrecision = Convert.ToSingle(TextBoxClustersPrecision.Text),
					MaxSizeSuperpixel = Convert.ToInt32(TextBoxMaxSizeSuperpixel.Text),
					MinSizeSuperpixel = Convert.ToInt32(TextBoxMinSizeSuperpixel.Text),
					MaxSmearDistance = Convert.ToInt32(TextBoxMaxLength.Text),
					ClustersCount = Convert.ToInt32(TextBoxColorCount.Text),
					FilterRank = Convert.ToInt32(TextBoxFilterRank.Text)
				};
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка: {ex.Message}");
			}
		}

		private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
		}

		private async void ButtonRun_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (_image == null) return;

				var context = new ImageContext(_image);

				if (_settings != null)
				{
					context.ClusterMaxIteration = _settings.ClusterMaxIteration;
					context.ClustersPrecision = _settings.ClustersPrecision;
					context.ClustersCount = _settings.ClustersCount;
					context.FilterRank = _settings.FilterRank;
					context.MaxSmearDistance = _settings.MaxSmearDistance;
					context.MaxSizeSuperpixel = _settings.MaxSizeSuperpixel;
					context.MinSizeSuperpixel = _settings.MinSizeSuperpixel;
				}

				_tracer = new SmearTracer(context);
				var progressBar = new Logic.ProgressBar();
				progressBar.NewStep += UpdateLabel;

				await _tracer.Execute(progressBar);
				
				var sprpxls = _tracer.SuperPixels();
				var clusters = _tracer.Clusters();
				var segments = _tracer.Segments();
				var brushsrks = _tracer.BrushStrokes();

				_images.Add(new ImageView(_image, "Оригинал"));
				_images.Add(new ImageView(sprpxls, "Суперпиксели"));
				_images.Add(new ImageView(clusters, "Кластеры"));
				_images.Add(new ImageView(segments, "Сегменты"));
				_images.Add(new ImageView(brushsrks, "Мазки"));

				Image.Source = clusters;
				LabelStatus.Content = _images[_currentImageIndex].Name;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка! {ex.Message}");
			}
		}

		private void ButtonPtlSave_Click(object sender, RoutedEventArgs e)
		{
			if (_tracer != null)
			{
				try
				{
					SaveFile(_tracer.GetPlt());
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Ошибка! {ex.Message}");
				}
			}
		}

		private void UpdateLabel(object sender, ProgressBarEventArgs args)
		{
			Dispatcher.Invoke(() =>
			{
				LabelStatus.Content = args.Msg;
			});
		}
	}
}

