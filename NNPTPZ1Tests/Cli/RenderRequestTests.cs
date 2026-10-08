using Microsoft.VisualStudio.TestTools.UnitTesting;
using NNPTPZ1.Cli;

namespace NNPTPZ1.Tests
{
    [TestClass]
    public class RenderRequestTests
    {
        private static string[] Required()
        {
            return new string[]
            {
                "--width", "60", "--height", "60",
                "--minX", "-2", "--maxX", "2",
                "--minY", "-2", "--maxY", "2",
                "--output", "out.png"
            };
        }

        private static RenderRequest Parse(params string[] args)
        {
            RenderRequest request;
            string error;

            Assert.IsTrue(
                RenderRequest.TryParse(args, out request, out error),
                "Parsing failed: " + error);

            return request;
        }

        private static void Reject(params string[] args)
        {
            RenderRequest request;
            string error;

            Assert.IsFalse(RenderRequest.TryParse(args, out request, out error));

            Assert.IsFalse(
                string.IsNullOrEmpty(error),
                "Rejected without a reason.");
        }

        [TestMethod]
        public void RequiredOptionsParse()
        {
            RenderRequest request = Parse(Required());

            Assert.AreEqual(60, request.Width);
            Assert.AreEqual(60, request.Height);
            Assert.AreEqual(-2.0, request.MinX);
            Assert.AreEqual(2.0, request.MaxX);
            Assert.AreEqual(-2.0, request.MinY);
            Assert.AreEqual(2.0, request.MaxY);
            Assert.AreEqual("out.png", request.OutputPath);
        }

        [TestMethod]
        public void MissingOptionsFallBackToDefaults()
        {
            RenderRequest request = Parse(Required());

            Assert.AreEqual("1 + x^2", request.PolynomialText);
            Assert.AreEqual(30, request.MaxNewtonIterations);
            Assert.AreEqual(0.01, request.RootMatchTolerance);
            Assert.AreEqual(2, request.ShadeStepPerIteration);
            Assert.AreEqual(0.0001, request.ZeroOffset);
        }

        [TestMethod]
        public void EqualsFormParses()
        {
            RenderRequest request = Parse(
                "--width=60", "--height=60",
                "--minX=-2", "--maxX=2",
                "--minY=-2", "--maxY=2",
                "--output=out.png");

            Assert.AreEqual(60, request.Width);
            Assert.AreEqual("out.png", request.OutputPath);
        }

        [TestMethod]
        public void OptionOrderDoesNotMatter()
        {
            RenderRequest first = Parse(
                "--width", "60", "--height", "60",
                "--minX", "-2", "--maxX", "2",
                "--minY", "-2", "--maxY", "2",
                "--output", "out.png",
                "--polynomial", "x^5 - 1", "--iterations", "50");

            RenderRequest second = Parse(
                "--iterations", "50", "--output", "out.png",
                "--maxY", "2", "--minY", "-2",
                "--maxX", "2", "--minX", "-2",
                "--polynomial", "x^5 - 1",
                "--height", "60", "--width", "60");

            Assert.AreEqual(first.Width, second.Width);
            Assert.AreEqual(first.Height, second.Height);
            Assert.AreEqual(first.MinX, second.MinX);
            Assert.AreEqual(first.MaxX, second.MaxX);
            Assert.AreEqual(first.MinY, second.MinY);
            Assert.AreEqual(first.MaxY, second.MaxY);
            Assert.AreEqual(first.OutputPath, second.OutputPath);
            Assert.AreEqual(first.PolynomialText, second.PolynomialText);
            Assert.AreEqual(first.MaxNewtonIterations, second.MaxNewtonIterations);
        }

        [TestMethod]
        public void OptionNamesAreCaseInsensitive()
        {
            RenderRequest request = Parse(
                "--WIDTH", "60", "--Height", "60",
                "--minx", "-2", "--MAXX", "2",
                "--miny", "-2", "--maxy", "2",
                "--OUTPUT", "out.png",
                "--Iterations", "50", "--SHADESTEP", "0");

            Assert.AreEqual(60, request.Width);
            Assert.AreEqual(50, request.MaxNewtonIterations);
            Assert.AreEqual(0, request.ShadeStepPerIteration);
        }

