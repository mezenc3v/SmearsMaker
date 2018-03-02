using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GradientTracer.Logic;
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Image;
using SmearsMaker.ImageProcessing.FeatureDetection;
using SmearsMaker.ImageProcessing.Filtering;
using Point = SmearsMaker.Common.BaseTypes.Point;

namespace GradientTracer
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
				var splitter = new SuperpixelSplitter((int)_settings.SizeSuperPixel.Value, (int)_settings.SizeSuperPixel.Value, 1);
				_superPixels = splitter.Splitting(segment);

				GtHelper.UpdateCenter(Layers.Original, _superPixels);
				GtHelper.UpdateCenter(Layers.Gradient, _superPixels);

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
			var spixels = GtHelper.CreateRandomImage(_superPixels, Layers.SuperPixels, Model);

			Progress.NewProgress("Вычисление градиентов суперпикселей");
			var spixelsGrad = GtHelper.CreateImage(_superPixels, Layers.Gradient, Model);

			Progress.NewProgress("Вычисление мазков (линии)");
			var smearsMap = GtHelper.PaintImage(Model.Image, _strokes, (float)_settings.WidthSmearUI.Value);

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
				new ImageView(sobelGradients, "Поле градиентов"),
				new ImageView(sobelCurves, "Границы"),
				new ImageView(spixels, "Суперпиксели"),
				new ImageView(spixelsGrad, "Суперпиксели-градиенты"),
				new ImageView(smearsMap, "Мазки"),
			};
		}

		public override string GetPlt()
		{
			var height = _settings.HeightPlt;
			var width = _settings.WidthPlt;

			var delta = (float)height.Value / Model.Height;
			var widthImage = Model.Width * delta;
			if (widthImage > width.Value)
			{
				delta *= (float)width.Value / widthImage;
			}
			var smearWidth = (int)(_settings.WidthSmear.Value * delta);
			const int index = 1;
			//start plt and add pen
			var plt = new StringBuilder().Append($"IN;SP{index};");
			//add pen width
			plt.Append($"PW{smearWidth},{index};");
			//group strokes by color
			var bsGroups = _strokes.GroupBy(s => s.AverageData.GrayScale);

			foreach (var bsGroup in bsGroups.OrderBy(group => group.Key))
			{
				var average = bsGroup.First().AverageData.Data;
				//add pen color
				plt.Append($"PC{index},{(uint)average[2]},{(uint)average[1]},{(uint)average[0]};");
				foreach (var stroke in bsGroup.OrderBy(b => b.Objects.Count))
				{
					//add strokes
					plt.Append($"PU{(uint)(stroke.Head.X * delta)},{(uint)(height.Value - stroke.Head.Y * delta)};");
					plt.Append("PD");

					for (int i = 1; i < stroke.Objects.Count - 1; i++)
					{
						var point = stroke.Objects[i].Centroid.Position;
						plt.Append($"{(uint)(point.X * delta)},{(uint)(height.Value - point.Y * delta)},");
					}

					if (stroke.Objects.Count == 1)
					{
						plt.Append($"{(uint)(stroke.Tail.X * delta)},{(uint)(height.Value - stroke.Tail.Y * delta)},");
						plt.Append($"{(uint)(stroke.Tail.X * delta) + 1},{(uint)(height.Value - stroke.Tail.Y * delta) + 1};");
					}
					else
					{
						plt.Append($"{(uint)(stroke.Tail.X * delta)},{(uint)(height.Value - stroke.Tail.Y * delta)};");
					}
				}
			}
			return plt.ToString();
		}
	}
}
