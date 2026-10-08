using Microsoft.VisualStudio.TestTools.UnitTesting;
using NNPTPZ1.Mathematics;

namespace NNPTPZ1.Mathematics.Tests
{
    [TestClass]
    public class PolynomialParserTests
    {
        private static string Parse(string text)
        {
            Polynomial polynomial;
            string error;

            Assert.IsTrue(
                Polynomial.TryParse(text, out polynomial, out error),
                "Parsing '" + text + "' failed: " + error);

            return polynomial.ToString();
        }

        private static void Reject(string text)
        {
            Polynomial polynomial;
            string error;

            Assert.IsFalse(
                Polynomial.TryParse(text, out polynomial, out error),
                "Parsing '" + text + "' should have failed.");

            Assert.IsFalse(
                string.IsNullOrEmpty(error),
                "Parsing '" + text + "' failed without a reason.");
        }

        [DataTestMethod]
        [DataRow("1 + x^2", "(1 + 0i) + (0 + 0i)x + (1 + 0i)x^2")]
        [DataRow("x^3 + 1", "(1 + 0i) + (0 + 0i)x + (0 + 0i)x^2 + (1 + 0i)x^3")]
        [DataRow("x", "(0 + 0i) + (1 + 0i)x")]
        [DataRow("x^2", "(0 + 0i) + (0 + 0i)x + (1 + 0i)x^2")]
        [DataRow("x^3+1", "(1 + 0i) + (0 + 0i)x + (0 + 0i)x^2 + (1 + 0i)x^3")]
        [DataRow("X^3 + 1", "(1 + 0i) + (0 + 0i)x + (0 + 0i)x^2 + (1 + 0i)x^3")]
        [DataRow("x^5 - 1", "(-1 + 0i) + (0 + 0i)x + (0 + 0i)x^2 + (0 + 0i)x^3 + (0 + 0i)x^4 + (1 + 0i)x^5")]
        [DataRow("-x^2", "(0 + 0i) + (0 + 0i)x + (-1 + 0i)x^2")]
        [DataRow("x^2 - x + 1", "(1 + 0i) + (-1 + 0i)x + (1 + 0i)x^2")]
        [DataRow("2*x^3 + 1", "(1 + 0i) + (0 + 0i)x + (0 + 0i)x^2 + (2 + 0i)x^3")]
        [DataRow("3x^2", "(0 + 0i) + (0 + 0i)x + (3 + 0i)x^2")]
        [DataRow("x^3 + i*x", "(0 + 0i) + (0 + 1i)x + (0 + 0i)x^2 + (1 + 0i)x^3")]
        [DataRow("0.5*x^2 + 1", "(1 + 0i) + (0 + 0i)x + (0.5 + 0i)x^2")]
        public void ParsesValidPolynomial(string text, string expected)
        {
            Assert.AreEqual(expected, Parse(text));
        }

        [TestMethod]
        public void ParsesHighPowers()
        {
            string actual = Parse("x^10");

            Assert.IsFalse(actual.Contains("xxx"));
            Assert.IsTrue(actual.Contains("x^10"));
        }

        [TestMethod]
        public void ParsedPolynomialEvaluatesCorrectly()
        {
            Polynomial polynomial;
            string error;

            Assert.IsTrue(Polynomial.TryParse("1 + x^2", out polynomial, out error), error);
            Assert.AreEqual(1.0, polynomial.Eval(0.0).Real);
            Assert.AreEqual(2.0, polynomial.Eval(1.0).Real);
            Assert.AreEqual(5.0, polynomial.Eval(2.0).Real);
            Assert.AreEqual("(0 + 0i) + (2 + 0i)x", polynomial.Derive().ToString());
        }

        [TestMethod]
        public void ParsedRootsSatisfyEquation()
        {
            Polynomial polynomial;
            string error;

            Assert.IsTrue(Polynomial.TryParse("1 + x^2", out polynomial, out error), error);

            ComplexNumber plusI = new ComplexNumber { Real = 0, Imaginary = 1 };
            ComplexNumber minusI = new ComplexNumber { Real = 0, Imaginary = -1 };

            Assert.AreEqual(0.0, polynomial.Eval(plusI).GetAbsoluteValue(), 1e-12);
            Assert.AreEqual(0.0, polynomial.Eval(minusI).GetAbsoluteValue(), 1e-12);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("5")]
        [DataRow("x^")]
        [DataRow("abc")]
        [DataRow("x^2+")]
        [DataRow("x^^2")]
        public void RejectsInvalidPolynomial(string text)
        {
            Reject(text);
        }
    }
}
