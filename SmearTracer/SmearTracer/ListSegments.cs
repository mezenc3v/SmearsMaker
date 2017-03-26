using System.Collections.Generic;
using System.Linq;

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
            var segmentData = inputData.OrderBy(d=>d.X).ToList();
            while (segmentData.Count > 0)
            {
                var data = segmentData;                       
                int countPrevious, countNext;
                var segment = new Segment();
                segment.Data.Add(data[0]);
                data.RemoveAt(0);
                do
                {
                    segmentData = new List<Pixel>();
                    countPrevious = data.Count;                   
                    foreach (Pixel pixel in data)
                    {
                        if (segment.CompareTo(pixel))
                        {
                            segment.Data.Add(pixel);                     
                        }
                        else
                        {
                            segmentData.Add(pixel);
                        }
                    }
                    data = segmentData;
                    countNext = segmentData.Count;
                } while (countPrevious != countNext);
                Segments.Add(segment);
            }
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
