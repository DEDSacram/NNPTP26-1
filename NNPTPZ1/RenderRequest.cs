using System;
using System.Collections.Generic;
using System.Globalization;

namespace NNPTPZ1
{
    class RenderRequest
    {
        private const string DefaultPolynomial = "1 + x^2";
        private const int DefaultMaxNewtonIterations = 30;
        private const double DefaultRootMatchTolerance = 0.01;
        private const int DefaultShadeStepPerIteration = 2;
        private const double DefaultZeroOffset = 0.0001;

        private static readonly string[] KnownOptions =
        {
            "width", "height", "minX", "maxX", "minY", "maxY", "output",
            "polynomial", "iterations", "tolerance", "shadeStep", "zeroOffset"
        };

        private static readonly string Usage =
@"NNPTPZ1 --width <n> --height <n> --minX <n> --maxX <n> --minY <n> --maxY <n>
              --output <file> [options]

Draws a Newton fractal. Arguments may come in any order and are given as
--name value or --name=value.

Required:
  --width <n>      image width in pixels, greater than zero
  --height <n>     image height in pixels, greater than zero
  --minX <n>       left edge of the drawn area, below --maxX
  --maxX <n>       right edge of the drawn area
  --minY <n>       bottom edge of the drawn area, below --maxY
  --maxY <n>       top edge of the drawn area
  --output <file>  image file to write

Optional:
  --polynomial <expr>
                  terms summed or subtracted, x may carry an exponent.
                  Coefficients may be real or imaginary, 'i' marks the latter.
                  Default: " + DefaultPolynomial + @"
  --iterations <n> how many times Newton's method runs per pixel, above zero.
                  Default: " + DefaultMaxNewtonIterations + @"
  --tolerance <n>  two roots closer than this count as one root, above zero.
                  Default: " + DefaultRootMatchTolerance.ToString(CultureInfo.InvariantCulture) + @"
  --shadeStep <n>  how much each iteration darkens the colour, zero or more.
                  Default: " + DefaultShadeStepPerIteration + @"
  --zeroOffset <n> value both coordinates move to when exactly zero, above zero.
                  Default: " + DefaultZeroOffset.ToString(CultureInfo.InvariantCulture) + @"

Other:
  --help           print this text and stop

Examples:
  NNPTPZ1 --width 800 --height 600 --minX -2 --maxX 2 --minY -1.5 --maxY 1.5
          --output fractal.png

  NNPTPZ1 --width=800 --height=600 --minX=-2 --maxX=2 --minY=-2 --maxY=2
          --output=fractal.png --polynomial=""x^5 - 1"" --iterations 50";

