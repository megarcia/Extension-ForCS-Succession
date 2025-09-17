namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// Constants used throughout ForC 
    /// </summary>
    public class Constants
    {
        public const int FIREINTENSITYCOUNT = 5;
        public const int NUMSNAGS = 1000;
        public const int NUMBIOMASSCOMPONENTS = 6;  // ComponentType.FINEROOT + 1, The total number of biomass components.
        public const double BIOTOC = 0.5;
        public const int NUMSOILPOOLS = 10; // SoilPoolType.BLACKCARBON + 1;
        public const int NUMSNAGPOOLS = 2; // Snags.SnagType.OTHERSNAG + 1 i.e., stem and branches snag pool
        public const double FINEROOTSABOVERATIO = 0.5;
        public const double COARSEROOTABOVERATIO = 0.5;
        public const int NUMDISTURBANCES = 9;  // note, if adding more disturbances, then increase this
        public const double DECAYREFTEMP = 10.0;  // reference soil temperature for decay calculation; originally decayRefTemp from SoilDecay.CalcDecayFTemp
        public const int NUMSLOWPOOLS = 2;  // number of above- and below-ground slow soil carbon pool; originally numberABSlowPool from Soils.DoSoilDynamics
        public const int AGSLOWPOOLIDX = 0;  // above-ground slow carbon pool; originally aboveSlowPool from Soils.DoSoilDynamics
        public const int BGSLOWPOOLIDX = 1;  // below-ground slow soil carbon pool; originally belowSlowPool from Soils.DoSoilDynamics
    }
}
