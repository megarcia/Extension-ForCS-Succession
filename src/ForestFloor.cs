namespace Landis.Extension.Succession.ForC
{
    public class ForestFloor
    {
        /// <summary>
        /// this routine is called from disturbances such as drought
        /// </summary>
        /// <param name="site"></param>
        /// <param name="species"></param>
        /// <param name="age"></param>
        /// <param name="wood"></param>
        /// <param name="nonwood"></param>
        /// <param name="DistTypeName"></param>
        /// <param name="tmpFireSeverity"></param>
        public static void DisturbanceImpactsBiomass(ActiveSite site, ISpecies species, int age, double wood, double nonwood, string DistTypeName, int tmpFireSeverity)
        {
            SiteVars.soils[site].DisturbanceImpactsBiomass(site, species, age, wood, nonwood, DistTypeName, 0);  
            int iage = age;
        }
    }
}
