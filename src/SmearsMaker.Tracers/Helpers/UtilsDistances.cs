using System;

namespace SmearsMaker.Tracers.Helpers
{
	public partial class Utils
	{
		internal static double ManhattanDistance(System.Windows.Point first, System.Windows.Point second)
		{
			double dictance = 0;

			var d = first.X - second.X;
			if (d < 0)
			{
				dictance -= d;
			}
			else
			{
				dictance += d;
			}

			d = first.Y - second.Y;

			if (d < 0)
			{
				dictance -= d;
			}
			else
			{
				dictance += d;
			}

			return dictance;
		}

		internal static double SqrtDistance(System.Windows.Point first, System.Windows.Point second)
		{
			var sum = Math.Pow(first.X - second.X, 2);
			sum += Math.Pow(first.Y - second.Y, 2);
			return Math.Sqrt(sum);
		}
	}
}