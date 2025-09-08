using System;
using System.Collections.Generic;
using System.Text;
using Landis.Utilities;

namespace Landis.Extension.Succession.ForC
{
    class DisturbTransferFromPool : IDisturbTransferFromPool
    {
        public const string mk_sDisturbTypeFire = "Fire";
        public const string mk_sDisturbTypeHarvest = "Harvest";

        private int m_nID;
        private string m_sName;
        private double m_dPropToAir = 0.0;
        private double m_dPropToFloor = 0.0;
        private double m_dPropToFPS = 0.0;
        private double m_dPropToDOM = 0.0;

        public DisturbTransferFromPool(int nID)
        {
            // Set the member data through the property, so error/range checking code doesn't have to be duplicated.
            this.ID = nID;
        }

        public DisturbTransferFromPool(int nID, string sName)
        {
            // Set the member data through the property, so error/range checking code doesn't have to be duplicated.
            this.ID = nID;
            this.Name = sName;
        }

        public DisturbTransferFromPool(int nID, double dPropToAir, double dPropToFloor, double dPropToFPS, double dPropToDOM)
        {
            // Set the member data through the property, so error/range checking code doesn't have to be duplicated.
            this.ID = nID;
            if ((dPropToAir + dPropToFloor + dPropToFPS + dPropToDOM) > 1.0)
                throw new Landis.Utilities.InputValueException("Proportions", "Sum of all proportions must be no greater than 1.0.  The total of the proportions is = {0}.", dPropToAir + dPropToFloor + dPropToFPS + dPropToDOM);
            this.PropToAir = dPropToAir;
            this.PropToFloor = dPropToFloor;
            this.PropToFPS = dPropToFPS;
            this.PropToDOM = dPropToDOM;
        }

        public DisturbTransferFromPool(int nID, string sName, double dPropToAir, double dPropToFloor, double dPropToFPS, double dPropToDOM)
        {
            // Set the member data through the property, so error/range checking code doesn't have to be duplicated.
            this.ID = nID;
            this.Name = sName;
            if ((dPropToAir + dPropToFloor + dPropToFPS + dPropToDOM) > 1.0)
                throw new Landis.Utilities.InputValueException("Proportions", "Sum of all proportions must be no greater than 1.0.  The total of the proportions is = {0}.", dPropToAir + dPropToFloor + dPropToFPS + dPropToDOM);
            this.PropToAir = dPropToAir;
            this.PropToFloor = dPropToFloor;
            this.PropToFPS = dPropToFPS;
            this.PropToDOM = dPropToDOM;
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
                    throw new Landis.Utilities.InputValueException(value.ToString(), "ID must be greater than 0.  The value provided is = {0}.", value);
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
                    throw new Landis.Utilities.InputValueException(value.ToString(), "A Name must be provided.");
                m_sName = value;
            }
        }

        public double PropToAir
        {
            get
            {
                return m_dPropToAir;
            }
            set
            {
                if ((value < 0.0) || (value > 1.0))
                    throw new Landis.Utilities.InputValueException(value.ToString(), "Proportion to Air must be in the range [0.0, 1.0].");
                m_dPropToAir = value;
            }
        }

        public double PropToFloor
        {
            get
            {
                return m_dPropToFloor;
            }
            set
            {
                if ((value < 0.0) || (value > 1.0))
                    throw new InputValueException(value.ToString(), "Proportion to Floor must be in the range [0.0, 1.0].");
                m_dPropToFloor = value;
            }
        }

        public double PropToFPS
        {
            get
            {
                return m_dPropToFPS;
            }
            set
            {
                if ((value < 0.0) || (value > 1.0))
                    throw new InputValueException(value.ToString(), "Proportion to FPS must be in the range [0.0, 1.0].");
                m_dPropToFPS = value;
            }
        }

        public double PropToDOM
        {
            get
            {
                return m_dPropToDOM;
            }
            set
            {
                if ((value < 0.0) || (value > 1.0))
                    throw new Landis.Utilities.InputValueException(value.ToString(), "Proportion to DOM must be in the range [0.0, 1.0].");
                m_dPropToDOM = value;
            }
        }
    }
}
