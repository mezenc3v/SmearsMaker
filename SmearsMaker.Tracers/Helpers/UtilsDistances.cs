using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.Tracers.Helpers
{
	public partial class Utils
	{
		private static double Distance(Point first, Point second)
		{
			double dictance = 0;

			var d = first.Position.X - second.Position.X;
			if (d < 0)
			{
				dictance -= d;
			}
			else
			{
				dictance += d;
			}

			d = first.Position.Y - second.Position.Y;

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
	}
}