// NOTE: InputValueException --> Landis.Utilities

using Landis.Utilities;

namespace Landis.Extension.Succession.ForC
{
    public class ProbEstablishment : TimeInput, IProbEstablishment
    {
        double m_dProbEstablishment = 0.0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProbEstablishment()
        {
        }

        public ProbEstablishment(int nYear, double dProbEstablishment)
        {
            Year = nYear;
            Establishment = dProbEstablishment;
        }

        public double Establishment
        {
            get
            {
                return m_dProbEstablishment;
            }
            set
            {
                if (value < 0.0)
                    throw new InputValueException(value.ToString(),
                                                  "Establishment Probability must be >= 0.  The value provided is = {0}.", value);
                m_dProbEstablishment = value;
            }
        }
    }
}