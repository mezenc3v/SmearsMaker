using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using NLog;
using SmearsMaker.ClusterAnalysis.Kmeans;
using SmearsMaker.Common;
using SmearsMaker.Concatenation;
using SmearsMaker.Filtering;
using SmearsMaker.Model;
using SmearsMaker.Model.Helpers;
using SmearsMaker.Segmentation;
using SmearsMaker.Segmentation.SimpleSegmentsSplitter;
using SmearsMaker.Segmentation.SuperpixelSplitter;
using Color = System.Drawing.Color;
using Point = SmearsMaker.Model.Point;

namespace SmearsMaker.SmearTracer
{
	public class Analyzer : ITracer
	{
		public List<ImageSetting> Settings => _model.Settings;

		private readonly ImageModel _model;
		private readonly List<Smear> _smears;
		private readonly Progress _progress;
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		public Analyzer(BitmapImage image, Progress progress)
		{
			_progress = progress ?? throw new NullReferenceException(nameof(progress));
			_model = new ImageModel(image);
			_smears = new List<Smear>();
		}

		public Task Execute()
		{
			var model = Model.ImageModel.ConvertBitmapToImage(_model.Image, ColorModel.Rgb);
			var filter = new MedianFilter((int)_model.FilterRank.Value, _model.Width, _model.Height);

			//model.ChangeColorModel(ColorModel.GrayScale);
			var kmeans = new KmeansClassic((int)_model.ClustersCount.Value, _model.ClustersPrecision.Value, model, (int)_model.ClusterMaxIteration.Value);
			var splitter = new SimpleSegmentsSplitter();
			var supPixSplitter = new SuperpixelSplitter((int)_model.MinSizeSuperpixel.Value, (int)_model.MaxSizeSuperpixel.Value, _model.ClustersPrecision.Value);
			var bsm = new BsmPair((int)_model.MaxSmearDistance.Value);

			var segmentsCount = 0;
			var smearsCount = 0;

			return Task.Run(() =>
			{
				Log.Trace("Начало обработки изображения");
				_progress.NewProgress("Фильтрация");
				var sw = Stopwatch.StartNew();
				filter.Filter(model);
				Log.Trace($"Фильтрация заняла {sw.Elapsed.Seconds} с.");
				sw.Restart();
				_progress.NewProgress("Кластеризация");
				var clusters = kmeans.Clustering();
				Log.Trace($"Кластеризация заняла {sw.Elapsed.Seconds} с.");
				var defectedPixels = new List<Point>();
				_progress.NewProgress("Обработка", 0, clusters.Sum(c => c.Data.Count));
				Parallel.ForEach(clusters, (cluster) =>
				{
					var swClusters = Stopwatch.StartNew();
					var segments = splitter.Split(cluster.Data);
					
					segmentsCount += segments.Count;
					Log.Trace($"Сегментация кластера размером {cluster.Data.Count} пикселей заняла {swClusters.Elapsed.Seconds} с.");
					swClusters.Reset();
					Parallel.ForEach(segments, segment =>
					{
						var superPixels = supPixSplitter.Splitting(segment);

						foreach (var supPix in superPixels)
						{
							if (supPix.Data.Count < _model.MinSizeSuperpixel.Value)
							{
								lock (defectedPixels)
								{
									defectedPixels.AddRange(supPix.Data);
								}
							}
						}

						if (superPixels.Count > 0)
						{
							var smears = bsm.Execute(superPixels.ToList<IObject>());
							smearsCount += smears.Count;
							foreach (var smear in smears)
							{
								var newSmear = new Smear(smear)
								{
									Cluster = cluster,
									Segment = segment
								};

								_smears.Add(newSmear);
								_progress.Update(newSmear.BrushStroke.Objects.Sum(o => o.Data.Count));
							}
						}
					});
					Log.Trace($"Разбиение на мазки кластера размером {cluster.Data.Count} пикселей заняло {swClusters.Elapsed.Seconds} с.");
				});

				foreach (var point in defectedPixels)
				{
					if (point == null)
					{
						throw new NullReferenceException("point");
					}
				}
				_progress.NewProgress("Распределение удаленных точек");
				Helper.Concat(_smears, defectedPixels);

				Log.Trace("Обработка изображения завершена");
				_progress.NewProgress("Готово");
				Log.Trace($"Сформировано {clusters.Count} кластеров, {segmentsCount} сегментов, {smearsCount} мазков");
			});
		}

		public List<ImageViewModel> Views => new List<ImageViewModel>
			{
				new ImageViewModel(_model.Image, "Оригинал"),
				new ImageViewModel(SuperPixels(), "Суперпиксели"),
				new ImageViewModel(Clusters(), "Кластеры"),
				new ImageViewModel(Segments(), "Сегменты"),
				new ImageViewModel(BrushStrokes(), "Мазки"),
				new ImageViewModel(RandomBrushStrokes(), "Мазки(рандом)"),
				new ImageViewModel(BrushStrokesLines(), "Мазки(линии)")
			};

