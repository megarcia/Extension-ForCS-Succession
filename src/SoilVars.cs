//  Authors:  Caren Dymond, Sarah Beukema

using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.UniversalCohorts;
using System.Collections.Generic;
using Landis.Utilities;
using System.IO;
using System;

namespace Landis.Extension.Succession.ForC
{
    public class SoilVars
    {
        public static IInputParameters iParams;
        public static IInputDMParameters iParamsDM;
        public static double[,] decayRates = new double[SoilClass.NUMSOILPOOLS, PlugIn.ModelCore.Species.Count];      //soil pool decay rates
        public static string[] DistType = new string[] { "none", "fire", "harvest", "wind", "bda", "drought", "defol", "other", "land use" };
        public static double[, ,] BioInput = new double[SoilClass.NUMBIOMASSCOMPONENTS, PlugIn.ModelCore.Species.Count, PlugIn.MaxLife];
        public static double[,] BioLive = new double[SoilClass.NUMBIOMASSCOMPONENTS, PlugIn.ModelCore.Species.Count];
    }
}
