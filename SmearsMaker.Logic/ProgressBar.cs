using System;

namespace SmearsMaker.Logic
{
	public class ProgressBar
	{
		public event EventHandler<ProgressBarEventArgs> NewStep;
		public event EventHandler<ProgressBarEventArgs> UpdateProgress;

		public string Message;
		public int Position;
		public int Minimum;
		public int Maximum;
		public int Step;

		public void NewProgress(string msg, int minimum, int maximum, int step)
		{
			Message = msg;
			Minimum = minimum;
			Maximum = maximum;
			Step = step;
			Position = (Step > 0) ? Minimum : Maximum;

			NewStep?.Invoke(this, new ProgressBarEventArgs(msg, CurrPercent));
		}

		public void Decrement(int val)
		{
			Position -= val;
		}

		public void Increment(int val)
		{
			Position += val;
		}

		public void Update()
		{
			Position += Step;

			UpdateProgress?.Invoke(this, new ProgressBarEventArgs(Message, CurrPercent));
		}

		private int CurrPercent => (Minimum + Position) * 100 / (Maximum - Minimum);
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