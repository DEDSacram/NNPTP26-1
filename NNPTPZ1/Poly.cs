using System.Collections.Generic;

namespace NNPTPZ1.Mathematics
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
    }
}