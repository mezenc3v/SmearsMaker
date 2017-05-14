using System.Collections.Generic;
using System.Windows;

namespace SmearTracer.Model
{
    public class SuperPixel : GraphicUnit
    {
        public SuperPixel(Point point)
        {
            Center = new Pixel(point);
            Data = new List<Pixel>();
            Color = Generate();
        }

        public SuperPixel(GraphicUnit superPixel)
        {
            Center = superPixel.Center;
            Data = superPixel.Data;
            Color = Generate();
            MinX = superPixel.MinX;
            MaxX = superPixel.MaxX;
            MinY = superPixel.MinY;
            MaxY = superPixel.MaxY;
        }
    }
}
