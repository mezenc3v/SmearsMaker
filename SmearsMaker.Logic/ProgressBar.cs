using System;

namespace SmearsMaker.Logic
{
	public class ProgressBar
	{
		public event EventHandler<ProgressBarEventArgs> UpdateProgress;

		public string Message;
		public int Position;
		public int Minimum;
		public int Maximum;
		public int Step;

		public void NewProgress(string msg, int minimum, int maximum)
		{
			Message = msg;
			Minimum = minimum;
			Maximum = maximum;
			Position = 0;

			UpdateProgress?.Invoke(this, new ProgressBarEventArgs(msg, CurrPercent));
		}

		public void NewProgress(string msg)
		{
			Message = msg;

			UpdateProgress?.Invoke(this, new ProgressBarEventArgs(msg, CurrPercent));
		}

		public void Update(int step)
		{
			Position += step;

			UpdateProgress?.Invoke(this, new ProgressBarEventArgs(Message, CurrPercent));
		}

		private int CurrPercent => Minimum - Maximum != 0 ? (Minimum + Position) * 100 / (Maximum - Minimum) : 0;
	}

	public class ProgressBarEventArgs : EventArgs
	{
		public ProgressBarEventArgs(string msg, int percentage)
		{
			Msg = msg;
			Percentage = percentage;
		}

		public ProgressBarEventArgs(string msg)
		{
			Msg = msg;
		}

		public string Msg { get; set; }

		public int Percentage { get; set; }
	}
}