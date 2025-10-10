// Authors: Caren Dymond, Sarah Beukema

namespace Landis.Extension.Succession.ForC
{
    public class Snags
    {
        public static bool bSnagsPresent = false;
        public static int[] DiedAt = new int[Constants.NUMSNAGS];
        public static int[] initSpecIdx = new int[Constants.NUMSNAGS];
        public static int[] initSnagAge = new int[Constants.NUMSNAGS];
        public static string[] initSnagDist = new string[Constants.NUMSNAGS];
        public static bool[] bSnagsUsed = new bool[Constants.NUMSNAGS];     // flag for if this site contained this snag type.
        private double[,] BioSnag = new double[2, PlugIn.ModelCore.Species.Count];

        public enum SnagType
        {
            STEMSNAGS = 0,
            OTHERSNAG
        };

        public static void Initialize(IInputSnagParams parameters)
        {
            if (parameters != null)
            {
                bSnagsPresent = true;
                for (int i = 0; i < Constants.NUMSNAGS; i++)
                {
                    DiedAt[i] = parameters.SnagAgeAtDeath[i];
                    initSnagAge[i] = parameters.SnagTimeSinceDeath[i];
                    initSpecIdx[i] = parameters.SnagSpecies[i];
                    initSnagDist[i] = parameters.SnagDisturb[i];
                    bSnagsUsed[i] = false;
                }
            }
        }
    }
}
