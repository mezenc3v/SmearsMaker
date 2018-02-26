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
using SmearsMaker.Common;
using SmearsMaker.Common.BaseTypes;
using SmearsMaker.Common.Image;
using SmearsMaker.ImageProcessing.Clustering;
using SmearsMaker.ImageProcessing.Filtering;
using SmearTracer.Logic;
using Color = System.Drawing.Color;
using Point = SmearsMaker.Common.BaseTypes.Point;

namespace SmearTracer
{
	public class STracer : TracerBase
	{
		public override List<ImageSetting> Settings => _settings.Settings;
		public override List<ImageView> Views => CreateViews();

		private readonly STImageSettings _settings;
		private List<Smear> _smears;

		public STracer(BitmapSource image) : base(image)
		{
			_settings = new STImageSettings(Model.Width, Model.Height);
		}

		public override Task Execute()
		{
			var filter = new MedianFilter((int)_settings.FilterRank.Value, Model.Width, Model.Height);

			var kmeans = new KmeansClassic((int)_settings.ClustersCount.Value, _settings.ClustersPrecision.Value, Model.Points, (int)_settings.ClusterMaxIteration.Value);
			var splitter = new SimpleSegmentsSplitter();
			var supPixSplitter = new SuperpixelSplitter((int)_settings.MinSizeSuperpixel.Value, (int)_settings.MaxSizeSuperpixel.Value, _settings.ClustersPrecision.Value);
			var bsm = new BsmPair((int)_settings.MaxSmearDistance.Value);

			var segmentsCount = 0;
			var smearsCount = 0;

			return Task.Run(() =>
			{
				_smears = new List<Smear>();
				Log.Trace("Начало обработки изображения");
				Progress.NewProgress("Фильтрация");
				var sw = Stopwatch.StartNew();
				filter.Filter(Model.Points);
				Log.Trace($"Фильтрация заняла {sw.Elapsed.Seconds} с.");
				sw.Restart();
				Progress.NewProgress("Кластеризация");
				var clusters = kmeans.Clustering();
				Log.Trace($"Кластеризация заняла {sw.Elapsed.Seconds} с.");
				var defectedPixels = new List<Point>();
				Progress.NewProgress("Обработка", 0, clusters.Sum(c => c.Data.Count));
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

						Parallel.ForEach(superPixels, (supPix) =>
						{
							if (supPix.Data.Count < _settings.MinSizeSuperpixel.Value)
							{
								lock (defectedPixels)
								{
									defectedPixels.AddRange(supPix.Data);
								}
							}
						});

						if (superPixels.Count > 0)
						{
							var smears = bsm.Execute(superPixels.ToList<Segment>());
							smearsCount += smears.Count;
							foreach (var smear in smears)
							{
								var newSmear = new Smear(smear)
								{
									Cluster = cluster,
									Segment = segment
								};

								lock (_smears)
								{
									_smears.Add(newSmear);
								}
								Progress.Update(newSmear.BrushStroke.Objects.Sum(o => o.Data.Count));
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
				Progress.NewProgress("Распределение удаленных точек");
				STHelper.Concat(_smears, defectedPixels);

				Log.Trace("Обработка изображения завершена");
				Progress.NewProgress("Готово");
				Log.Trace($"Сформировано {clusters.Count} кластеров, {segmentsCount} сегментов, {smearsCount} мазков");
			});
			
		}

		private List<ImageView> CreateViews()
		{
			return new List<ImageView>
			{
				new ImageView(Model.Image, "Оригинал"),
				new ImageView(SuperPixels(), "Суперпиксели"),
				new ImageView(Clusters(), "Кластеры"),
				new ImageView(Segments(), "Сегменты"),
				new ImageView(BrushStrokes(), "Мазки"),
				new ImageView(RandomBrushStrokes(), "Мазки(рандом)"),
				new ImageView(BrushStrokesLines(), "Мазки(линии)")
			};
		}

		private BitmapSource SuperPixels()
		{
			Progress.NewProgress("Вычисление суперпикселей");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				foreach (var obj in smear.BrushStroke.Objects)
				{
					var color = STHelper.GetGandomData(3);
					obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(color.ToArray()));
					//obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(obj.Centroid.Pixels[Layers.Original].Data));
					data.AddRange(obj.Data);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource Clusters()
		{
			Progress.NewProgress("Вычисление кластеров");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				foreach (var obj in smear.BrushStroke.Objects)
				{
					obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(smear.Cluster.Centroid.Data));
					data.AddRange(obj.Data);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource Segments()
		{
			Progress.NewProgress("Вычисление сегментов");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				foreach (var obj in smear.BrushStroke.Objects)
				{
					obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(obj.Centroid.Pixels[Layers.Original].Data));
					data.AddRange(obj.Data);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource BrushStrokes()
		{
			Progress.NewProgress("Вычисление мазков");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				var objs = smear.BrushStroke.Objects;
				var center = objs.OrderBy(p => p.Centroid.Pixels[Layers.Original].Sum).ToList()[objs.Count / 2].Centroid.Pixels[Layers.Original].Data;
				foreach (var obj in objs)
				{
					obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(center));
					data.AddRange(obj.Data);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource RandomBrushStrokes()
		{
			Progress.NewProgress("Вычисление мазков(рандом)");
			var data = new List<Point>();
			foreach (var smear in _smears)
			{
				var objs = smear.BrushStroke.Objects;
				var color = STHelper.GetGandomData(3);
				color.Add(255);
				foreach (var obj in objs)
				{
					obj.Data.ForEach(d => d.Pixels[Layers.Filtered] = new Pixel(color.ToArray()));
					data.AddRange(obj.Data);
				}
			}
			return Model.ConvertToBitmapSource(data, Layers.Filtered);
		}

		private BitmapSource BrushStrokesLines()
		{
			Progress.NewProgress("Вычисление мазков (линии)");
			Bitmap bitmap;
			using (var outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(Model.Image));
				enc.Save(outStream);
				bitmap = new Bitmap(outStream);
			}

			var g = Graphics.FromImage(bitmap);
			//g.Clear(Color.White);

			foreach (var smear in _smears.OrderByDescending(s => s.BrushStroke.Objects.Count))
			{
				var center = smear.BrushStroke.Objects.OrderBy(p => p.Centroid.Pixels[Layers.Original].Sum).ToList()[smear.BrushStroke.Objects.Count / 2].Centroid;

				var pen = new Pen(Color.FromArgb((byte)center.Pixels[Layers.Original].Data[3], (byte)center.Pixels[Layers.Original].Data[2],
					(byte)center.Pixels[Layers.Original].Data[1], (byte)center.Pixels[Layers.Original].Data[0]));

				var brush = new SolidBrush(Color.FromArgb((byte)center.Pixels[Layers.Original].Data[3], (byte)center.Pixels[Layers.Original].Data[2],
					(byte)center.Pixels[Layers.Original].Data[1], (byte)center.Pixels[Layers.Original].Data[0]));

				var pointsF = smear.BrushStroke.Objects.Select(point => new PointF((int)point.Centroid.Position.X, (int)point.Centroid.Position.Y)).ToArray();
				pen.Width = ((int)_settings.MinSizeSuperpixel.Value) / 20 + 1;
				if (pointsF.Length > 1)
				{
					g.FillEllipse(brush, pointsF.First().X, pointsF.First().Y, pen.Width, pen.Width);
					g.FillEllipse(brush, pointsF.Last().X, pointsF.Last().Y, pen.Width, pen.Width);
					g.DrawLines(pen, pointsF);
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

		public override string GetPlt()
		{
			var height = _settings.HeightPlt;

			var width = _settings.WidthPlt;

			var delta = ((float)height.Value / Model.Height);
			var widthImage = Model.Width * delta;
			if (widthImage > width.Value)
			{
				delta *= (float)width.Value / widthImage;
			}
			var smearWidth = (int)((_settings.MinSizeSuperpixel.Value / 20 + 1) * delta);
			var index = 1;
			//building string
			var plt = new StringBuilder().Append("IN;");
			var clusterGroups = _smears.GroupBy(s => s.Cluster);

			foreach (var cluster in clusterGroups.OrderByDescending(c => c.Key.Centroid.Sum))
			{
				plt.Append($"PC{index},{(uint)cluster.Key.Centroid.Data[2]},{(uint)cluster.Key.Centroid.Data[1]},{(uint)cluster.Key.Centroid.Data[0]};");
				var segmentsGroups = cluster.GroupBy(c => c.Segment);

				foreach (var segment in segmentsGroups.OrderByDescending(s => s.Key.Centroid.Pixels[Layers.Original].Sum))
				{
					var brushstrokeGroup = segment.GroupBy(b => b.BrushStroke);

					foreach (var brushStroke in brushstrokeGroup.OrderByDescending(b => b.Key.Objects.Count))
					{
						plt.Append($"PW{smearWidth},{index};");
						plt.Append($"PU{(uint)(brushStroke.Key.Objects.First().Centroid.Position.X * delta)},{(uint)(height.Value - brushStroke.Key.Objects.First().Centroid.Position.Y * delta)};");

						for (int i = 1; i < brushStroke.Key.Objects.Count - 1; i++)
						{
							plt.Append($"PD{(uint)(brushStroke.Key.Objects[i].Centroid.Position.X * delta)},{(uint)(height.Value - brushStroke.Key.Objects[i].Centroid.Position.Y * delta)};");
						}

						plt.Append($"PU{(uint)(brushStroke.Key.Objects.Last().Centroid.Position.X * delta)},{(uint)(height.Value - brushStroke.Key.Objects.Last().Centroid.Position.Y * delta)};");
					}
				}
				index++;
			}

			return plt.ToString();
		}
	}
}
