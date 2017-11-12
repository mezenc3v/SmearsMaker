namespace SmearsMaker.Wpf
{
	public class Settings
	{
		public int FilterRank { get; set; }
		public int ClustersCount { get; set; }
		public int MaxSmearDistance { get; set; }
		public int MinSizeSuperpixel { get; set; }
		public int MaxSizeSuperpixel { get; set; }
		public float ClustersPrecision { get; set; }
		public int ClusterMaxIteration { get; set; }
	}
}