		private BitmapSource SuperPixels()
		{
			_progress.NewProgress("Вычисление суперпикселей");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				foreach (var obj in smear.BrushStroke.Objects)
				{
					obj.Data.ForEach(d => d.SetFilteredPixel(obj.Centroid.Original.Data));
					data.AddRange(obj.Data);
				}
			}
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, data);
		}

		private BitmapSource Clusters()
		{
			_progress.NewProgress("Вычисление кластеров");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				foreach (var obj in smear.BrushStroke.Objects)
				{
					obj.Data.ForEach(d => d.SetFilteredPixel(smear.Cluster.Centroid.Data));
					data.AddRange(obj.Data);
				}
			}
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, data);
		}

		private BitmapSource Segments()
		{
			_progress.NewProgress("Вычисление сегментов");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				foreach (var obj in smear.BrushStroke.Objects)
				{
					obj.Data.ForEach(d => d.SetFilteredPixel(smear.Segment.Centroid.Original.Data));
					data.AddRange(obj.Data);
				}
			}
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, data);
		}

		private BitmapSource BrushStrokes()
		{
			_progress.NewProgress("Вычисление мазков");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				var objs = smear.BrushStroke.Objects;
				var center = objs.OrderBy(p => p.Centroid.Original.Sum).ToList()[objs.Count / 2].Centroid.Original.Data;
				foreach (var obj in objs)
				{
					obj.Data.ForEach(d => d.SetFilteredPixel(center));
					data.AddRange(obj.Data);
				}
			}
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, data);
		}

		private BitmapSource RandomBrushStrokes()
		{
			_progress.NewProgress("Вычисление мазков(рандом)");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				var objs = smear.BrushStroke.Objects;
				var color = Helper.GetGandomData(3);
				color.Add(255);
				foreach (var obj in objs)
				{
					obj.Data.ForEach(d => d.SetFilteredPixel(color.ToArray()));
					data.AddRange(obj.Data);
				}
			}
			return ImageHelper.ConvertRgbToRgbBitmap(_model.Image, data);
		}

		private BitmapSource BrushStrokesLines()
		{
			_progress.NewProgress("Вычисление мазков (линии)");
			System.Drawing.Bitmap bitmap;
			using (var outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(_model.Image));
				enc.Save(outStream);
				bitmap = new Bitmap(outStream);
			}

			var g = Graphics.FromImage(bitmap);
			g.Clear(Color.White);

			foreach (var smear in _smears.OrderByDescending(s=>s.BrushStroke.Objects.Count))
			{
				var center = smear.BrushStroke.Objects.OrderBy(p => p.Centroid.Original.Sum).ToList()[smear.BrushStroke.Objects.Count / 2].Centroid;

				var pen = new Pen(Color.FromArgb((byte)center.Original.Data[3], (byte)center.Original.Data[2],
					(byte)center.Original.Data[1], (byte)center.Original.Data[0]));

				var brush = new SolidBrush(Color.FromArgb((byte)center.Original.Data[3], (byte)center.Original.Data[2],
					(byte)center.Original.Data[1], (byte)center.Original.Data[0]));

				var pointsF = smear.BrushStroke.Objects.Select(point => new PointF((int)point.Centroid.Position.X, (int)point.Centroid.Position.Y)).ToArray();
				pen.Width = ((int)_model.MinSizeSuperpixel.Value) / 20 + 1;
				if (pointsF.Length > 1)
				{
					g.FillEllipse(brush, pointsF.First().X, pointsF.First().Y, pen.Width, pen.Width);
					g.FillEllipse(brush, pointsF.Last().X, pointsF.Last().Y, pen.Width, pen.Width);
					g.DrawLines(pen, pointsF);
					//g.DrawCurve(pen, pointsF);
					/*for (int i = 0; i < pointsF.Length; i++)
					{
						if (i + 1 < pointsF.Length)
						{
							g.DrawLine(pen, pointsF[i], pointsF[i + 1]);
						}
					}*/
				}
				else
				{
					g.FillEllipse(brush, pointsF.First().X, pointsF.First().Y, pen.Width, pen.Width);
				}
			}

			g.Dispose();

			var bmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				bitmap.GetHbitmap(),
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());

			return bmp;
		}

		public string GetPlt()
		{
			var height = _model.HeightPlt;
			var width = _model.WidthPlt;

			var delta = ((float)height.Value / _model.Height);
			var widthImage = _model.Width * delta;
			if (widthImage > width.Value)
			{
				delta *= (float)width.Value / widthImage;
			}

			//building string
			var plt = new StringBuilder().Append("IN;");

			var clusterGroups = _smears.GroupBy(s => s.Cluster);

			foreach (var cluster in clusterGroups.OrderByDescending(c => c.Key.Centroid.Sum))
			{
				plt.Append($"PC1,{(uint)cluster.Key.Centroid.Data[2]},{(uint)cluster.Key.Centroid.Data[1]},{(uint)cluster.Key.Centroid.Data[0]};");
				var segmentsGroups = cluster.GroupBy(c => c.Segment);

				foreach (var segment in segmentsGroups.OrderByDescending(s => s.Key.Centroid.Original.Sum))
				{
					var brushstrokeGroup = segment.GroupBy(b => b.BrushStroke);

					foreach (var brushStroke in brushstrokeGroup.OrderByDescending(b => b.Key.Objects.Count))
					{
						plt.Append($"PU{(uint)(brushStroke.Key.Objects.First().Centroid.Position.X * delta)},{(uint)(height.Value - brushStroke.Key.Objects.First().Centroid.Position.Y * delta)};");

						for (int i = 1; i < brushStroke.Key.Objects.Count - 1; i++)
						{
							plt.Append($"PD{(uint)(brushStroke.Key.Objects[i].Centroid.Position.X * delta)},{(uint)(height.Value - brushStroke.Key.Objects[i].Centroid.Position.Y * delta)};");
						}

						plt.Append($"PU{(uint)(brushStroke.Key.Objects.Last().Centroid.Position.X * delta)},{(uint)(height.Value - brushStroke.Key.Objects.Last().Centroid.Position.Y * delta)};");
					}
				}
			}

			return plt.ToString();
		}
	}
}
