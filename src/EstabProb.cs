// NOTE: InputValueException --> Landis.Utilities.InputValueException

namespace Landis.Extension.Succession.ForC
{
    public class EstabProb : TimeInput, IEstabProb
    {
        double m_dEstabProb = 0.0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public EstabProb()
        {
        }

        public EstabProb(int nYear, double dEstabProb)
        {
            this.Year = nYear;
            this.Establishment = dEstabProb;
        }

        public double Establishment
        {
            get
            {
                return m_dEstabProb;
            }
            set
            {
                if (value < 0.0)
                    throw new InputValueException(value.ToString(),
                                                  "Establishment Probability must be >= 0.  The value provided is = {0}.", value);
                m_dEstabProb = value;
            }
        }
    }
}