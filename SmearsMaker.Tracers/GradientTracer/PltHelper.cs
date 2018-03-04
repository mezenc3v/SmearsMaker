using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmearsMaker.Common.BaseTypes;

namespace SmearsMaker.Tracers.GradientTracer
{
	public static class PltHelper
	{
		internal static string GetPlt(List<BrushStroke> strokes, double pltHeight, double pltWidth, double widthStroke, int imageHeight, int imageWidth)
		{
			var delta = (float)pltHeight / imageHeight;
			var widthImage = imageWidth * delta;
			if (widthImage > pltWidth)
			{
				delta *= (float)pltWidth / widthImage;
			}
			var smearWidth = (int)(widthStroke * delta);
			const int index = 1;
			//start plt and add pen
			var plt = new StringBuilder().Append($"IN;SP{index};");
			//add pen pltWidth
			plt.Append($"PW{smearWidth},{index};");
			//group strokes by color
			var bsGroups = strokes.GroupBy(s => s.AverageData.GrayScale);

			foreach (var bsGroup in bsGroups.OrderBy(group => group.Key))
			{
				var average = bsGroup.First().AverageData.Data;
				//add pen color
				plt.Append($"PC{index},{(uint)average[2]},{(uint)average[1]},{(uint)average[0]};");
				foreach (var stroke in bsGroup.OrderBy(b => b.Objects.Count))
				{
					//add strokes
					plt.Append($"PU{(uint)(stroke.Head.X * delta)},{(uint)(pltHeight - stroke.Head.Y * delta)};");
					plt.Append("PD");

					for (int i = 1; i < stroke.Objects.Count - 1; i++)
					{
						var point = stroke.Objects[i].Centroid.Position;
						plt.Append($"{(uint)(point.X * delta)},{(uint)(pltHeight - point.Y * delta)},");
					}

					if (stroke.Objects.Count == 1)
					{
						plt.Append($"{(uint)(stroke.Tail.X * delta)},{(uint)(pltHeight - stroke.Tail.Y * delta)},");
						//Hack: paint shortest lines
						plt.Append($"{(uint)(stroke.Tail.X * delta) + 1},{(uint)(pltHeight - stroke.Tail.Y * delta) + 1};");
					}
					else
					{
						plt.Append($"{(uint)(stroke.Tail.X * delta)},{(uint)(pltHeight - stroke.Tail.Y * delta)};");
					}
				}
			}
			return plt.ToString();
		}
	}
}