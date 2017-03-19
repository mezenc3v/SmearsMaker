﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SmearTracer
{
    public class Pixel
    {
        public double[] Data { get; set; }
        public double[] Coordinates { get;}

        public Pixel()
        {

        }

        public Pixel(double[] data, int x, int y)
        {
            Data = data;
            Coordinates = new double[]{x,y};
        }
    }
}
