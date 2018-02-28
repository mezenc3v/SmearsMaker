using System;

namespace SmearsMaker.Common
{
	public class Progress : IProgress
	{
		public event EventHandler<ProgressBarEventArgs> UpdateProgress;

		private string _message;
		private int _position;
		private int _minimum;
		private int _maximum;

		private int CurrPercent => _minimum - _maximum != 0 ? (_minimum + _position) * 100 / (_maximum - _minimum) : 0;

		public void NewProgress(string msg, int minimum, int maximum)
		{
			_message = msg;
			_minimum = minimum;
			_maximum = maximum;
			_position = 0;

			UpdateProgress?.Invoke(this, new ProgressBarEventArgs(msg, CurrPercent));
		}

		public void NewProgress(string msg)
		{
			_message = msg;

			UpdateProgress?.Invoke(this, new ProgressBarEventArgs(msg, CurrPercent));
		}

		public void Update(int step)
		{
			_position += step;

			UpdateProgress?.Invoke(this, new ProgressBarEventArgs(_message, CurrPercent));
		}
	}

	
}