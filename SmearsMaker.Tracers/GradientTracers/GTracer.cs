using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Image;
using SmearsMaker.ImageProcessing.FeatureDetection;
using SmearsMaker.ImageProcessing.Filtering;
using SmearsMaker.Tracers.Helpers;
using Point = SmearsMaker.Common.BaseTypes.Point;

namespace SmearsMaker.Tracers.GradientTracers
{
	public abstract class GTracer : TracerBase
	{
		public override List<ImageSetting> Settings => _settings.Settings;
		public override List<ImageView> Views => CreateViews();

		private List<Point> _sobelPoints;
		private List<Segment> _superPixels;
		private List<BrushStroke> _strokes;

		internal readonly GtImageSettings _settings;
		internal int SplitterLength;
		internal double BsmLength;
		internal ISplitter Splitter;
		internal IBsm Bsm;

		protected abstract void PreExecute();

		protected void ConfigureServices(ISplitter splitter, IBsm bsm)
		{
			Splitter = splitter;
			Bsm = bsm;
		}
		protected GTracer(BitmapSource image, IProgress progress) : base(image, progress)
		{
			_settings = new GtImageSettings(Model.Width, Model.Height);
		}

		public override Task Execute()
		{
			PreExecute();
			var filter = new MedianFilter((int)_settings.FilterRank.Value, Model.Width, Model.Height);
			var sobel = new Sobel(Model.Width, Model.Height);
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
				_superPixels = Splitter.Splitting(segment, SplitterLength);

				Utils.UpdateCenter(Layers.Original, _superPixels);
				Utils.UpdateCenter(Layers.Filtered, _superPixels);
				Utils.UpdateCenter(Layers.Gradient, _superPixels);

				sw.Restart();
				Progress.NewProgress("Создание мазков");
				_strokes = Bsm.Execute(_superPixels, BsmLength, (float)_settings.Tolerance.Value);
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

			Progress.NewProgress("Вычисление мазков");
			var segments = new List<Segment>();
			foreach (var stroke in _strokes)
			{
				var objects = stroke.Objects;
				foreach (var o in objects)
				{
					foreach (var point in o.Data)
					{
						point.Pixels[Layers.Filtered] = stroke.AverageData;
					}
				}
				segments.AddRange(objects);
			}
			var smears = ImageHelper.CreateImage(segments, Layers.Filtered, Model);

			Progress.NewProgress("Вычисление мазков (линии)");
			var smearsMap = ImageHelper.PaintStrokes(Model.Image, _strokes, (int)_settings.WidthSmearUI.Value);

			Progress.NewProgress("Вычисление размытия");
			var blurredImage = Model.ConvertToBitmapSource(Model.Points, Layers.Filtered);

			Progress.NewProgress("Вычисление градиентов");
			var sobelGradients = Model.ConvertToBitmapSource(_sobelPoints, Layers.Gradient);

			Progress.NewProgress("Вычисление границ");
			var sobelCurves = Model.ConvertToBitmapSource(_sobelPoints, Layers.Curves);

			Progress.NewProgress("Вычисление центров");
			var centres = Model.ConvertToBitmapSource(_superPixels.Select(s => s.Centroid).ToList(), Layers.Original);

			return new List<ImageView>
			{
				new ImageView(Model.Image, "Оригинал"),
				new ImageView(blurredImage, "Размытое изображение"),
				new ImageView(sobelCurves, "Границы"),
				new ImageView(sobelGradients, "Поле градиентов"),
				new ImageView(spixelsGrad, "Суперпиксели-градиенты"),
				new ImageView(centres, "Центры суперпикселей"),
				new ImageView(spixels, "Суперпиксели"),
				new ImageView(smearsMap, "Мазки (Линии)"),
				new ImageView(smears, "Мазки"),
			};
		}

		public override string GetPlt()
		{
			return PltHelper.GetPlt(_strokes, _settings.HeightPlt.Value, _settings.WidthPlt.Value, _settings.WidthSmear.Value, Model.Height, Model.Width);
		}
	}
}
