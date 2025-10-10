// Authors: Caren Dymond, Sarah Beukema

// NOTE: ActiveSite --> Landis.SpatialModeling
// NOTE: ISpecies --> Landis.Core

using Landis.Core;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.ForC
{
    public struct SiteCohortToAdd
    {
        public ActiveSite site;
        public ISpecies species;
        public int newBiomass;

        public SiteCohortToAdd(ActiveSite site, ISpecies species, int newBiomass)
        {
            this.site = site;
            this.species = species;
            this.newBiomass = newBiomass;
        }
    }
}