        [TestMethod]
        public void EachRendererOptionReachesTheRequest()
        {
            RenderRequest request = Parse(
                "--width", "60", "--height", "60",
                "--minX", "-2", "--maxX", "2",
                "--minY", "-2", "--maxY", "2",
                "--output", "out.png",
                "--polynomial", "x^5 - 1",
                "--iterations", "50",
                "--tolerance", "0.5",
                "--shadeStep", "0",
                "--zeroOffset", "0.5");

            Assert.AreEqual("x^5 - 1", request.PolynomialText);
            Assert.AreEqual(50, request.MaxNewtonIterations);
            Assert.AreEqual(0.5, request.RootMatchTolerance);
            Assert.AreEqual(0, request.ShadeStepPerIteration);
            Assert.AreEqual(0.5, request.ZeroOffset);
        }

        [TestMethod]
        public void HelpTextMentionsEveryOption()
        {
            string help = RenderRequest.Help;

            foreach (string option in new string[]
            {
                "--width", "--height", "--minX", "--maxX", "--minY", "--maxY",
                "--output", "--polynomial", "--iterations", "--tolerance",
                "--shadeStep", "--zeroOffset", "--help"
            })
            {
                Assert.IsTrue(help.Contains(option), "Help misses " + option);
            }
        }

        [DataTestMethod]
        [DataRow("--help", true)]
        [DataRow("--HELP", true)]
        [DataRow("-h", true)]
        public void RecognizesHelpRequest(string argument, bool expected)
        {
            Assert.AreEqual(expected, RenderRequest.IsHelpRequest(new string[] { argument }));
        }

        [TestMethod]
        public void OrdinaryArgumentsAreNotHelp()
        {
            Assert.IsFalse(RenderRequest.IsHelpRequest(Required()));
            Assert.IsFalse(RenderRequest.IsHelpRequest(new string[0]));
            Assert.IsFalse(RenderRequest.IsHelpRequest(null));
        }

        [DataTestMethod]
        [DataRow("--width", "0")]
        [DataRow("--width", "-5")]
        [DataRow("--width", "abc")]
        [DataRow("--width", "10.5")]
        [DataRow("--height", "0")]
        [DataRow("--iterations", "0")]
        [DataRow("--iterations", "-5")]
        [DataRow("--iterations", "abc")]
        [DataRow("--tolerance", "0")]
        [DataRow("--tolerance", "-1")]
        [DataRow("--tolerance", "NaN")]
        [DataRow("--shadeStep", "-1")]
        [DataRow("--shadeStep", "abc")]
        [DataRow("--zeroOffset", "0")]
        [DataRow("--zeroOffset", "-1")]
        [DataRow("--minX", "abc")]
        [DataRow("--minX", "NaN")]
        [DataRow("--minX", "Infinity")]
        public void RejectsBadValues(string option, string value)
        {
            string[] args = Required();
            string[] extended = new string[args.Length + 2];
            args.CopyTo(extended, 0);
            extended[args.Length] = option;
            extended[args.Length + 1] = value;

            Reject(extended);
        }

        [TestMethod]
        public void AllowsZeroShadeStep()
        {
            string[] args = Required();
            string[] extended = new string[args.Length + 2];
            args.CopyTo(extended, 0);
            extended[args.Length] = "--shadeStep";
            extended[args.Length + 1] = "0";

            Assert.AreEqual(0, Parse(extended).ShadeStepPerIteration);
        }

        [TestMethod]
        public void RejectsReversedRanges()
        {
            Reject(
                "--width", "60", "--height", "60",
                "--minX", "2", "--maxX", "-2",
                "--minY", "-2", "--maxY", "2",
                "--output", "out.png");

            Reject(
                "--width", "60", "--height", "60",
                "--minX", "-2", "--maxX", "2",
                "--minY", "2", "--maxY", "-2",
                "--output", "out.png");
        }

        [TestMethod]
        public void RejectsMissingRequiredOption()
        {
            Reject(
                "--height", "60",
                "--minX", "-2", "--maxX", "2",
                "--minY", "-2", "--maxY", "2",
                "--output", "out.png");
        }

        [TestMethod]
        public void RejectsUnknownAndDuplicateOptions()
        {
            string[] args = Required();
            string[] withUnknown = new string[args.Length + 2];
            args.CopyTo(withUnknown, 0);
            withUnknown[args.Length] = "--nonsense";
            withUnknown[args.Length + 1] = "5";
            Reject(withUnknown);

            Reject(
                "--width", "60", "--width", "99", "--height", "60",
                "--minX", "-2", "--maxX", "2",
                "--minY", "-2", "--maxY", "2",
                "--output", "out.png");
        }

        [TestMethod]
        public void RejectsMalformedOptions()
        {
            Reject();
            Reject("60", "60");
            Reject("--width");
            Reject("--", "60");
            Reject("--width", "60", "--height", "60");
        }
    }
}
