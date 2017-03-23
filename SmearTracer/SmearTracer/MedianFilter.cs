using System.Collections.Generic;
using System.Linq;

namespace SmearTracer
{
    public class MedianFilter
    {
        public int Rank { get; set; }
        private readonly int _width;
        private readonly int _height;

        public MedianFilter(int rank, int width, int height)
        {
            Rank = rank;
            _width = width;
            _height = height;
        }

        public List<Pixel> Compute(List<Pixel> data)
        {
            for (int coordX = 0; coordX < _width; coordX++)
            {
                for (int coordY = 0; coordY < _height; coordY++)
                {
                    List<double[]> mask = new List<double[]>();

                    for (int coordMaskX = coordX - Rank; coordMaskX <= coordX + Rank; coordMaskX++)
                    {
                        for (int coordMaskY = coordY - Rank; coordMaskY <= coordY + Rank; coordMaskY++)
                        {
                            int index = coordMaskX * _height + coordMaskY;
                            if (index < _height * _width && coordMaskX >= 0 && coordMaskY >= 0)
                            {
                                mask.Add(data[index].Data);
                            }
                        }
                    }
                    data[coordX * _height + coordY].Data = mask.OrderByDescending(v => v.Sum()).ToArray()[mask.Count / 2];
                }
            }
            return data;
        }
    }
}
