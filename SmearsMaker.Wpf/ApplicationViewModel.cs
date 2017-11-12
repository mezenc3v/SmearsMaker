using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SmearsMaker.Logic;

namespace SmearsMaker.Wpf
{
	public class ApplicationViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public SettingsViewModel Settings { get; set; }

		public ICommand OpenImage { get;}
		public ICommand SavePlt { get; }
		public ICommand SaveImages { get; }

		public string Label
		{
			get => _label;
			set
			{
				_label = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Label)));
			}
		}

		public ImageSource CurrentImage
		{
			get => _currentImage;
			set
			{
				_currentImage = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentImage)));
			}
		}

		private string _label;
		private ImageSource _currentImage;
		private readonly List<ImageViewModel> _images;
		private int _currentImageIndex;
		private SmearTracer _tracer;
		private BitmapImage _image;

		public ApplicationViewModel()
		{
			OpenImage = new Command(OpenFile);
			SavePlt = new Command(SavePtl);
			SaveImages = new Command(SaveImagesInFolder);
			Settings = new SettingsViewModel();
			_images = new List<ImageViewModel>();
			_currentImageIndex = 0;
			Label = "Выберите изображение";
		}

		public async Task Run()
		{
			try
			{
				if (_image == null) return;

				var context = new ImageModel(_image);

				if (Settings != null)
				{
					context.ClusterMaxIteration = Settings.ClusterMaxIteration;
					context.ClustersPrecision = Settings.ClustersPrecision;
					context.ClustersCount = Settings.ClustersCount;
					context.FilterRank = Settings.FilterRank;
					context.MaxSmearDistance = Settings.MaxSmearDistance;
					context.MaxSizeSuperpixel = Settings.MaxSizeSuperpixel;
					context.MinSizeSuperpixel = Settings.MinSizeSuperpixel;
					context.HeightPlt = Settings.HeightPlt;
					context.WidthPlt = Settings.WidthPlt;
				}

				var progressBar = new ProgressBar();
				progressBar.UpdateProgress += UpdateProgress;
				_tracer = new SmearTracer(context, progressBar);

				await _tracer.Execute();

				var sprpxls = _tracer.SuperPixels();
				var clusters = _tracer.Clusters();
				var segments = _tracer.Segments();
				var brushsrks = _tracer.BrushStrokes();

				_images.Add(new ImageViewModel(_image, "Оригинал"));
				_images.Add(new ImageViewModel(sprpxls, "Суперпиксели"));
				_images.Add(new ImageViewModel(clusters, "Кластеры"));
				_images.Add(new ImageViewModel(segments, "Сегменты"));
				_images.Add(new ImageViewModel(brushsrks, "Мазки"));

				CurrentImage = _images[_currentImageIndex].Source;
				Label = _images[_currentImageIndex].Name;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка! {ex.Message}");
			}
		}

		private void UpdateProgress(object sender, ProgressBarEventArgs args)
		{
			Label = args.Percentage != 0 ? $"{args.Msg} {args.Percentage}%" : args.Msg;
		}
		public void SavePtl()
		{
			if (_tracer != null)
			{
				try
				{
					var plt = _tracer.GetPlt();

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
				catch (Exception ex)
				{
					MessageBox.Show($"Ошибка! {ex.Message}");
				}
			}
		}

		private void OpenFile()
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
				_images.Add(new ImageViewModel(_image, "Оригинал"));
				CurrentImage = _image;
				Label = "Нажмите кнопку старт";
				Settings.Update(_image.PixelWidth, _image.PixelHeight);
			}
		}

		private void SaveImagesInFolder()
		{
			var sf = new SaveFileDialog()
			{
				FileName = "select folder"
			};

			if (sf.ShowDialog() != true) return;
			var path = Path.GetDirectoryName(sf.FileName);
			foreach (var image in _images)
			{
				var encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(image.Source));
				if (path == null) continue;
				using (var filestream = new FileStream(Path.Combine(path, $"{image.Name}.bmp"), FileMode.Create))
				{
					encoder.Save(filestream);
				}
			}
		}

		public void ChangeImage(KeyEventArgs e)
		{
			if (_images.Count <= 0)
			{
				return;
			}
			switch (e.Key)
			{
				case Key.Right:
					if (_currentImageIndex < _images.Count - 1)
					{
						CurrentImage = _images[++_currentImageIndex].Source;
					}
					else
					{
						_currentImageIndex = 0;
						CurrentImage = _images[_currentImageIndex].Source;
					}
					Label = _images[_currentImageIndex].Name;
					break;
				case Key.Left:
					if (_currentImageIndex > 0)
					{
						CurrentImage = _images[--_currentImageIndex].Source;
					}
					else
					{
						_currentImageIndex = _images.Count - 1;
						CurrentImage = _images[_currentImageIndex].Source;
					}
					Label = _images[_currentImageIndex].Name;
					break;
				default: return;
			}
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string prop = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}
	}
}