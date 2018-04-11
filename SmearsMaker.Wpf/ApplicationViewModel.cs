using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NLog;
using SmearsMaker.Common;
using SmearsMaker.Common.Image;
using SmearsMaker.HPGL;

namespace SmearsMaker.Wpf
{
	public class ApplicationViewModel : INotifyPropertyChanged
	{
		public List<Type> Tracers { get; }

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
		private readonly List<ImageView> _images;
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
		private int _currentImageIndex;
		private ITracer _tracer;
		private BitmapImage _image;
		private readonly PltReader _reader;

		public ApplicationViewModel()
		{
			OpenImage = new Command(OpenFile);
			SavePlt = new Command(SavePtl);
			SaveImages = new Command(SaveImagesInFolder);
			_images = new List<ImageView>();
			_currentImageIndex = 0;
			Label = "Выберите изображение";
			Tracers = Helper.LoadLibraries();
			_reader = new PltReader();
		}

		public void SetAlgorithm(Type tracer)
		{
			if (_image == null || tracer == null) return;
			_tracer?.Dispose();
			_tracer = Activator.CreateInstance(tracer, _image, new Progress()) as ITracer;
			
			_tracer.Progress.UpdateProgress += UpdateProgress;
			Settings = _tracer.Settings;
		}

		public async Task Run()
		{
			try
			{
				if (_image == null) return;
				if (_tracer == null && Tracers.Any())
				{
					SetAlgorithm(Tracers.First());
				}
				_images.Clear();
				await _tracer.Execute();
				_images.AddRange(_tracer.Views);
				if (_images.Count < _currentImageIndex)
				{
					_currentImageIndex = _images.Count - 1;
				}
				Label = _images[_currentImageIndex].Name;
				CurrentImage = _images[_currentImageIndex].Source;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка! {ex.Message}");
				Log.Error(ex);
			}
		}

		private void SavePtl()
		{
			if (_tracer == null) return;

			try
			{
				var plt = _tracer.GetPlt();
				Helper.SavePlt(plt);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка! {ex.Message}");
			}
		}
		public void ChangeImage(KeyEventArgs e)
		{
			if (_images.Count == 0) return;

			switch (e.Key)
			{
				case Key.Right:
					_currentImageIndex = _currentImageIndex < _images.Count - 1 ? _currentImageIndex + 1 : 0;
					break;
				case Key.Left:
					_currentImageIndex = _currentImageIndex > 0 ? _currentImageIndex - 1 : _images.Count - 1;
					break;
				default:
					return;
			}
			CurrentImage = _images[_currentImageIndex].Source;
			Label = _images[_currentImageIndex].Name;
		}

		public void OpenPlt()
		{
			var name = Helper.OpenPlt();
			if (name != null)
			{
				CurrentImage = _reader.Read(name);
			}
		}

		private void UpdateProgress(object sender, ProgressBarEventArgs args)
		{
			Label = args.Percentage != 0 ? $"{args.Msg} {args.Percentage}%" : args.Msg;
		}

		private void OpenFile()
		{
			var imageUri = Helper.OpenImage();
			if (imageUri == null) return;
			_image = new BitmapImage(imageUri);
			CurrentImage = _image;
			Label = "Нажмите кнопку старт";

			if (_tracer != null)
			{
				SetAlgorithm(_tracer.GetType());
			}
		}

		private void SaveImagesInFolder()
		{
			var path = Helper.GetFolder();

			foreach (var image in _images)
			{
				Helper.SaveBitmapSource(path, image.Name, image.Source);
			}
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string prop = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}
	}
}