        private RenderRequest(int width, int height,
            double minX, double maxX, double minY, double maxY, string outputPath,
            string polynomialText, int maxNewtonIterations, double rootMatchTolerance,
            int shadeStepPerIteration, double zeroOffset)
        {
            Width = width;
            Height = height;
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            OutputPath = outputPath;
            PolynomialText = polynomialText;
            MaxNewtonIterations = maxNewtonIterations;
            RootMatchTolerance = rootMatchTolerance;
            ShadeStepPerIteration = shadeStepPerIteration;
            ZeroOffset = zeroOffset;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public double MinX { get; private set; }
        public double MaxX { get; private set; }
        public double MinY { get; private set; }
        public double MaxY { get; private set; }
        public string OutputPath { get; private set; }
        public string PolynomialText { get; private set; }
        public int MaxNewtonIterations { get; private set; }
        public double RootMatchTolerance { get; private set; }
        public int ShadeStepPerIteration { get; private set; }
        public double ZeroOffset { get; private set; }

        public static string Help
        {
            get { return Usage; }
        }

        public static bool TryParse(string[] args, out RenderRequest request, out string error)
        {
            request = null;
            error = null;

            Dictionary<string, string> options;
            if (!TryReadOptions(args, out options, out error))
            {
                return false;
            }

            int width;
            if (!TryReadRequired(options, "width", ReadWholeNumber,
                out width, out error)) return false;

            int height;
            if (!TryReadRequired(options, "height", ReadWholeNumber,
                out height, out error)) return false;

            double minX;
            if (!TryReadRequired(options, "minX", ReadAnyNumber, out minX, out error)) return false;

            double maxX;
            if (!TryReadRequired(options, "maxX", ReadAnyNumber, out maxX, out error)) return false;

            double minY;
            if (!TryReadRequired(options, "minY", ReadAnyNumber, out minY, out error)) return false;

            double maxY;
            if (!TryReadRequired(options, "maxY", ReadAnyNumber, out maxY, out error)) return false;

            string outputPath;
            if (!TryReadRequired(options, "output", ReadText, out outputPath, out error)) return false;

            if (minX >= maxX)
            {
                error = "--minX must be smaller than --maxX, got " + minX + " and " + maxX + ".";
                return false;
            }

            if (minY >= maxY)
            {
                error = "--minY must be smaller than --maxY, got " + minY + " and " + maxY + ".";
                return false;
            }

            string polynomialText;
            if (!TryReadOptional(options, "polynomial", DefaultPolynomial, ReadText,
                out polynomialText, out error)) return false;

            int iterations;
            if (!TryReadOptionalWholeNumber(options, "iterations", DefaultMaxNewtonIterations,
                false, out iterations, out error)) return false;

            double tolerance;
            if (!TryReadOptional(options, "tolerance", DefaultRootMatchTolerance,
                ReadPositiveNumber, out tolerance, out error)) return false;

            int shadeStep;
            if (!TryReadOptionalWholeNumber(options, "shadeStep", DefaultShadeStepPerIteration,
                true, out shadeStep, out error)) return false;

            double zeroOffset;
            if (!TryReadOptional(options, "zeroOffset", DefaultZeroOffset,
                ReadPositiveNumber, out zeroOffset, out error)) return false;

            request = new RenderRequest(width, height, minX, maxX, minY, maxY, outputPath,
                polynomialText, iterations, tolerance, shadeStep, zeroOffset);
            return true;
        }

        public static bool IsHelpRequest(string[] args)
        {
            if (args == null)
            {
                return false;
            }

            foreach (string arg in args)
            {
                if (arg.Equals("--help", StringComparison.OrdinalIgnoreCase) || arg == "-h")
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryReadOptions(string[] args,
            out Dictionary<string, string> options, out string error)
        {
            options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            error = null;

            if (args == null || args.Length == 0)
            {
                error = "No arguments given.";
                return false;
            }

            for (int i = 0; i < args.Length; i++)
            {
                string argument = args[i];

                if (!argument.StartsWith("--", StringComparison.Ordinal))
                {
                    error = "Expected an option starting with '--' but got '" + argument + "'.";
                    return false;
                }

                string name;
                string value;
                int equals = argument.IndexOf('=');

                if (equals >= 0)
                {
                    name = argument.Substring(2, equals - 2);
                    value = argument.Substring(equals + 1);
                }
                else
                {
                    name = argument.Substring(2);

                    if (i + 1 >= args.Length)
                    {
                        error = "The option --" + name + " has no value.";
                        return false;
                    }

                    value = args[++i];
                }

                if (name.Length == 0)
                {
                    error = "Found '--' without an option name.";
                    return false;
                }

                if (options.ContainsKey(name))
                {
                    error = "The option --" + name + " was given more than once.";
                    return false;
                }

                if (!IsKnownOption(name))
                {
                    error = "Unknown option --" + name + ".";
                    return false;
                }

                options.Add(name, value);
            }

            return true;
        }

        private static bool IsKnownOption(string name)
        {
            foreach (string known in KnownOptions)
            {
                if (string.Equals(known, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryReadRequired(Dictionary<string, string> options, string name,
            ReadIntOption reader, out int value, out string error)
        {
            string text;
            if (!TryTake(options, name, out text, out error))
            {
                value = 0;
                return false;
            }

            return reader(text, name, false, out value, out error);
        }

        private static bool TryReadRequired(Dictionary<string, string> options, string name,
            ReadDoubleOption reader, out double value, out string error)
        {
            string text;
            if (!TryTake(options, name, out text, out error))
            {
                value = 0;
                return false;
            }

            return reader(text, name, out value, out error);
        }

        private static bool TryReadRequired(Dictionary<string, string> options, string name,
            ReadStringOption reader, out string value, out string error)
        {
            string text;
            if (!TryTake(options, name, out text, out error))
            {
                value = null;
                return false;
            }

            return reader(text, name, out value, out error);
        }

        private static bool TryReadOptional(Dictionary<string, string> options, string name,
            string fallback, ReadStringOption reader, out string value, out string error)
        {
            string text;
            if (!options.TryGetValue(name, out text))
            {
                value = fallback;
                error = null;
                return true;
            }

            return reader(text, name, out value, out error);
        }

        private static bool TryReadOptionalWholeNumber(Dictionary<string, string> options, string name,
            int fallback, bool zeroAllowed, out int value, out string error)
        {
            string text;
            if (!options.TryGetValue(name, out text))
            {
                value = fallback;
                error = null;
                return true;
            }

            return ReadWholeNumber(text, name, zeroAllowed, out value, out error);
        }

        private static bool TryReadOptional(Dictionary<string, string> options, string name,
            double fallback, ReadDoubleOption reader, out double value, out string error)
        {
            string text;
            if (!options.TryGetValue(name, out text))
            {
                value = fallback;
                error = null;
                return true;
            }

            return reader(text, name, out value, out error);
        }

        private static bool TryTake(Dictionary<string, string> options, string name,
            out string text, out string error)
        {
            if (!options.TryGetValue(name, out text))
            {
                error = "Missing required option --" + name + ".";
                return false;
            }

            error = null;
            return true;
        }

        private static bool ReadText(string text, string name, out string value, out string error)
        {
            if (string.IsNullOrEmpty(text.Trim()))
            {
                error = "The " + name + " must not be empty.";
                value = null;
                return false;
            }

            value = text;
            error = null;
            return true;
        }

        private static bool ReadWholeNumber(string text, string name, bool zeroAllowed,
            out int value, out string error)
        {
            int parsed;
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                error = "The --" + name + " must be a whole number, got '" + text + "'.";
                value = 0;
                return false;
            }

            if (parsed < 0 || (parsed == 0 && !zeroAllowed))
            {
                error = "The --" + name + " must be "
                    + (zeroAllowed ? "zero or greater" : "greater than zero")
                    + ", got " + parsed + ".";
                value = 0;
                return false;
            }

            value = parsed;
            error = null;
            return true;
        }

        private static bool ReadAnyNumber(string text, string name,
            out double value, out string error)
        {
            double parsed;
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
            {
                error = "The --" + name + " must be a number, got '" + text + "'.";
                value = 0;
                return false;
            }

            if (double.IsNaN(parsed) || double.IsInfinity(parsed))
            {
                error = "The --" + name + " must be a finite number, got '" + text + "'.";
                value = 0;
                return false;
            }

            value = parsed;
            error = null;
            return true;
        }

        private static bool ReadPositiveNumber(string text, string name,
            out double value, out string error)
        {
            if (!ReadAnyNumber(text, name, out value, out error))
            {
                return false;
            }

            if (value <= 0)
            {
                error = "The --" + name + " must be greater than zero, got " + value + ".";
                value = 0;
                return false;
            }

            return true;
        }

        private delegate bool ReadIntOption(string text, string name, bool zeroAllowed,
            out int value, out string error);

        private delegate bool ReadDoubleOption(string text, string name,
            out double value, out string error);

        private delegate bool ReadStringOption(string text, string name,
            out string value, out string error);
    }
}