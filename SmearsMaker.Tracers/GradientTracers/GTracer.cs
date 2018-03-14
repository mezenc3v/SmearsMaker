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
		public override List<ImageSetting> Settings => GtSettings.Settings;
		public override List<ImageView> Views => CreateViews();

		private List<Point> _sobelPoints;
		private List<Segment> _superPixels;
		private List<BrushStroke> _strokes;

		internal readonly GtImageSettings GtSettings;
		internal abstract int SplitterLength { get; }
		internal abstract double BsmLength { get; }
		internal ISplitter Splitter;
		internal IBsm Bsm;

		protected GTracer(BitmapSource image, IProgress progress, ISplitter splitter, IBsm bsm) : base(image, progress)
		{
			GtSettings = new GtImageSettings(Model.Width, Model.Height);
			Splitter = splitter;
			Bsm = bsm;
		}

		public override Task Execute()
		{
			var filter = new MedianFilter((int)GtSettings.FilterRank.Value, Model.Width, Model.Height);
			var sobel = new Sobel(Model.Width, Model.Height);
			var centroid = new Point((double)Model.Width / 2, (double)Model.Height / 2);	
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
				Utils.AddCenter(Layers.Original, new List<Segment> { segment });
				segment.Data = _sobelPoints;
				Progress.NewProgress("Создание суперпикселей");
				_superPixels = Splitter.Splitting(segment, SplitterLength);
				Utils.UpdateCenter(Layers.Gradient, _superPixels);
				sw.Restart();
				Progress.NewProgress("Создание мазков");
				_strokes = Bsm.Execute(_superPixels, BsmLength, (float)GtSettings.Tolerance.Value);
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
			var smears = ImageHelper.CreateImageFromStrokes(_strokes, Layers.Filtered, Model);

			Progress.NewProgress("Вычисление мазков (линии)");
			var smearsMap = ImageHelper.PaintStrokes(Model.Image, _strokes, (int)GtSettings.WidthSmearUI.Value);

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
			return PltHelper.GetPlt(_strokes, GtSettings.HeightPlt.Value, GtSettings.WidthPlt.Value, GtSettings.WidthSmear.Value, Model.Height, Model.Width);
		}
	}
}
