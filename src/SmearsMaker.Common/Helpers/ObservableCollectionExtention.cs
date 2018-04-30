using System;
using System.Collections.Generic;

namespace SmearsMaker.Common.Helpers
{
	public static class ObservableCollectionExtention
	{
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (var cur in enumerable)
			{
				action(cur);
			}
		}
	}
}