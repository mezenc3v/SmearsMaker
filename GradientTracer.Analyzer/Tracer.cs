using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using SmearsMaker.Common;
using SmearsMaker.Model;
using SmearsMaker.Filtering;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;
using GradientTracer.FeatureDetection;

namespace GradientTracer.Analyzer
{
	public class Tracer : ITracer
	{
		public List<ImageSetting> Settings => _model.Settings;

		private readonly ImageModel _model;
		private List<Point> _sobelPoints;
		private SmearsMaker.Model.ImageModel _data;
		private readonly Progress _progress;
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		public Tracer(BitmapImage image, Progress progress)
		{
			_progress = progress ?? throw new NullReferenceException(nameof(progress));
			_model = new ImageModel(image);
		}

		public Task Execute()
		{
			_data = SmearsMaker.Model.ImageModel.ConvertBitmapToImage(_model.Image, ColorModel.Rgb);
			var filter = new MedianFilter((int)_model.FilterRank.Value, _model.Width, _model.Height);
			var sobel = new Sobel(_model.Width, _model.Height);
			return Task.Run(() =>
			{
				Log.Trace("Начало обработки изображения");
				_progress.NewProgress("Фильтрация");
				var sw = Stopwatch.StartNew();
				filter.Filter(_data);
				Log.Trace($"Фильтрация заняла {sw.Elapsed.Seconds} с.");
				sw.Restart();
				_progress.NewProgress("Оператор Собеля");
				_sobelPoints = sobel.Compute(_data);
				Log.Trace($"Операция заняла {sw.Elapsed.Seconds} с.");

				Log.Trace("Обработка изображения завершена");
				_progress.NewProgress("Готово");
				//Log.Trace($"Сформировано {clusters.Count} кластеров, {segmentsCount} сегментов, {smearsCount} мазков");
			});
		}

		public List<ImageViewModel> Views => new List<ImageViewModel>
		{
			new ImageViewModel(_model.Image, "Оригинал"),
			new ImageViewModel(Antialiasing(), "Размытое изображение"),
			new ImageViewModel(SobelGradients(), "Поле градиентов"),
			new ImageViewModel(SobelCurves(), "Границы"),
		};

		private BitmapSource Antialiasing()
		{
			_progress.NewProgress("Вычисление размытия");
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, _data.Points.ToList(), SmearsMaker.Model.Consts.Filtered);
		}

		private BitmapSource SobelGradients()
		{
			_progress.NewProgress("Вычисление градиентов");
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, _sobelPoints, FeatureDetection.Consts.Gradient);
		}

		private BitmapSource SobelCurves()
		{
			_progress.NewProgress("Вычисление границ");
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, _sobelPoints, FeatureDetection.Consts.Curves);
		}

		public string GetPlt()
		{
			throw new NotImplementedException();
		}
	}
}
