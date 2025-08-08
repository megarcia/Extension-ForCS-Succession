//  Authors:  Robert M. Scheller

using Landis.Core;
using System.Collections.Generic;

namespace Landis.Extension.Succession.ForC
{

    public interface IEcoSppDOMDynamicInputRecord
    {
        ISpecies Species { get; set; }
        int NDOMPoolID { get; set; }
        double DecayRate { get; set; }
        double AmountT0 { get; set; }
        double Q10 { get; set; }
    }

    public class EcoSppDOMDynamicInputRecord
    : IEcoSppDOMDynamicInputRecord
    {
        private ISpecies species;
        private int nDOMPoolID;
        private double decayRate;
        private double amountT0;
        private double q10;

        public ISpecies Species
        {
            get
            {
                return species;
            }
            set
            {
                species = value;
            }
        }

        public int NDOMPoolID
        {
            get
            {
                return nDOMPoolID;
            }
            set
            {
                nDOMPoolID = value;
            }
        }

        public double DecayRate
        {
            get
            {
                return decayRate;
            }
            set
            {
                decayRate = value;
            }
        }

        public double AmountT0
        {
            get
            {
                return amountT0;
            }
            set
            {
                amountT0 = value;
            }
        }

        public double Q10
        {
            get
            {
                return q10;
            }
            set
            {
                q10 = value;
            }
        }

        public EcoSppDOMDynamicInputRecord()
        {
        }

    }
}
