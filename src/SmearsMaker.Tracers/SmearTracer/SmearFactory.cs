using SmearsMaker.Common;
using SmearsMaker.Common.Image;
using SmearsMaker.ImageProcessing;
using SmearsMaker.ImageProcessing.Clustering;
using SmearsMaker.ImageProcessing.FeatureDetection;
using SmearsMaker.ImageProcessing.Filtering;
using SmearsMaker.Tracers.Logic;

namespace SmearsMaker.Tracers.SmearTracer
{
	public class SmearFactory : IServicesFactory
	{
		private readonly ImageModel _model;
		private readonly IProgress _progress;
		private readonly StImageSettings _settings;

		public SmearFactory(StImageSettings settings, ImageModel model, IProgress progress)
		{
			_model = model;
			_progress = progress;
			_settings = settings;
		}

		public IStrokesBuilder CreateStrokesBuilder()
		{
			return new PairStrokesBuilder(_settings.MaxSmearDistance.Value);
		}

		public ISplitter CreateSplitter()
		{
			return new SuperpixelSplitter(_progress, (int)_settings.WidthSmear.Value);
		}

		public IClusterizer CreateClusterizer()
		{
			return new KmeansClassic((int)_settings.ClustersCount.Value, _settings.ClustersPrecision.Value, (int)_settings.ClusterMaxIteration.Value);
		}

		public IFilter CreateFilter()
		{
			return new MedianFilter((int)_settings.FilterRank.Value, _model.Width, _model.Height);
		}

		public IDetector CreateDetector()
		{
			return new Sobel(_model.Width, _model.Height);
		}
	}
}