using System;
using System.Collections.Generic;
using System.Drawing;
using NNPTPZ1.Mathematics;

namespace NNPTPZ1
{
    class FractalRenderer
    {
        private const int MaxNewtonIterations = 30;
        private const double RootMatchTolerance = 0.01;
        private const int ShadeStepPerIteration = 2;
        private const double ZeroOffset = 0.0001;

        private static readonly Color[] Colors =
        {
            Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Orange,
            Color.Fuchsia, Color.Gold, Color.Cyan, Color.Magenta
        };

        private readonly Poly polynomial;
        private readonly Poly derivative;
        private readonly List<Cplx> roots = new List<Cplx>();
        private readonly int maxNewtonIterations;
        private readonly double rootMatchTolerance;
        private readonly int shadeStepPerIteration;
        private readonly double zeroOffset;

        public FractalRenderer(Poly polynomial,
            int maxNewtonIterations = MaxNewtonIterations,
            double rootMatchTolerance = RootMatchTolerance,
            int shadeStepPerIteration = ShadeStepPerIteration,
            double zeroOffset = ZeroOffset)
        {
            this.polynomial = polynomial;
            this.maxNewtonIterations = maxNewtonIterations;
            this.rootMatchTolerance = rootMatchTolerance;
            this.shadeStepPerIteration = shadeStepPerIteration;
            this.zeroOffset = zeroOffset;
            derivative = polynomial.Derive();
        }

        public Bitmap Render(int width, int height,
            double xmin, double xmax, double ymin, double ymax)
        {
            double xstep = (xmax - xmin) / width;
            double ystep = (ymax - ymin) / height;
            Bitmap image = new Bitmap(width, height);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Cplx start = CreateComplexPoint(xmin + j * xstep, ymin + i * ystep);

                    int iterations;
                    Cplx root = RefineToRoot(start, out iterations);

                    int rootIndex = RegisterRoot(root);

                    image.SetPixel(j, i, ShadeColour(Colors[rootIndex % Colors.Length], iterations));
                }
            }

            return image;
        }

        private Cplx CreateComplexPoint(double x, double y)
        {
            Cplx point = new Cplx { Re = x, Imaginari = (float)y };

            if (point.Re == 0)
            {
                point.Re = zeroOffset;
            }

            if (point.Imaginari == 0)
            {
                point.Imaginari = (float)zeroOffset;
            }

            return point;
        }

        private Cplx RefineToRoot(Cplx start, out int iterations)
        {
            Cplx point = start;
            iterations = 0;

            for (int step = 0; step < maxNewtonIterations; step++)
            {
                Cplx difference = polynomial.Eval(point).Divide(derivative.Eval(point));
                point = point.Subtract(difference);
                iterations++;
            }

            return point;
        }

        private int RegisterRoot(Cplx point)
        {
            int match = -1;

            for (int index = 0; index < roots.Count; index++)
            {
                if (SquaredDistance(point, roots[index]) <= rootMatchTolerance)
                {
                    match = index;
                }
            }

            if (match < 0)
            {
                roots.Add(point);
                return roots.Count - 1;
            }

            return match;
        }

        private Color ShadeColour(Color colour, int iterations)
        {
            int shade = iterations * shadeStepPerIteration;

            return Color.FromArgb(
                ClampChannel(colour.R - shade),
                ClampChannel(colour.G - shade),
                ClampChannel(colour.B - shade));
        }

        private static int ClampChannel(int channel)
        {
            return Math.Min(Math.Max(0, channel), 255);
        }

        private static double SquaredDistance(Cplx left, Cplx right)
        {
            double realDifference = left.Re - right.Re;
            double imaginaryDifference = left.Imaginari - right.Imaginari;

            return realDifference * realDifference + imaginaryDifference * imaginaryDifference;
        }
    }
}