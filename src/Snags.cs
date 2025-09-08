//  Authors:  Caren Dymond, Sarah Beukema

using Landis.SpatialModeling;
using Landis.Core;
using System.Collections.Generic;
using Landis.Utilities;

namespace Landis.Extension.Succession.ForC
{
    public class Snags
    {
        public static bool bSnagsPresent = false;
        public const int NUMSNAGS = 1000;
        public static int[] DiedAt = new int[NUMSNAGS];
        public static int[] initSpecIdx = new int[NUMSNAGS];
        public static int[] initSnagAge = new int[NUMSNAGS];
        public static string[] initSnagDist = new string[NUMSNAGS];
        public static bool[] bSnagsUsed = new bool[NUMSNAGS];     //flag for if this site contained this snag type.

        private double[,] BioSnag = new double[2, PlugIn.ModelCore.Species.Count];

        public static void Initialize(IInputSnagParms parameters)
        {
            if (parameters != null)
            {
                bSnagsPresent = true;
                for (int i = 0; i < NUMSNAGS; i++)
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
