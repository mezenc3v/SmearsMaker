using System.Linq;
using System.Diagnostics;
using SmearsMaker.Common;
using System.Threading.Tasks;
using SmearsMaker.Common.Image;
using System.Collections.Generic;
using SmearsMaker.Tracers.Helpers;
using System.Windows.Media.Imaging;
using SmearsMaker.ImageProcessing.Segmenting;
using Point = SmearsMaker.Common.BaseTypes.Point;
using SmearsMaker.ImageProcessing.SmearsFormation;

namespace SmearsMaker.Tracers.GradientTracers
{
	public abstract class GTracer : TracerBase
	{
		public override List<ImageSetting> Settings => GtSettings.Settings;
		public override List<ImageView> Views => CreateViews();

		private List<Point> _filteredPoints;
		private List<Point> _detectorPoints;
		private List<Segment> _superPixels;
		private List<BrushStroke> _strokes;

		internal readonly GtImageSettings GtSettings;
		internal IServicesFactory Factory;

		protected GTracer(BitmapSource image, IProgress progress) : base(image, progress)
		{
			GtSettings = new GtImageSettings(Model.Width, Model.Height);
		}

		public override Task Execute()
		{
			var detector = Factory.CreateDetector();
			var splitter = Factory.CreateSplitter();
			var filter = Factory.CreateFilter();
			var bsm = Factory.CreateBsm();

			var segment = new Segment();

			return Task.Run(() =>
			{
				Log.Trace("Начало обработки изображения");
				Progress.NewProgress("Фильтрация");
				var sw = Stopwatch.StartNew();
				_filteredPoints = filter.Filtering(Model.Points);
				Log.Trace($"Фильтрация заняла {sw.Elapsed.Seconds} с.");
				sw.Restart();
				Progress.NewProgress("Вычисление градиентов");
				Log.Trace("Вычисление градиентов");
				_detectorPoints = detector.Compute(_filteredPoints);
				Log.Trace($"Операция заняла {sw.Elapsed.Seconds} с.");
				sw.Restart();
				segment.Points.AddRange(_detectorPoints);
				_superPixels = splitter.Splitting(segment);
				Log.Trace($"Операция заняла {sw.Elapsed.Seconds} с.");
				sw.Restart();
				Log.Trace("Создание мазков");
				_strokes = bsm.Execute(_superPixels);
				Log.Trace($"Операция заняла {sw.Elapsed.Seconds} с.");
				Log.Trace($"Сформировано {_superPixels.Count} суперпикселей, {_strokes.Count} мазков");
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
			var smears = ImageHelper.CreateImageFromStrokes(_strokes, Layers.Filtered, Model);

			Progress.NewProgress("Вычисление мазков (линии)");
			var smearsMap = ImageHelper.PaintStrokes(Model.Image, _strokes, (int)GtSettings.WidthSmearUI.Value);

			Progress.NewProgress("Вычисление размытия");
			var blurredImage = Model.ConvertToBitmapSource(_filteredPoints, Layers.Filtered);

			Progress.NewProgress("Вычисление градиентов");
			var sobelGradients = Model.ConvertToBitmapSource(_detectorPoints, Layers.Gradient);

			Progress.NewProgress("Вычисление границ");
			var sobelCurves = Model.ConvertToBitmapSource(_detectorPoints, Layers.Curves);

			Progress.NewProgress("Вычисление центров");
			var centres = Model.ConvertToBitmapSource(_superPixels.Select(s => s.GetCenterPoint()).ToList(), Layers.Original);

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
			return PltHelper.GetPlt(_strokes, GtSettings.HeightPlt.Value, GtSettings.WidthPlt.Value, GtSettings.WidthSmear.Value, Model.Height, Model.Width);
		}
	}
}
