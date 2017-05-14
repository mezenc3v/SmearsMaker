﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;

namespace SmearTracer.Model
{
    public class Circle
    {
        public Point Center;
        public double Radius;
        public double[] Color;

        public Circle(Point center, double radius)
        {
            Center = center;
            Radius = radius;
            Color = Generate();
        }

        public bool Contains(Pixel pixel)
        {
            if (Math.Pow(pixel.Position.X - Center.X, 2) + Math.Pow(pixel.Position.Y - Center.Y, 2) <= Radius)
            {
                return true;
            }
            return false;
        }

        public bool Contains(Point pixel)
        {
            if (Math.Pow(pixel.X - Center.X, 2) + Math.Pow(pixel.Y - Center.Y, 2) <= Radius)
            {
                return true;
            }
            return false;
        }

        public int Contains(List<Pixel> data)
        {
            int counter = 0;
            foreach (var pixel in data)
            {
                if (Contains(pixel))
                {
                    counter++;
                }
            }
            return counter;
        }

        private static double[] Generate()
        {
            RNGCryptoServiceProvider c = new RNGCryptoServiceProvider();
            byte[] randomNumber = new byte[3];
            c.GetBytes(randomNumber);

            byte r = randomNumber[0];
            byte g = randomNumber[1];
            byte b = randomNumber[2];

            double[] color = { r, g, b, 255 };

            return color;
        }
    }
}