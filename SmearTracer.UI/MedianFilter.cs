using System.Collections.Generic;
using System.Linq;
using SmearTracer.Core.Abstract;

namespace SmearTracer.Core
{
    public class MedianFilter:IFilter
    {
        public int Rank;
        private readonly int _width;
        private readonly int _height;

        public MedianFilter(int rank, int width, int height)
        {
            Rank = rank;
            _width = width;
            _height = height;
        }

        public List<IUnit> Filtering(List<IUnit> units)
        {
            for (int coordX = 0; coordX < _width; coordX++)
            {
                for (int coordY = 0; coordY < _height; coordY++)
                {
                    var mask = new List<double[]>();

                    for (int coordMaskX = coordX - Rank; coordMaskX <= coordX + Rank; coordMaskX++)
                    {
                        for (int coordMaskY = coordY - Rank; coordMaskY <= coordY + Rank; coordMaskY++)
                        {
                            var index = coordMaskX * _height + coordMaskY;
                            if (index < _height * _width && coordMaskX >= 0 && coordMaskY >= 0)
                            {
                                mask.Add(units[index].Data);
                            }
                        }
                    }
                    units[coordX * _height + coordY].Data = mask.OrderByDescending(v => v.Sum()).ToArray()[mask.Count / 2];
                }
            }
            return units;
        }
    }
}
