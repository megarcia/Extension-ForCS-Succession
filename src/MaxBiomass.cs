// NOTE: InputValueException --> Landis.Utilities

using Landis.Utilities;

namespace Landis.Extension.Succession.ForC
{
    public class MaxBiomass : TimeInput, IMaxBiomass
    {
        double m_dMaxBiomass = 0.0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MaxBiomass()
        {
        }

        public MaxBiomass(int nYear, double dMaxBiomass)
        {
            Year = nYear;
            MaxBio = dMaxBiomass;
        }

        public double MaxBio
        {
            get
            {
                return m_dMaxBiomass;
            }
            set
            {
                if (value < 0.0)
                    throw new InputValueException(value.ToString(),
                                                  "Maximum biomass must be >= 0.  The value provided is = {0}.", value);
                m_dMaxBiomass = value;
            }
        }
    }
}