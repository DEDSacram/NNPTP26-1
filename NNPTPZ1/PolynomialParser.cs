using System.Collections.Generic;
using System.Globalization;
using NNPTPZ1.Mathematics;

namespace NNPTPZ1
{
    public class PolynomialParser
    {
        private const int MinimumDegree = 1;

        public static bool TryParse(string text, out Polynomial polynomial, out string error)
        {
            polynomial = null;
            error = null;

            if (string.IsNullOrEmpty(text) || text.Trim().Length == 0)
            {
                error = "The polynomial must not be empty.";
                return false;
            }

            List<Term> terms;
            if (!TryReadTerms(text, out terms, out error))
            {
                return false;
            }

            Dictionary<int, ComplexNumber> byPower = new Dictionary<int, ComplexNumber>();
            int highestPower = 0;

            foreach (Term term in terms)
            {
                byPower[term.Power] = term.Coefficient;
                if (term.Power > highestPower)
                {
                    highestPower = term.Power;
                }
            }

            if (highestPower < MinimumDegree)
            {
                error = "The polynomial must contain at least one term with x.";
                return false;
            }

            Polynomial parsed = new Polynomial();
            for (int power = 0; power <= highestPower; power++)
            {
                ComplexNumber coefficient;
                if (!byPower.TryGetValue(power, out coefficient))
                {
                    coefficient = ComplexNumber.Zero;
                }

                parsed.Add(coefficient);
            }

            polynomial = parsed;
            return true;
        }

        private static bool TryReadTerms(string text, out List<Term> terms, out string error)
        {
            terms = new List<Term>();
            error = null;

            string expression = text.Replace(" ", string.Empty);
            int position = 0;

            while (position < expression.Length)
            {
                ComplexNumber coefficient;
                int power;
                int nextPosition;

                if (!TryReadTerm(expression, position,
                    out coefficient, out power, out nextPosition, out error))
                {
                    return false;
                }

                terms.Add(new Term(coefficient, power));
                position = nextPosition;
            }

            return true;
        }

        private static bool TryReadTerm(string expression, int position,
            out ComplexNumber coefficient, out int power, out int nextPosition, out string error)
        {
            error = null;
            int start = position;

            double sign = 1;
            if (position < expression.Length && IsSign(expression[position]))
            {
                if (expression[position] == '-')
                {
                    sign = -1;
                }

                position++;
            }

            string numberText;
            bool hasCoefficient = TryReadNumber(expression, ref position, out numberText);

            bool imaginary = position < expression.Length
                && (expression[position] == 'i' || expression[position] == 'j');

            if (imaginary)
            {
                position++;
            }

            if (position < expression.Length && expression[position] == '*')
            {
                position++;
            }

            if (!hasCoefficient && !imaginary && !IsX(expression, position))
            {
                coefficient = null;
                power = 0;
                nextPosition = position;
                error = "'" + expression.Substring(start) + "' is not a valid term.";
                return false;
            }

            int termPower = 0;
            if (IsX(expression, position))
            {
                position++;
                termPower = 1;

                if (position < expression.Length && expression[position] == '^')
                {
                    position++;
                    string exponentText;
                    if (!TryReadNumber(expression, ref position, out exponentText))
                    {
                        coefficient = null;
                        power = 0;
                        nextPosition = position;
                        error = "Missing exponent after 'x^'.";
                        return false;
                    }

                    termPower = int.Parse(exponentText, CultureInfo.InvariantCulture);
                }
            }

            double magnitude = hasCoefficient ? ReadNumber(numberText) : 1;
            double value = sign * magnitude;

            coefficient = imaginary
                ? new ComplexNumber { Real = 0, Imaginary = value }
                : new ComplexNumber { Real = value, Imaginary = 0 };

            power = termPower;
            nextPosition = position;
            return true;
        }

        private static bool IsX(string expression, int position)
        {
            return position < expression.Length
                && (expression[position] == 'x' || expression[position] == 'X');
        }

        private static double ReadNumber(string text)
        {
            double value = 0;
            double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            return value;
        }

        private static bool TryReadNumber(string expression, ref int position, out string numberText)
        {
            int start = position;
            bool seenDot = false;

            while (position < expression.Length)
            {
                char character = expression[position];

                if (char.IsDigit(character))
                {
                    position++;
                    continue;
                }

                if (character == '.' && !seenDot)
                {
                    seenDot = true;
                    position++;
                    continue;
                }

                if ((character == 'e' || character == 'E') && position > start
                    && position + 1 < expression.Length
                    && (char.IsDigit(expression[position + 1]) || IsSign(expression[position + 1])))
                {
                    position += 2;
                    continue;
                }

                break;
            }

            numberText = expression.Substring(start, position - start);
            return numberText.Length > 0;
        }

        private static bool IsSign(char character)
        {
            return character == '+' || character == '-';
        }

        private class Term
        {
            public Term(ComplexNumber coefficient, int power)
            {
                Coefficient = coefficient;
                Power = power;
            }

            public ComplexNumber Coefficient { get; private set; }
            public int Power { get; private set; }
        }
    }
}