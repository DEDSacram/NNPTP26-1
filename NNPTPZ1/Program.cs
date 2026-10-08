using System;
using System.Drawing;
using NNPTPZ1.Cli;
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

            if (RenderRequest.IsHelpRequest(args))
            {
                Console.WriteLine(RenderRequest.Help);
                return Success;
            }

            if (!RenderRequest.TryParse(args, out request, out problem))
            {
                ReportProblem(problem);
                return BadArguments;
            }

            try
            {
                if (!Draw(request))
                {
                    return BadArguments;
                }
            }
            catch (Exception failure)
            {
                Console.Error.WriteLine("Cannot draw or write '" + request.OutputPath + "'.");
                Console.Error.WriteLine(failure.Message);
                return RenderFailed;
            }

            return Success;
        }

        private static bool Draw(RenderRequest request)
        {
            Polynomial polynomial;
            string problem;

            if (!Polynomial.TryParse(request.PolynomialText, out polynomial, out problem))
            {
                Console.Error.WriteLine("Cannot read the polynomial '" + request.PolynomialText + "'.");
                Console.Error.WriteLine(problem);
                Console.Error.WriteLine();
                Console.Error.WriteLine(RenderRequest.Help);
                return false;
            }

            Console.WriteLine(polynomial);
            Console.WriteLine(polynomial.Derive());

            FractalRenderer renderer = new FractalRenderer(polynomial,
                request.MaxNewtonIterations,
                request.RootMatchTolerance,
                request.ShadeStepPerIteration,
                request.ZeroOffset);

            using (Bitmap image = renderer.Render(request.Width, request.Height,
                request.MinX, request.MaxX, request.MinY, request.MaxY))
            {
                image.Save(request.OutputPath);
            }

            return true;
        }

        private static void ReportProblem(string problem)
        {
            Console.Error.WriteLine("Error: " + problem);
            Console.Error.WriteLine();
            Console.Error.WriteLine(RenderRequest.Help);
        }
    }
}