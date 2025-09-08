namespace Landis.Extension.Succession.ForC
{
    public class ForestFloor
    {
        // this routine is called from disturbances such as drought
        public static void DisturbanceImpactsBiomass(ActiveSite site, ISpecies species, int age, double wood, double nonwood, string DistTypeName, int tmpFireSeverity)
        {
            SiteVars.soilClass[site].DisturbanceImpactsBiomass(site, species, age, wood, nonwood, DistTypeName, 0);  
            int iage = age;
        }
    }
}
