// NOTE: InputValueException --> Landis.Utilities.InputValueException

namespace Landis.Extension.Succession.ForC
{
    class DisturbTransferFromPool : IDisturbTransferFromPool
    {
        private int m_nID;
        private string m_sName;
        private double m_dFracToAir = 0.0;
        private double m_dFracToFloor = 0.0;
        private double m_dFracToFPS = 0.0;
        private double m_dFracToDOM = 0.0;

        public DisturbTransferFromPool(int nID)
        {
            // Set the member data through the property, so error/range checking code doesn't have to be duplicated.
            ID = nID;
        }

        public DisturbTransferFromPool(int nID, string sName)
        {
            // Set the member data through the property, so error/range checking code doesn't have to be duplicated.
            ID = nID;
            Name = sName;
        }

        public DisturbTransferFromPool(int nID, double dFracToAir, double dFracToFloor, double dFracToFPS, double dFracToDOM)
        {
            // Set the member data through the property, so error/range checking code doesn't have to be duplicated.
            ID = nID;
            if ((dFracToAir + dFracToFloor + dFracToFPS + dFracToDOM) > 1.0)
                throw new InputValueException("Proportions",
                                              "Sum of all proportions must be no greater than 1.0.  The total of the proportions is = {0}.", dFracToAir + dFracToFloor + dFracToFPS + dFracToDOM);
            FracToAir = dFracToAir;
            FracToFloor = dFracToFloor;
            FracToFPS = dFracToFPS;
            FracToDOM = dFracToDOM;
        }

        public DisturbTransferFromPool(int nID, string sName, double dFracToAir, double dFracToFloor, double dFracToFPS, double dFracToDOM)
        {
            // Set the member data through the property, so error/range checking code doesn't have to be duplicated.
            ID = nID;
            Name = sName;
            if ((dFracToAir + dFracToFloor + dFracToFPS + dFracToDOM) > 1.0)
                throw new InputValueException("Proportions",
                                              "Sum of all proportions must be no greater than 1.0.  The total of the proportions is = {0}.", dFracToAir + dFracToFloor + dFracToFPS + dFracToDOM);
            FracToAir = dFracToAir;
            FracToFloor = dFracToFloor;
            FracToFPS = dFracToFPS;
            FracToDOM = dFracToDOM;
        }

        public int ID
        {
            get
            {
                return m_nID;
            }
            set
            {
                if (value <= 0)
                    throw new InputValueException(value.ToString(),
                                                  "ID must be greater than 0.  The value provided is = {0}.", value);
                m_nID = value;
            }
        }

        public string Name
        {
            get
            {
                return m_sName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new InputValueException(value.ToString(),
                                                  "A Name must be provided.");
                m_sName = value;
            }
        }

        public double FracToAir
        {
            get
            {
                return m_dFracToAir;
            }
            set
            {
                if ((value < 0.0) || (value > 1.0))
                    throw new InputValueException(value.ToString(),
                                                  "Proportion to Air must be in the range [0.0, 1.0].");
                m_dFracToAir = value;
            }
        }

        public double FracToFloor
        {
            get
            {
                return m_dFracToFloor;
            }
            set
            {
                if ((value < 0.0) || (value > 1.0))
                    throw new InputValueException(value.ToString(),
                                                  "Proportion to Floor must be in the range [0.0, 1.0].");
                m_dFracToFloor = value;
            }
        }

        public double FracToFPS
        {
            get
            {
                return m_dFracToFPS;
            }
            set
            {
                if ((value < 0.0) || (value > 1.0))
                    throw new InputValueException(value.ToString(),
                                                  "Proportion to FPS must be in the range [0.0, 1.0].");
                m_dFracToFPS = value;
            }
        }

        public double FracToDOM
        {
            get
            {
                return m_dFracToDOM;
            }
            set
            {
                if ((value < 0.0) || (value > 1.0))
                    throw new InputValueException(value.ToString(),
                                                  "Proportion to DOM must be in the range [0.0, 1.0].");
                m_dFracToDOM = value;
            }
        }
    }
}
