using System;

namespace SmearsMaker.Common
{
	public interface IProgress
	{
		event EventHandler<ProgressBarEventArgs> UpdateProgress;
		void NewProgress(string msg, int minimum, int maximum);
		void NewProgress(string msg);
		void Update(int step);
	}

	public class ProgressBarEventArgs : EventArgs
	{
		public string Msg { get; }
		public int Percentage { get; }
		public ProgressBarEventArgs(string msg)
		{
			Msg = msg;
		}

		public ProgressBarEventArgs(string msg, int percentage)
		{
			Msg = msg;
			Percentage = percentage;
		}
	}
}