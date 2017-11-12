using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SmearsMaker.ClusterAnalysis.Kmeans;
using SmearsMaker.Concatenation;
using SmearsMaker.Filtering;
using SmearsMaker.Model;
using SmearsMaker.Model.Helpers;
using SmearsMaker.Segmentation;
using SmearsMaker.Segmentation.SimpleSegmentsSplitter;
using SmearsMaker.Segmentation.SuperpixelSplitter;

namespace SmearsMaker.Logic
{
	public class SmearTracer
	{
		private readonly ImageModel _model;
		private readonly List<Smear> _smears;
		private readonly ProgressBar _progress;

		public SmearTracer(ImageModel model, ProgressBar progress)
		{
			_progress = progress ?? throw new NullReferenceException(nameof(progress));
			_model = model ?? throw new NullReferenceException(nameof(model));
			_smears = new List<Smear>();
		}

		public Task Execute()
		{
			var model = Model.ImageModel.ConvertBitmapToImage(_model.Image, ColorModel.Rgb);
			var filter = new MedianFilter(_model.FilterRank, _model.Width, _model.Height);

			//model.ChangeColorModel(ColorModel.GrayScale);
			var kmeans = new KmeansClassic(_model.ClustersCount, _model.ClustersPrecision, model, _model.ClusterMaxIteration);
			var splitter = new SimpleSegmentsSplitter();
			var supPixSplitter = new SuperpixelSplitter(_model.MinSizeSuperpixel, _model.MaxSizeSuperpixel, _model.ClustersPrecision);
			var bsm = new BsmPair(_model.MaxSmearDistance);

			return Task.Run(() =>
			{
				_progress.NewProgress("Фильтрация");
				filter.Filter(model);
				_progress.NewProgress("Кластеризация");
				var clusters = kmeans.Clustering();
				_progress.NewProgress("Обработка", 0, clusters.Sum(c => c.Data.Count));
				Parallel.ForEach(clusters, (cluster) =>
				{
					var segments = splitter.Split(cluster.Data);

					Parallel.ForEach(segments, segment =>
					{
						var superPixels = supPixSplitter.Splitting(segment);
						var smears = bsm.Execute(superPixels.ToList<IObject>());

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
					});
				});
				_progress.NewProgress("Готово");
			});
		}

		public BitmapSource SuperPixels()
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

		public BitmapSource Clusters()
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

		public BitmapSource Segments()
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

		public BitmapSource BrushStrokes()
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

		public string GetPlt()
		{
			var height = _model.HeightPlt;
			var width = _model.WidthPlt;

			var delta = ((float)height / _model.Height);
			var widthImage = _model.Width * delta;
			if (widthImage > width)
			{
				delta *= width / widthImage;
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
						plt.Append($"PU{(uint)(brushStroke.Key.Objects.First().Centroid.Position.X * delta)},{(uint)(height - brushStroke.Key.Objects.First().Centroid.Position.Y * delta)};");

						for (int i = 1; i < brushStroke.Key.Objects.Count - 1; i++)
						{
							plt.Append($"PD{(uint)(brushStroke.Key.Objects[i].Centroid.Position.X * delta)},{(uint)(height - brushStroke.Key.Objects[i].Centroid.Position.Y * delta)};");
						}

						plt.Append($"PU{(uint)(brushStroke.Key.Objects.Last().Centroid.Position.X * delta)},{(uint)(height - brushStroke.Key.Objects.Last().Centroid.Position.Y * delta)};");
					}
				}
			}

			return plt.ToString();
		}
	}
}
