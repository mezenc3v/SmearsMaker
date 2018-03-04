using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Image;
using SmearsMaker.ImageProcessing.FeatureDetection;
using SmearsMaker.ImageProcessing.Filtering;
using SmearsMaker.Tracers.Helpers;
using SmearsMaker.Tracers.Logic;
using Point = SmearsMaker.Common.BaseTypes.Point;

namespace SmearsMaker.Tracers.GradientTracer
{
	public class GTracer : TracerBase
	{
		public override List<ImageSetting> Settings => _settings.Settings;
		public override List<ImageView> Views => CreateViews();

		private readonly GtImageSettings _settings;
		private List<Point> _sobelPoints;
		private List<Segment> _superPixels;
		private List<BrushStroke> _strokes;

		public GTracer(BitmapSource image, IProgress progress) : base(image, progress)
		{
			_settings = new GtImageSettings(Model.Width, Model.Height);
		}

		public override Task Execute()
		{
			var filter = new MedianFilter((int)_settings.FilterRank.Value, Model.Width, Model.Height);
			var sobel = new Sobel(Model.Width, Model.Height);
			var bsm = new GradientBsm(Math.Sqrt(_settings.SizeSuperPixel.Value) * 2 - 2, (float)_settings.Tolerance.Value);
			var centroid = new Point(Model.Width / 2, Model.Height / 2);
			centroid.Pixels.AddPixel(Layers.Original, null);
			var segment = new Segment(centroid, Model.Width, Model.Height);
			return Task.Run(() =>
			{
				Log.Trace("Начало обработки изображения");
				Progress.NewProgress("Фильтрация");
				var sw = Stopwatch.StartNew();
				filter.Filter(Model.Points);
				Log.Trace($"Фильтрация заняла {sw.Elapsed.Seconds} с.");
				sw.Restart();
				Progress.NewProgress("Вычисление градиентов");
				_sobelPoints = sobel.Compute(Model.Points);
				Log.Trace($"Операция заняла {sw.Elapsed.Seconds} с.");

				segment.Data = _sobelPoints;
				Progress.NewProgress("Создание суперпикселей");
				var splitter = new SuperpixelSplitter((int)_settings.SizeSuperPixel.Value);
				_superPixels = splitter.Splitting(segment);

				Utils.UpdateCenter(Layers.Original, _superPixels);
				Utils.UpdateCenter(Layers.Gradient, _superPixels);

				sw.Restart();
				Progress.NewProgress("Создание мазков");
				_strokes = bsm.Execute(_superPixels);
				Log.Trace($"Операция заняла {sw.Elapsed.Seconds} с.");
				Log.Trace($"Сформировано {_superPixels.Count} суперпикселей, {_strokes} мазков");
				Log.Trace("Обработка изображения завершена");
				Progress.NewProgress("Готово");
			});
			
		}

		private List<ImageView> CreateViews()
		{
			Progress.NewProgress("Вычисление суперпикселей");
			var spixels = ImageHelper.CreateRandomImage(_superPixels, Layers.SuperPixels, Model);

			Progress.NewProgress("Вычисление градиентов суперпикселей");
			var spixelsGrad = ImageHelper.CreateImage(_superPixels, Layers.Gradient, Model);

			Progress.NewProgress("Вычисление мазков (линии)");
			var smearsMap = ImageHelper.PaintStrokes(Model.Image, _strokes, (float)_settings.WidthSmearUI.Value);

			Progress.NewProgress("Вычисление размытия");
			var blurredImage = Model.ConvertToBitmapSource(Model.Points, Layers.Filtered);

			Progress.NewProgress("Вычисление градиентов");
			var sobelGradients = Model.ConvertToBitmapSource(_sobelPoints, Layers.Gradient);

			Progress.NewProgress("Вычисление границ");
			var sobelCurves = Model.ConvertToBitmapSource(_sobelPoints, Layers.Curves);

			return new List<ImageView>
			{
				new ImageView(Model.Image, "Оригинал"),
				new ImageView(blurredImage, "Размытое изображение"),
				new ImageView(sobelCurves, "Границы"),
				new ImageView(sobelGradients, "Поле градиентов"),
				new ImageView(spixelsGrad, "Суперпиксели-градиенты"),
				new ImageView(spixels, "Суперпиксели"),
				new ImageView(smearsMap, "Мазки"),
			};
		}

		public override string GetPlt()
		{
			return PltHelper.GetPlt(_strokes, _settings.HeightPlt.Value, _settings.WidthPlt.Value, _settings.WidthSmear.Value, Model.Height, Model.Width);
		}
	}
}
