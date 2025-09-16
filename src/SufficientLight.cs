// Authors: Caren Dymond, Sarah Beukema

// NOTE: InputValueException --> Landis.Utilities.InputValueException

namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// Definition of the probability of germination under different light levels for 5 shade classes.
    /// </summary>
    public class SufficientLight : ISufficientLight
    {
        private byte shadeClass;
        private double probSufficientLight0;
        private double probSufficientLight1;
        private double probSufficientLight2;
        private double probSufficientLight3;
        private double probSufficientLight4;
        private double probSufficientLight5;
        
        public SufficientLight()
        {
        }

        /// <summary>
        /// The shade class (between 1 and 5).
        /// </summary>
        public byte ShadeClass
        {
            get
            {
                return shadeClass;
            }
            set
            {
                if (value > 5 || value < 1)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 1 and 5.");
                shadeClass = value;
            }
        }

        public double ProbSufficientLight0
        {
            get
            {
                return probSufficientLight0;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0 and 1");
                probSufficientLight0 = value;
            }
        }

        public double ProbSufficientLight1
        {
            get
            {
                return probSufficientLight1;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0 and 1");
                probSufficientLight1 = value;
            }
        }

        public double ProbSufficientLight2
        {
            get
            {
                return probSufficientLight2;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0 and 1");
                probSufficientLight2 = value;
            }
        }

        public double ProbSufficientLight3
        {
            get
            {
                return probSufficientLight3;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0 and 1");
                probSufficientLight3 = value;
            }
        }

        public double ProbSufficientLight4
        {
            get
            {
                return probSufficientLight4;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0 and 1");
                probSufficientLight4 = value;
            }
        }

        public double ProbSufficientLight5
        {
            get
            {
                return probSufficientLight5;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0 and 1");
                probSufficientLight5 = value;
            }
        }
    }
}
