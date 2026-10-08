using System;
using System.Collections.Generic;
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
        private const int MaxNewtonIterations = 30;
        private const double RootMatchTolerance = 0.01;
        private const int ShadeStepPerIteration = 2;
        private const double ZeroOffset = 0.0001;
        private const string DefaultOutputPath = "../../../out.png";

        private static readonly Color[] Colors =
        {
            Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Orange,
            Color.Fuchsia, Color.Gold, Color.Cyan, Color.Magenta
        };

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
            Poly derivative = polynomial.Derive();

            Console.WriteLine(polynomial);
            Console.WriteLine(derivative);

            using (Bitmap image = RenderFractal(width, height, xmin, xmax, ymin, ymax, polynomial, derivative))
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

        private static Bitmap RenderFractal(int width, int height,
            double xmin, double xmax, double ymin, double ymax,
            Poly polynomial, Poly derivative)
        {
            double xstep = (xmax - xmin) / width;
            double ystep = (ymax - ymin) / height;
            List<Cplx> roots = new List<Cplx>();
            Bitmap image = new Bitmap(width, height);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Cplx start = CreateComplexPoint(xmin + j * xstep, ymin + i * ystep);

                    int iterations;
                    Cplx root = RefineToRoot(start, polynomial, derivative, out iterations);

                    int rootIndex = RegisterRoot(roots, root);

                    image.SetPixel(j, i, ShadeColour(Colors[rootIndex % Colors.Length], iterations));
                }
            }

            return image;
        }

        private static Cplx CreateComplexPoint(double x, double y)
        {
            Cplx point = new Cplx { Re = x, Imaginari = (float)y };

            if (point.Re == 0)
            {
                point.Re = ZeroOffset;
            }

            if (point.Imaginari == 0)
            {
                point.Imaginari = (float)ZeroOffset;
            }

            return point;
        }

        private static Cplx RefineToRoot(Cplx start, Poly polynomial, Poly derivative, out int iterations)
        {
            Cplx point = start;
            iterations = 0;

            for (int step = 0; step < MaxNewtonIterations; step++)
            {
                Cplx difference = polynomial.Eval(point).Divide(derivative.Eval(point));
                point = point.Subtract(difference);
                iterations++;
            }

            return point;
        }

        private static int RegisterRoot(List<Cplx> roots, Cplx point)
        {
            int match = -1;

            for (int index = 0; index < roots.Count; index++)
            {
                if (SquaredDistance(point, roots[index]) <= RootMatchTolerance)
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

        private static Color ShadeColour(Color colour, int iterations)
        {
            int shade = iterations * ShadeStepPerIteration;

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

    namespace Mathematics
    {
        public class Poly
        {
            /// <summary>
            /// Coe
            /// </summary>
            public List<Cplx> Coe { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public Poly() => Coe = new List<Cplx>();

            public void Add(Cplx coe) =>
                Coe.Add(coe);

            /// <summary>
            /// Derives this polynomial and creates new one
            /// </summary>
            /// <returns>Derivated polynomial</returns>
            public Poly Derive()
            {
                Poly derivative = new Poly();
                for (int power = 1; power < Coe.Count; power++)
                {
                    derivative.Add(Coe[power].Multiply(new Cplx { Re = power }));
                }

                return derivative;
            }

            /// <summary>
            /// Evaluates polynomial at given point
            /// </summary>
            /// <param name="x">point of evaluation</param>
            /// <returns>y</returns>
            public Cplx Eval(double x)
            {
                return Eval(new Cplx { Re = x, Imaginari = 0 });
            }

            /// <summary>
            /// Evaluates polynomial at given point
            /// </summary>
            /// <param name="x">point of evaluation</param>
            /// <returns>y</returns>
            public Cplx Eval(Cplx x)
            {
                Cplx sum = Cplx.Zero;
                for (int power = 0; power < Coe.Count; power++)
                {
                    sum = sum.Add(Term(power, x));
                }

                return sum;
            }

            private Cplx Term(int power, Cplx x)
            {
                if (power == 0)
                {
                    return Coe[0];
                }

                Cplx xPower = x;
                for (int i = 1; i < power; i++)
                {
                    xPower = xPower.Multiply(x);
                }

                return Coe[power].Multiply(xPower);
            }

            /// <summary>
            /// ToString
            /// </summary>
            /// <returns>String repr of polynomial</returns>
            public override string ToString()
            {
                string text = "";
                for (int power = 0; power < Coe.Count; power++)
                {
                    if (power > 0)
                    {
                        text += " + ";
                    }

                    text += Coe[power] + new string('x', power);
                }

                return text;
            }
        }

        public class Cplx
        {
            public double Re { get; set; }
            public float Imaginari { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is Cplx)
                {
                    Cplx x = obj as Cplx;
                    return x.Re == Re && x.Imaginari == Imaginari;
                }
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return Re.GetHashCode() ^ Imaginari.GetHashCode();
                }
            }

            public readonly static Cplx Zero = new Cplx()
            {
                Re = 0,
                Imaginari = 0
            };

            public Cplx Multiply(Cplx b)
            {
                Cplx a = this;
                // aRe*bRe + aRe*bIm*i + aIm*bRe*i + aIm*bIm*i*i
                return new Cplx()
                {
                    Re = a.Re * b.Re - a.Imaginari * b.Imaginari,
                    Imaginari = (float)(a.Re * b.Imaginari + a.Imaginari * b.Re)
                };
            }

            public double GetAbsoluteValue()
            {
                return Math.Sqrt(Re * Re + Imaginari * Imaginari);
            }

            public Cplx Add(Cplx b)
            {
                Cplx a = this;
                return new Cplx()
                {
                    Re = a.Re + b.Re,
                    Imaginari = a.Imaginari + b.Imaginari
                };
            }

            public double GetAngleInDegrees()
            {
                return Math.Atan2(Imaginari, Re) * 180.0 / Math.PI;
            }

            public Cplx Subtract(Cplx b)
            {
                Cplx a = this;
                return new Cplx()
                {
                    Re = a.Re - b.Re,
                    Imaginari = a.Imaginari - b.Imaginari
                };
            }

            public override string ToString()
            {
                return $"({Re} + {Imaginari}i)";
            }

            internal Cplx Divide(Cplx b)
            {
                // (aRe + aIm*i) / (bRe + bIm*i)
                // ((aRe + aIm*i) * (bRe - bIm*i)) / ((bRe + bIm*i) * (bRe - bIm*i))
                //  bRe*bRe - bIm*bIm*i*i
                var tmp = this.Multiply(new Cplx() { Re = b.Re, Imaginari = -b.Imaginari });
                var tmp2 = b.Re * b.Re + b.Imaginari * b.Imaginari;

                return new Cplx()
                {
                    Re = tmp.Re / tmp2,
                    Imaginari = (float)(tmp.Imaginari / tmp2)
                };
            }
        }
    }
}