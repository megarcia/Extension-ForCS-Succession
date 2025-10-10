// Authors: Caren Dymond, Sarah Beukema

namespace Landis.Extension.Succession.ForC
{
    public class SoilVars
    {
        public static IInputParams iParams;
        public static IInputDisturbanceMatrixParams iParamsDM;
        public static double[,] decayRates = new double[Constants.NUMSOILPOOLS, PlugIn.ModelCore.Species.Count];      //soil pool decay rates
        public static string[] DistType = new string[] { "none", "fire", "harvest", "wind", "bda", "drought", "defol", "other", "land use" };
        public static double[, ,] BioInput = new double[Constants.NUMBIOMASSCOMPONENTS, PlugIn.ModelCore.Species.Count, PlugIn.MaxLife];
        public static double[,] BioLive = new double[Constants.NUMBIOMASSCOMPONENTS, PlugIn.ModelCore.Species.Count];
    }
}
