using System.Collections.Generic;
using NNPTPZ1;

namespace NNPTPZ1.Mathematics
{
    public class Polynomial
    {
        /// <summary>
        /// Coefficients, ordered by ascending power of x.
        /// </summary>
        public List<ComplexNumber> Coefficients { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Polynomial() => Coefficients = new List<ComplexNumber>();

        public void Add(ComplexNumber coefficient) =>
            Coefficients.Add(coefficient);

        /// <summary>
        /// Reads a polynomial from text, so callers do not have to supply
        /// the coefficients one by one.
        /// </summary>
        /// <param name="text">for example "x^5 - 1"</param>
        /// <param name="polynomial">parsed polynomial</param>
        /// <param name="error">why parsing failed</param>
        /// <returns>true when the text is a valid polynomial</returns>
        public static bool TryParse(string text, out Polynomial polynomial, out string error)
        {
            return PolynomialParser.TryParse(text, out polynomial, out error);
        }

        /// <summary>
        /// Derives this polynomial and creates new one
        /// </summary>
        /// <returns>Derivated polynomial</returns>
        public Polynomial Derive()
        {
            Polynomial derivative = new Polynomial();
            for (int power = 1; power < Coefficients.Count; power++)
            {
                derivative.Add(Coefficients[power].Multiply(new ComplexNumber { Real = power }));
            }

            return derivative;
        }

        /// <summary>
        /// Evaluates polynomial at given point
        /// </summary>
        /// <param name="x">point of evaluation</param>
        /// <returns>y</returns>
        public ComplexNumber Eval(double x)
        {
            return Eval(new ComplexNumber { Real = x, Imaginary = 0 });
        }

        /// <summary>
        /// Evaluates polynomial at given point
        /// </summary>
        /// <param name="x">point of evaluation</param>
        /// <returns>y</returns>
        public ComplexNumber Eval(ComplexNumber x)
        {
            ComplexNumber sum = ComplexNumber.Zero;
            for (int power = 0; power < Coefficients.Count; power++)
            {
                sum = sum.Add(Term(power, x));
            }

            return sum;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>String repr of polynomial</returns>
        public override string ToString()
        {
            string text = "";
            for (int power = 0; power < Coefficients.Count; power++)
            {
                if (power > 0)
                {
                    text += " + ";
                }

                text += Coefficients[power] + PowerSymbol(power);
            }

            return text;
        }

        private static string PowerSymbol(int power)
        {
            if (power == 0)
            {
                return "";
            }

            if (power == 1)
            {
                return "x";
            }

            return "x^" + power;
        }

        private ComplexNumber Term(int power, ComplexNumber x)
        {
            if (power == 0)
            {
                return Coefficients[0];
            }

            ComplexNumber xPower = x;
            for (int i = 1; i < power; i++)
            {
                xPower = xPower.Multiply(x);
            }

            return Coefficients[power].Multiply(xPower);
        }
    }
}