// NOTE: InputValueException --> Landis.Utilities.InputValueException

namespace Landis.Extension.Succession.ForC
{
    public class ANPP : TimeInput, IANPP
    {
        double m_dGramsPerMetre2Year = 0.0;
        double m_dStdDev = 0.0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ANPP()
        {
        }

        public ANPP(int nYear, double dGramsPerMetre2Year, double dStdDev)
        {
            this.Year = nYear;
            this.GramsPerMetre2Year = dGramsPerMetre2Year;
            this.StdDev = dStdDev;
        }

        public double GramsPerMetre2Year
        {
            get
            {
                return m_dGramsPerMetre2Year;
            }
            set
            {
                if (value < 0.0)
                    throw new InputValueException(value.ToString(),
                                                  "Grams / m2-year must be >= 0.  The value provided is = {0}.", value);
                m_dGramsPerMetre2Year = value;
            }
        }

        public double StdDev
        {
            get
            {
                return m_dStdDev;
            }
            set
            {
                if (value < 0.0)
                    throw new InputValueException(value.ToString(),
                                                  "Std deviation must be >= 0.  The value provided is = {0}.", value);
                m_dStdDev = value;
            }
        }
    }
}
