namespace Landis.Extension.Succession.ForC
{
    public class ClimateAnnual : TimeInput, IClimateAnnual
    {
        double m_dMeanAnnualTemp = 0.0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClimateAnnual()
        {
        }

        public ClimateAnnual(int nYear, double dMeanAnnualTemp)
        {
            Year = nYear;
            ClimateAnnualTemp = dMeanAnnualTemp;
        }

        public double ClimateAnnualTemp
        {
            get
            {
                return m_dMeanAnnualTemp;
            }
            set
            {
                m_dMeanAnnualTemp = value;
            }
        }
    }
}