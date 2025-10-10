// Authors: Caren Dymond, Sarah Beukema

// NOTE: InputValue --> Landis.Utilities
// NOTE: ISpecies --> Landis.Core

using Landis.Core;
using Landis.Utilities;

namespace Landis.Extension.Succession.ForC
{
    public class InputSnagParams : IInputSnagParams
    {
        /// <summary>
        /// Initial Snag variables
        /// </summary>
        private int[] m_SnagSpecies;
        private int[] m_SnagAgeAtDeath;
        private int[] m_SnagTimeSinceDeath;
        private string[] m_SnagDisturb;

        public InputSnagParams()
        {
            m_SnagAgeAtDeath = new int[Constants.NUMSNAGS];
            m_SnagTimeSinceDeath = new int[Constants.NUMSNAGS];
            m_SnagSpecies = new int[Constants.NUMSNAGS];
            m_SnagDisturb = new string[Constants.NUMSNAGS];
        }

        /// <summary>
        /// Initial snag information
        /// </summary>
        /// <param name="species"></param>
        /// <param name="dAgeAtDeath"></param>
        /// <param name="dTimeSinceDeath"></param>
        /// <param name="sDisturbType"></param>
        /// <param name="i"></param>
        public void SetInitSnagInfo(ISpecies species, InputValue<int> dAgeAtDeath, InputValue<int> dTimeSinceDeath, InputValue<string> sDisturbType, int i)
        {
            m_SnagSpecies[i] = species.Index;
            m_SnagAgeAtDeath[i] = Util.CheckBiomassParm(dAgeAtDeath, 0, 999);
            m_SnagTimeSinceDeath[i] = Util.CheckBiomassParm(dTimeSinceDeath, 0, 999);
            m_SnagDisturb[i] = sDisturbType;
        }

        public int[] SnagSpecies
        {
            get
            {
                return m_SnagSpecies;
            }
        }

        public int[] SnagAgeAtDeath
        {
            get
            {
                return m_SnagAgeAtDeath;
            }
        }

        public int[] SnagTimeSinceDeath
        {
            get
            {
                return m_SnagTimeSinceDeath;
            }
        }

        public string[] SnagDisturb
        {
            get
            {
                return m_SnagDisturb;
            }
        }
    }
}
