using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmearTracer
{
    public class ListSegments
    {
        public List<Segment> Segments { get; set; }

        public ListSegments()
        {
            Segments = new List<Segment>();
        }

        public ListSegments(List<ListSegments> networks)
        {
            Segments = new List<Segment>();
            foreach (var segmentNetwork in networks)
            {
                Segments.AddRange(segmentNetwork.Segments);
            }
            Update();
        }

        public void Compute(List<Pixel> inputData)
        {
            List<Pixel> data = inputData;

            do
            {
                Segment segment = new Segment();
                int countPrevious, countNext;
                segment.Data.Add(data.First());
                do
                {
                    countPrevious = data.Count;
                    foreach (Pixel pixel in data)
                    {
                        if (segment.SuitableTo(pixel))
                        {
                            segment.Data.Add(pixel);
                        }
                    }
                    data.RemoveAll(d => segment.Data.Contains(d));
                    countNext = data.Count;
                } while (countPrevious != countNext);
                Segments.Add(segment);
            } while (data.Count>0);
            Update();
        }

        private void Update()
        {
            foreach (var segment in Segments)
            {
                double x = 0;
                double y = 0;
                double[] averageData = new double[segment.Data.First().Data.Length];
                foreach (var data in segment.Data)
                {
                    x += data.X;
                    y += data.Y;
                    for (int i = 0; i < averageData.Length; i++)
                    {
                        averageData[i] += data.Data[i];
                    }
                }
                x /= segment.Data.Count;
                y /= segment.Data.Count;

                for (int i = 0; i < averageData.Length; i++)
                {
                    averageData[i] /= segment.Data.Count;
                }
                Pixel centroid = new Pixel(averageData, (int)x,(int)y);
                segment.CentroidPixel = centroid;
            }
        }
    }
}
