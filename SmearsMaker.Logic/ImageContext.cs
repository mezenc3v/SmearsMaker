using System;
using System.Windows.Media.Imaging;

namespace SmearsMaker.Logic
{
	public class ImageContext
	{
		#region properties
		public BitmapSource Image { get; }
		public int FilterRank { get; set; }
		public int Width => Image.PixelWidth;
		public int Height => Image.PixelHeight;
		public int ClustersCount { get; set; }
		public int MaxSmearDistance { get; set; }
		public int MinSizeSuperpixel { get; set; }
		public int MaxSizeSuperpixel { get; set; }
		public float ClustersPrecision { get; set; }
		public int ClusterMaxIteration { get; set; }
		#endregion

		public ImageContext(BitmapSource image)
		{
			Image = image ?? throw new NullReferenceException("image");
			InitProperties();
		}

		private void InitProperties()
		{
			ClustersCount = (int)(Math.Sqrt(Width + Height) + Math.Sqrt(Width + Height) / 2) + 3;
			ClusterMaxIteration = 100;
			MinSizeSuperpixel = Width * Height / 1000;
			MaxSizeSuperpixel = Width * Height / 500;
			MaxSmearDistance = Width * Height;
			FilterRank = (int)Math.Sqrt((double)(Width + Height) / 80 / 2);
			ClustersPrecision = 0.001f;
		}
	}
}