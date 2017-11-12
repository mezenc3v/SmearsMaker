using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmearsMaker.Wpf
{
	public class SettingsViewModel : INotifyPropertyChanged
	{
		public int FilterRank { get; set; }
		public int ClustersCount { get; set; }
		public int MaxSmearDistance { get; set; }
		public int MinSizeSuperpixel { get; set; }
		public int MaxSizeSuperpixel { get; set; }
		public float ClustersPrecision { get; set; }
		public int ClusterMaxIteration { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public SettingsViewModel()
		{

		}

		public void Update(int width, int height)
		{
			ClustersCount = (int)(Math.Sqrt(width + height) + Math.Sqrt(width + height) / 2) + 3;
			ClusterMaxIteration = 100;
			MinSizeSuperpixel = width * height / 1000;
			MaxSizeSuperpixel = width * height / 500;
			MaxSmearDistance = width * height;
			FilterRank = (int)Math.Sqrt((double)(width + height) / 80 / 2);
			ClustersPrecision = 0.001f;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string prop = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}
	}
}