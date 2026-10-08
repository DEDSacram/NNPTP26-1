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

        private readonly Polynomial polynomial;
        private readonly Polynomial derivative;
        private readonly List<ComplexNumber> roots = new List<ComplexNumber>();
        private readonly int maxNewtonIterations;
        private readonly double rootMatchTolerance;
        private readonly int shadeStepPerIteration;
        private readonly double zeroOffset;

        public FractalRenderer(Polynomial polynomial,
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
            double minX, double maxX, double minY, double maxY)
        {
            double xStep = (maxX - minX) / width;
            double yStep = (maxY - minY) / height;
            Bitmap image = new Bitmap(width, height);

            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    ComplexNumber start = CreateComplexPoint(minX + column * xStep, minY + row * yStep);

                    int iterations;
                    ComplexNumber root = RefineToRoot(start, out iterations);

                    int rootIndex = RegisterRoot(root);

                    image.SetPixel(column, row, ShadeColour(Colors[rootIndex % Colors.Length], iterations));
                }
            }

            return image;
        }

        private ComplexNumber CreateComplexPoint(double x, double y)
        {
            ComplexNumber point = new ComplexNumber { Real = x, Imaginary = y };

            if (point.Real == 0)
            {
                point.Real = zeroOffset;
            }

            if (point.Imaginary == 0)
            {
                point.Imaginary = zeroOffset;
            }

            return point;
        }

        private ComplexNumber RefineToRoot(ComplexNumber start, out int iterations)
        {
            ComplexNumber point = start;
            iterations = 0;

            for (int step = 0; step < maxNewtonIterations; step++)
            {
                ComplexNumber difference = polynomial.Eval(point).Divide(derivative.Eval(point));
                point = point.Subtract(difference);
                iterations++;
            }

            return point;
        }

        private int RegisterRoot(ComplexNumber point)
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

        private static double SquaredDistance(ComplexNumber left, ComplexNumber right)
        {
            double realDifference = left.Real - right.Real;
            double imaginaryDifference = left.Imaginary - right.Imaginary;

            return realDifference * realDifference + imaginaryDifference * imaginaryDifference;
        }
    }
}