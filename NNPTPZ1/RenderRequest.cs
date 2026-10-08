using System;
using System.Globalization;

namespace NNPTPZ1
{
    class RenderRequest
    {
        private const int RequiredArgumentCount = 7;

        private const string Usage =
@"NNPTPZ1 <width> <height> <minX> <maxX> <minY> <maxY> <output.png>

Draws a Newton fractal of the polynomial 1 + x^2.

Arguments:
  width, height   size of the image in pixels, both must be greater than zero
  minX, maxX      left and right edge of the drawn area, minX must be below maxX
  minY, maxY      bottom and top edge of the drawn area, minY must be below maxY
  output.png      file the image is written to

Example:
  NNPTPZ1 800 600 -2 2 -1.5 1.5 fractal.png";

        private RenderRequest(int width, int height,
            double minX, double maxX, double minY, double maxY, string outputPath)
        {
            Width = width;
            Height = height;
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            OutputPath = outputPath;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public double MinX { get; private set; }
        public double MaxX { get; private set; }
        public double MinY { get; private set; }
        public double MaxY { get; private set; }
        public string OutputPath { get; private set; }

        public static string Help
        {
            get { return Usage; }
        }

        public static bool TryParse(string[] args, out RenderRequest request, out string error)
        {
            request = null;
            error = null;

            if (args == null || args.Length == 0)
            {
                error = "No arguments given.";
                return false;
            }

            if (args.Length < RequiredArgumentCount)
            {
                error = "Expected " + RequiredArgumentCount + " arguments but got "
                    + args.Length + ".";
                return false;
            }

            int width;
            if (!TryReadSize(args, 0, "width", out width, out error)) return false;

            int height;
            if (!TryReadSize(args, 1, "height", out height, out error)) return false;

            double minX;
            if (!TryReadNumber(args, 2, "minX", out minX, out error)) return false;

            double maxX;
            if (!TryReadNumber(args, 3, "maxX", out maxX, out error)) return false;

            double minY;
            if (!TryReadNumber(args, 4, "minY", out minY, out error)) return false;

            double maxY;
            if (!TryReadNumber(args, 5, "maxY", out maxY, out error)) return false;

            if (minX >= maxX)
            {
                error = "minX must be smaller than maxX, got minX=" + minX + " and maxX=" + maxX + ".";
                return false;
            }

            if (minY >= maxY)
            {
                error = "minY must be smaller than maxY, got minY=" + minY + " and maxY=" + maxY + ".";
                return false;
            }

            string outputPath = args[6];
            if (string.IsNullOrEmpty(outputPath.Trim()))
            {
                error = "The output path must not be empty.";
                return false;
            }

            request = new RenderRequest(width, height, minX, maxX, minY, maxY, outputPath);
            return true;
        }

        private static bool TryReadSize(string[] args, int index, string name,
            out int value, out string error)
        {
            int parsed;
            if (!int.TryParse(args[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                error = "The " + name + " must be a whole number, got '" + args[index] + "'.";
                value = 0;
                return false;
            }

            if (parsed <= 0)
            {
                error = "The " + name + " must be greater than zero, got " + parsed + ".";
                value = 0;
                return false;
            }

            value = parsed;
            error = null;
            return true;
        }

        private static bool TryReadNumber(string[] args, int index, string name,
            out double value, out string error)
        {
            double parsed;
            if (!double.TryParse(args[index], NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
            {
                error = "The " + name + " must be a number, got '" + args[index] + "'.";
                value = 0;
                return false;
            }

            if (double.IsNaN(parsed) || double.IsInfinity(parsed))
            {
                error = "The " + name + " must be a finite number, got '" + args[index] + "'.";
                value = 0;
                return false;
            }

            value = parsed;
            error = null;
            return true;
        }
    }
}