using System;
using System.Drawing;
using NNPTPZ1.Mathematics;

namespace NNPTPZ1
{
    /// <summary>
    /// This program should produce Newton fractals.
    /// See more at: https://en.wikipedia.org/wiki/Newton_fractal
    /// </summary>
    class Program
    {
        private const string DefaultOutputPath = "../../../out.png";

        static void Main(string[] args)
        {
            int width = int.Parse(args[0]);
            int height = int.Parse(args[1]);
            double minX = double.Parse(args[2]);
            double maxX = double.Parse(args[3]);
            double minY = double.Parse(args[4]);
            double maxY = double.Parse(args[5]);
            string output = args[6];

            Polynomial polynomial = CreatePolynomial(
                new ComplexNumber { Real = 1 },
                ComplexNumber.Zero,
                ComplexNumber.Zero,
                new ComplexNumber { Real = 1 });

            Console.WriteLine(polynomial);
            Console.WriteLine(polynomial.Derive());

            FractalRenderer renderer = new FractalRenderer(polynomial);

            using (Bitmap image = renderer.Render(width, height, minX, maxX, minY, maxY))
            {
                image.Save(output ?? DefaultOutputPath);
            }
        }

        private static Polynomial CreatePolynomial(params ComplexNumber[] coefficients)
        {
            Polynomial polynomial = new Polynomial();

            foreach (ComplexNumber coefficient in coefficients)
            {
                polynomial.Add(coefficient);
            }

            return polynomial;
        }
    }
}