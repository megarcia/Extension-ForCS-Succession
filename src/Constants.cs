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
        private const double BIOTOC = 0.5;
        public const int NUMSOILPOOLS = 10; // SoilPoolType.BLACKCARBON + 1;
        private const int NUMSNAGPOOLS = 2; // Snags.SnagType.OTHERSNAG + 1 i.e., stem and branches snag pool
        private const double FINEROOTSABOVERATIO = 0.5;
        private const double COARSEROOTABOVERATIO = 0.5;
        private const int NUMDISTURBANCES = 9;  // note, if add more dists, then increase this
    }
}
