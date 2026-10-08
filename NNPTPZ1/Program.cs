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
        private const int Success = 0;
        private const int BadArguments = 1;
        private const int RenderFailed = 2;

        static int Main(string[] args)
        {
            RenderRequest request;
            string problem;

            if (!RenderRequest.TryParse(args, out request, out problem))
            {
                ReportProblem(problem);
                return BadArguments;
            }

            try
            {
                Draw(request);
            }
            catch (Exception failure)
            {
                Console.Error.WriteLine("Cannot write the image to '" + request.OutputPath + "'.");
                Console.Error.WriteLine(failure.Message);
                return RenderFailed;
            }

            return Success;
        }

        private static void Draw(RenderRequest request)
        {
            Polynomial polynomial = CreatePolynomial(
                new ComplexNumber { Real = 1 },
                ComplexNumber.Zero,
                new ComplexNumber { Real = 1 });

            Console.WriteLine(polynomial);
            Console.WriteLine(polynomial.Derive());

            FractalRenderer renderer = new FractalRenderer(polynomial);

            using (Bitmap image = renderer.Render(request.Width, request.Height,
                request.MinX, request.MaxX, request.MinY, request.MaxY))
            {
                image.Save(request.OutputPath);
            }
        }

        private static void ReportProblem(string problem)
        {
            Console.Error.WriteLine("Error: " + problem);
            Console.Error.WriteLine();
            Console.Error.WriteLine(RenderRequest.Help);
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