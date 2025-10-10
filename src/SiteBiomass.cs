// Authors: Robert M. Scheller, James B. Domingo

// NOTE: ActiveSite --> Landis.SpatialModeling
// NOTE: ICohort --> Landis.Library.UniversalCohorts
// NOTE: ICommunity --> Landis.Library.InitialCommunities.Universal
// NOTE: IEcoregion --> Landis.Core
// NOTE: ISpecies --> Landis.Core
// NOTE: ISpeciesCohorts --> Landis.Library.UniversalCohorts
// NOTE: SiteCohorts --> Landis.Library.UniversalCohorts

using System.Collections.Generic;
using Landis.Core;
using Landis.Library.InitialCommunities.Universal;
using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// The initial live and dead biomass at a site.
    /// </summary>
    public class SiteBiomass
    {
        public Soils soils;
        public double SoilOrganicMatterC;
        public double DeadWoodMass;
        public double LitterMass;
        public double DeadWoodDecayRate;
        public double LitterDecayRate;
        private SiteCohorts cohorts;


        /// <summary>
        /// The site's initial cohorts.
        /// </summary>
        public SiteCohorts Cohorts
        {
            get
            {
                return cohorts;
            }
        }
        
        private SiteBiomass(SiteCohorts cohorts,
                               double soilOrganicMatterC,
                               double deadWoodMass,
                               double litterMass,
                               double deadWoodDecayRate,
                               double litterDecayRate,
                               Soils soils)
        {
            this.cohorts = cohorts;
            SoilOrganicMatterC = soilOrganicMatterC;
            DeadWoodMass = deadWoodMass;
            LitterMass = litterMass;
            DeadWoodDecayRate = deadWoodDecayRate;
            LitterDecayRate = litterDecayRate;
            this.soils = soils;
        }

        public static SiteCohorts Clone(SiteCohorts site_cohorts)
        {
            SiteCohorts clone = new SiteCohorts();
            foreach (ISpeciesCohorts speciesCohorts in site_cohorts)
                foreach (ICohort cohort in speciesCohorts)
                    clone.AddNewCohort(cohort.Species, cohort.Data.Age, cohort.Data.Biomass, new System.Dynamic.ExpandoObject());  
            return clone;
        }

        // Initial site biomass for each unique pair of initial
        // community and ecoregion; Key = 32-bit unsigned integer where
        // high 16-bits is the map code of the initial community and the
        // low 16-bits is the ecoregion's map code
        private static IDictionary<uint, SiteBiomass> initialSites;

        // Age cohorts for an initial community sorted from oldest to
        // youngest.  Key = initial community's map code
        private static IDictionary<uint, List<ICohort>> sortedCohorts;

        private static ushort successionTimestep;

        private static uint CalcKey(uint initCommunityMapCode,
                                       ushort ecoregionMapCode)
        {
            return (initCommunityMapCode << 16) | ecoregionMapCode;
        }

        static SiteBiomass()
        {
            initialSites = new Dictionary<uint, SiteBiomass>();
            sortedCohorts = new Dictionary<uint, List<ICohort>>();
        }

        /// <summary>
        /// Initializes this class.
        /// </summary>
        /// <param name="timestep">
        /// The plug-in's timestep.  It is used for growing biomass cohorts.
        /// </param>
        public static void Initialize(int timestep)
        {
            successionTimestep = (ushort)timestep;
        }


        /// <summary>
        /// Calculates the initial biomass at a site.
        /// </summary>
        /// <param name="site">
        /// The selected site.
        /// </param>
        /// <param name="initialCommunity">
        /// The initial community of age cohorts at the site.
        /// </param>
        public static SiteBiomass CalcInitSiteBiomass(ActiveSite site,
                                                      ICommunity initialCommunity)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            uint key = CalcKey(initialCommunity.MapCode, ecoregion.MapCode);
            SiteBiomass initSiteBiomass;
            if (initialSites.TryGetValue(key, out initSiteBiomass))
                return initSiteBiomass;
            //  If we don't have a sorted list of age cohorts for the initial
            //  community, make the list
            List<ICohort> sortedAgeCohorts;
            if (!sortedCohorts.TryGetValue(initialCommunity.MapCode, out sortedAgeCohorts))
            {
                sortedAgeCohorts = SortCohorts(initialCommunity.Cohorts);
                sortedCohorts[initialCommunity.MapCode] = sortedAgeCohorts;
            }
            SiteCohorts cohorts = MakeBiomassCohorts(sortedAgeCohorts, site);
            initSiteBiomass = new SiteBiomass(cohorts,
                                                SiteVars.SoilOrganicMatterC[site],
                                                SiteVars.DeadWoodMass[site].Mass,
                                                SiteVars.LitterMass[site].Mass,
                                                SiteVars.DeadWoodDecayRate[site],
                                                SiteVars.LitterDecayRate[site],
                                                SiteVars.soils[site]);
            initialSites[key] = initSiteBiomass;
            return initSiteBiomass;
        }

        /// <summary>
        /// Makes a list of age cohorts in an initial community sorted from
        /// oldest to youngest.
        /// </summary>
        public static List<ICohort> SortCohorts(List<ISpeciesCohorts> sppCohorts)
        {
            List<ICohort> cohorts = new List<ICohort>();
            foreach (ISpeciesCohorts speciesCohorts in sppCohorts)
            {
                foreach (ICohort cohort in speciesCohorts)
                    cohorts.Add(cohort);
            }
            cohorts.Sort(Library.UniversalCohorts.Util.WhichIsOlderCohort);
            return cohorts;
        }

        /// <summary>
        /// A method that computes the initial biomass for a new cohort at a
        /// site based on the existing cohorts.
        /// </summary>
        public delegate int CalculationMethod(ISpecies species,
                                              SiteCohorts siteCohorts,
                                              ActiveSite  site);

        /// <summary>
        /// Makes the set of biomass cohorts at a site based on the age cohorts
        /// at the site, using a specified method for computing a cohort's
        /// initial biomass.
        /// </summary>
        /// <param name="ageCohorts">
        /// A sorted list of age cohorts, from oldest to youngest.
        /// </param>
        /// <param name="site">
        /// Site where cohorts are located.
        /// </param>
        /// <param name="initSiteBiomassMethod">
        /// The method for computing the initial biomass for a new cohort.
        /// </param>
        public static SiteCohorts MakeBiomassCohorts(List<ICohort> ageCohorts,
                                                     ActiveSite site,
                                                     CalculationMethod initSiteBiomassMethod)
        {
            SiteVars.Cohorts[site] = new SiteCohorts();
            if (ageCohorts.Count == 0)
                return SiteVars.Cohorts[site];
            if (SoilVars.iParams.BiomassSpinUpFlag == 0)
            {
                foreach (var cohort in ageCohorts)
                {
                    SiteVars.Cohorts[site].AddNewCohort(cohort.Species,
                                                        cohort.Data.Age,
                                                        cohort.Data.Biomass,
                                                        cohort.Data.AdditionalParameters);
                    SiteVars.TotalBiomass[site] = Library.UniversalCohorts.Cohorts.ComputeNonYoungBiomass(SiteVars.Cohorts[site]);
                    SiteVars.soils[site].CollectBiomassMortality(cohort.Species, 0, 0, 0, 0);      // dummy for getting it to recognize that the species is now present.
                }
            }
            else
                SpinUpBiomassCohorts(ageCohorts, site, initSiteBiomassMethod);
            if (SoilVars.iParams.SoilSpinUpFlag == 0)
                ReadSoilValuesFromTable(site);
            else
                SiteVars.soils[site].SpinupSoils(site);
            SiteVars.soils[site].LastInitialSoilPass(site);
            return SiteVars.Cohorts[site];
        }

        private static void ReadSoilValuesFromTable(ActiveSite site)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            foreach (var cohort in SiteVars.Cohorts[site])
                SoilDecay.CalcDecayRates(ecoregion, cohort.Species);  // MG 20250911 updated procedure removes site argument
        }

        private static void SpinUpBiomassCohorts(List<ICohort> ageCohorts,
                                                 ActiveSite site,
                                                 CalculationMethod initSiteBiomassMethod)
        {
            int indexNextAgeCohort = 0;
            //  The index in the list of sorted age cohorts of the next
            //  cohort to be considered
            //  Loop through time from -N to 0 where N is the oldest cohort.
            //  So we're going from the time when the oldest cohort was "born"
            //  to the present time (= 0).  Because the age of any age cohort
            //  is a multiple of the succession timestep, we go from -N to 0
            //  by that timestep.  NOTE: the case where timestep = 1 requires
            //  special treatment because if we start at time = -N with a
            //  cohort with age = 1, then at time = 0, its age will N+1 not N.
            //  Therefore, when timestep = 1, the ending time is -1.
            int endTime = (successionTimestep == 1) ? -1 : 0;
            for (int time = -ageCohorts[0].Data.Age; time <= endTime; time += successionTimestep)
            {
                EcoregionData.GetAnnualTemperature(successionTimestep, 0);                        // ForCS CLIMATE
                SpeciesData.GenerateNewANPPandMaxBiomass(successionTimestep, time);
                //  Grow current biomass cohorts.
                if (time == endTime)
                {
                    SiteVars.soils[site].lastAge = ageCohorts[0].Data.Age;       // signal both that the spin-up is done, and the oldest age
                    SiteVars.soils[site].bKillNow = false;
                }
                PlugIn.GrowCohorts(site, successionTimestep, true);
                //  Add those cohorts that were born at the current year
                while (indexNextAgeCohort < ageCohorts.Count &&
                       ageCohorts[indexNextAgeCohort].Data.Age == -time)
                {
                    ISpecies species = ageCohorts[indexNextAgeCohort].Species;
                    int initSiteBiomass = initSiteBiomassMethod(species, SiteVars.Cohorts[site], site);
                    SiteVars.Cohorts[site].AddNewCohort(ageCohorts[indexNextAgeCohort].Species, 1,
                                                        initSiteBiomass, new System.Dynamic.ExpandoObject());
                    SiteVars.soils[site].CollectBiomassMortality(species, 0, 0, 0, 0);      //dummy for getting it to recognize that the species is now present.
                    indexNextAgeCohort++;
                }
                // just before the last timestep, we want to send a signal 
                // to the growth routine to kill the cohorts that need to 
                // be snags. Hardwire this first for testing.
                if (time == endTime - 1)
                    SiteVars.soils[site].bKillNow = true;
                if (time == endTime)
                {
                    SiteVars.soils[site].lastAge = ageCohorts[0].Data.Age;       //signal both that the spin-up is done, and the oldest age
                    SiteVars.soils[site].bKillNow = false;
                }
            }
        }

        /// <summary>
        /// Makes the set of biomass cohorts at a site based on the age cohorts
        /// at the site, using the default method for computing a cohort's
        /// initial biomass.
        /// </summary>
        /// <param name="ageCohorts">
        /// A sorted list of age cohorts, from oldest to youngest.
        /// </param>
        /// <param name="site">
        /// Site where cohorts are located.
        /// </param>
        public static SiteCohorts MakeBiomassCohorts(List<ICohort> ageCohorts,
                                                     ActiveSite site)
        {
            return MakeBiomassCohorts(ageCohorts, site, CohortBiomass.CalcInitCohortBiomass);
        }
    }
}
