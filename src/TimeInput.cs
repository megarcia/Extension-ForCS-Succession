using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Extension.Succession.ForC
{
    public class TimeInput : ITimeInput
    {
        int m_nYear = 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TimeInput()
        {
        }

        public TimeInput(int nYear)
        {
            this.Year = nYear;
        }

        public int Year
        {
            get
            {
                return m_nYear;
            }
            set
            {
                //if (value < 0)
                //    throw new Edu.Wisc.Forest.Flel.Util.InputValueException(value.ToString(), "Year must be >= 0.  The value provided is = {0}.", value);
                m_nYear = value;
            }
        }
    }
}
