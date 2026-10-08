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
            double xmin = double.Parse(args[2]);
            double xmax = double.Parse(args[3]);
            double ymin = double.Parse(args[4]);
            double ymax = double.Parse(args[5]);
            string output = args[6];

            Poly polynomial = CreatePolynomial(
                new Cplx { Re = 1 }, Cplx.Zero, Cplx.Zero, new Cplx { Re = 1 });

            Console.WriteLine(polynomial);
            Console.WriteLine(polynomial.Derive());

            FractalRenderer renderer = new FractalRenderer(polynomial);

            using (Bitmap image = renderer.Render(width, height, xmin, xmax, ymin, ymax))
            {
                image.Save(output ?? DefaultOutputPath);
            }
        }

        private static Poly CreatePolynomial(params Cplx[] coefficients)
        {
            Poly polynomial = new Poly();

            foreach (Cplx coefficient in coefficients)
            {
                polynomial.Add(coefficient);
            }

            return polynomial;
        }
    }
}