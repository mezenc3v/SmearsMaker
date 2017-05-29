using System.Collections.Generic;
using System.Windows;
using SmearTracer.Model.Abstract;

namespace SmearTracer.Model
{
    public class SuperPixel : GraphicUnit
    {
        public SuperPixel(Point point)
        {
            Center = new Pixel(point) {ArgbArray = Generate()};
            Data = new List<IUnit>();
            Color = Generate();
        }

        public SuperPixel(IGraphicUnit superPixel)
        {
            Center = superPixel.Center;
            Center.ArgbArray = Generate();
            Data = superPixel.Data;
            Color = Generate();
            MinX = superPixel.MinX;
            MaxX = superPixel.MaxX;
            MinY = superPixel.MinY;
            MaxY = superPixel.MaxY;
        }
    }
}
