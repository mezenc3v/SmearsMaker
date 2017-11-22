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
using NLog;
using SmearsMaker.Common;
using SmearsMaker.SmearTracer;

namespace SmearsMaker.Wpf
{
	public class ApplicationViewModel : INotifyPropertyChanged
	{
		public enum Algorithms { SmearTracer }

		private Algorithms _currentAlg;

		public event PropertyChangedEventHandler PropertyChanged;
		public List<ImageSetting> Settings { get; set; }

		public ICommand OpenImage { get; }
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
		private List<ImageViewModel> _images;
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
		private int _currentImageIndex;
		private ITracer _tracer;
		private BitmapImage _image;

		public ApplicationViewModel()
		{
			OpenImage = new Command(OpenFile);
			SavePlt = new Command(SavePtl);
			SaveImages = new Command(SaveImagesInFolder);
			_images = new List<ImageViewModel>();
			_currentImageIndex = 0;
			Label = "Выберите изображение";
		}

		public void SetAlgorithm(Algorithms alg)
		{
			if (alg == Algorithms.SmearTracer && _image != null)
			{
				var progressBar = new Progress();
				progressBar.UpdateProgress += UpdateProgress;
				_tracer = new Analyzer(_image, progressBar);
				Settings = _tracer.Settings;
				_currentAlg = alg;
			}
		}

		public async Task Run()
		{
			try
			{
				if (_image == null) return;
				if (_tracer == null)
				{
					SetAlgorithm(_currentAlg);
				}
				await _tracer.Execute();

				var views = _tracer.Views;
				_images = new List<ImageViewModel>();
				_images.AddRange(views);

				CurrentImage = _images[_currentImageIndex].Source;
				Label = _images[_currentImageIndex].Name;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка! {ex.Message}");
				Log.Error(ex);
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
				CurrentImage = _image;
				Label = "Нажмите кнопку старт";
				
				SetAlgorithm(_currentAlg);
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