using System;
using System.Windows.Media.Imaging;

namespace SmearsMaker.Logic
{
	public class ImageModel
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
		public int HeightPlt { get; set; }
		public int WidthPlt { get; set; }
		#endregion

		public ImageModel(BitmapSource image)
		{
			Image = image ?? throw new NullReferenceException("image");
		}
	}
}