// Authors: Caren Dymond, Sarah Beukema

// NOTE: ActiveSite --> Landis.SpatialModeling
// NOTE: base --> Landis.Library.Succession
// NOTE: Cohort --> Landis.Library.UniversalCohorts
// NOTE: DatasetParser --> Landis.Core
// NOTE: Disturbed --> Landis.Library.Succession
// NOTE: ExtensionType --> Landis.Core
// NOTE: ICohort --> Landis.Library.UniversalCohorts
// NOTE: ICommunity --> Landis.Library.InitialCommunities.Universal
// NOTE: ICore --> Landis.Core
// NOTE: IDataset --> Landis.Library.InitialCommunities.Universal
// NOTE: IEcoregion --> Landis.Core
// NOTE: IInputRaster --> Landis.SpatialModeling
// NOTE: ISpecies --> Landis.Core
// NOTE: Landis.Data --> Landis.Core
// NOTE: Reproduction --> Landis.Library.Succession
// NOTE: UIntPixel --> Landis.SpatialModeling

using System;
using System.Collections.Generic;
using System.Linq;
using Landis.Core;
using Landis.Library.InitialCommunities.Universal;
using Landis.Library.Succession;
using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.ForC
{
    public class PlugIn : Landis.Library.Succession.ExtensionBase
    {
        private static ICore modelCore;
        private IInputParams inputParams;
        private IInputSnagParams inputSnagParams;
        private IInputClimateParams inputClimateParams;
        private IInputDisturbanceMatrixParams inputDisturbanceMatrixParams;
        public static bool CalibrateMode;
        public static double CurrentYearSiteMortality;
        public static int MaxLife;
        private ICommunity initialCommunity;
        private static List<SiteCohortToAdd> siteCohortsToAdd;
        private List<ILight> sufficientLight;

        public PlugIn() : base(Names.ExtensionName)
        {
            siteCohortsToAdd = new List<SiteCohortToAdd>();
        }

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            InputParamsParser parser = new InputParamsParser();
            inputParams = Landis.Data.Load<IInputParams>(dataFile, parser);
            InputClimateParser parser3 = new InputClimateParser();
            inputClimateParams = Landis.Data.Load<IInputClimateParams>(inputParams.ClimateFile2, parser3);
            InputDisturbanceMatrixParser parser4 = new InputDisturbanceMatrixParser();
            inputDisturbanceMatrixParams = Landis.Data.Load<IInputDisturbanceMatrixParams>(inputParams.DMFile, parser4);
            if (inputParams.InitSnagFile != null)
            {
                InputSnagParser parser2 = new InputSnagParser();
                inputSnagParams = Landis.Data.Load<IInputSnagParams>(inputParams.InitSnagFile, parser2);
            }
            MaxLife = 0;
            foreach (ISpecies species in modelCore.Species)
            {
                if (MaxLife < species.Longevity)
                    MaxLife = species.Longevity;
            }
            SiteVars.Initialize(inputParams, inputDisturbanceMatrixParams);
        }

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }
       
        public override void Initialize()
        {
            Timestep = inputParams.Timestep;
            sufficientLight = inputParams.LightClassProbabilities;
            // Initialize climate.  A list of ecoregion indices is passed so that
            // the climate library can operate independently of the LANDIS-II core.
            List<int> ecoregionIndices = new List<int>();
            foreach(IEcoregion ecoregion in ModelCore.Ecoregions)
                ecoregionIndices.Add(ecoregion.Index);
            // Climate.Initialize(inputParams.ClimateFile, false, modelCore);     // LANDIS CLIMATE LIBRARY
            EcoregionData.Initialize(inputParams, inputClimateParams);
            SpeciesData.Initialize(inputParams);
            CalibrateMode = inputParams.CalibrateMode;
            CohortBiomass.SpinupMortalityFraction = inputParams.SpinupMortalityFraction;
            Snags.Initialize(inputSnagParams);
            // Cohorts must be created before the base class is initialized
            // because the base class's reproduction module uses the core's
            // SuccessionCohorts property in its Initialization method.
            Library.UniversalCohorts.Cohorts.Initialize(Timestep, new CohortBiomass());
            Reproduction.SufficientResources = IsSufficientLight;
            Reproduction.Establish = CanEstablish;
            Reproduction.AddNewCohort = AddNewCohort;
            Reproduction.MaturePresent = IsMaturePresent;
            base.Initialize(modelCore, inputParams.SeedAlgorithm); 
            SiteBiomass.Initialize(Timestep);
            Cohort.MortalityEvent += CohortMortality;
            InitializeSites(inputParams.InitialCommunities, inputParams.InitialCommunitiesMap, modelCore);
        }

        public override void Run()
        {
            if (ModelCore.CurrentTime > 0 && SiteVars.CapacityReduction == null)
                SiteVars.CapacityReduction = ModelCore.GetSiteVar<double>("Harvest.CapacityReduction");
            SiteVars.FireSeverity = ModelCore.GetSiteVar<byte>("Fire.Severity");
            EcoregionData.GetAnnualTemperature(Timestep, 0);                            // ForCS CLIMATE
            SpeciesData.GenerateNewANPPandMaxBiomass(Timestep, 0);
            base.RunReproductionFirst();
            // write the maps, if the timestep is right
            if (inputParams.OutputMap > 0)   // 0 = don't print
            {  
                if (ModelCore.CurrentTime % inputParams.OutputMap == 0)
                    Outputs.WriteMaps(inputParams.OutputMapPath, inputParams.OutputMap);
            }
            // Clear list of cohorts to add after growth phase for later
            siteCohortsToAdd.Clear();
        }

        protected override void InitializeSite(ActiveSite site)
        {
            SiteBiomass initSiteBiomass = SiteBiomass.CalcInitSiteBiomass(site, initialCommunity);
            SiteVars.Cohorts[site] = SiteBiomass.Clone(initSiteBiomass.Cohorts);
            // Note: we need this both here and in SiteVars.Initialize()?
            SiteVars.soils[site] = new Soils(initSiteBiomass.soils);
            SiteVars.SoilOrganicMatterC[site] = initSiteBiomass.SoilOrganicMatterC;            
            SiteVars.DeadWoodMass[site].Mass = initSiteBiomass.DeadWoodMass;
            SiteVars.LitterMass[site].Mass = initSiteBiomass.LitterMass;
            SiteVars.DeadWoodDecayRate[site] = initSiteBiomass.DeadWoodDecayRate;
            SiteVars.LitterDecayRate[site] = initSiteBiomass.LitterDecayRate;
            SiteVars.soils[site].BiomassOutput(site, 1);
        }

        public void CohortMortality(object sender,
                                    MortalityEventArgs eventArgs)
        {
            ExtensionType disturbanceType = eventArgs.DisturbanceType;
            ActiveSite site = eventArgs.Site;
            ICohort cohort = eventArgs.Cohort;
            ISpecies species = cohort.Species;
            double foliar = (double)cohort.ComputeNonWoodyBiomass(site);
            double wood = (double)cohort.Data.Biomass - foliar;
            if (eventArgs.Reduction >= 1)
            {
                if (disturbanceType == null)
                {
                    double totalRoot = Roots.CalcRootBiomass(site, species, cohort.Data.Biomass);
                    SiteVars.soils[site].CollectBiomassMortality(species, cohort.Data.Age, wood, foliar, 0);
                    SiteVars.soils[site].CollectBiomassMortality(species, cohort.Data.Age, Roots.CoarseRoot, Roots.FineRoot, 1);
                    if (site.DataIndex == 1)
                        ModelCore.UI.WriteLine("{0} Roots from dying cohort {1}", ModelCore.CurrentTime, Roots.FineRoot);
                }
                if (disturbanceType != null)
                {
                    Disturbed[site] = true;
                    if (disturbanceType.IsMemberOf("disturbance:fire"))
                        Reproduction.CheckForPostFireRegen(eventArgs.Cohort, site);
                    else
                        Reproduction.CheckForResprouting(eventArgs.Cohort, site);
                    SiteVars.soils[site].DisturbanceImpactsBiomass(site, cohort.Species, cohort.Data.Age, wood, foliar, disturbanceType.Name, 0);
                }
            }
            else
            {
                float mortality = eventArgs.Reduction;
                float fractionPartialMortality = mortality / (float)cohort.Data.Biomass;
                double foliarInput = foliar * fractionPartialMortality;
                double woodInput = wood * fractionPartialMortality;
                SiteVars.soils[site].DisturbanceImpactsBiomass(site, cohort.Species, cohort.Data.Age, woodInput, foliarInput, disturbanceType.Name, 0);
                Disturbed[site] = true;
            }
            return;
        }

        /// <summary>
        /// Add a new cohort to a site. Cohort will not be officially 
        /// added to site until after growth phase.
        /// This is a Delegate method to base succession.
        /// </summary>
        public void AddNewCohort(ISpecies species,
                                 ActiveSite site,
                                 string reproductionType,
                                 double propBiomass = 1.0)  // not "fracBiomass" to conform to Landis.Library.Succession.Reproduction.AddNewCohort
        {
            int newBiomass = CohortBiomass.CalcInitCohortBiomass(species, SiteVars.Cohorts[site], site);
            // Cohorts will be officially added after growth phase
            siteCohortsToAdd.Add(new SiteCohortToAdd(site, species, newBiomass));
        }

        protected override void AgeCohorts(ActiveSite site,
                                           ushort years,
                                           int? successionTimestep)
        {
            GrowCohorts(site, years, successionTimestep.HasValue);
        }

        /// <summary>
        /// Grows all cohorts at a site for a specified number of years.  The
        /// dead pools at the site also decompose for the given time period.
        /// </summary>
        public static void GrowCohorts(ActiveSite site,
                                       int years,
                                       bool isSuccessionTimestep)
        {
            IEcoregion ecoregion = ModelCore.Ecoregion[site];
            double preGrowthBiomass = 0;
            SiteVars.ResetAnnualValues(site);
            for (int y = 0; y < years; y++)
            {
                /*  CODE RELATED TO THE USE OF ONE OF THE BIGGER LANDIS CLIMATE LIBRARIES
                // Do not reset annual climate if it has already happend for this year.
                if(!EcoregionData.ClimateUpdates[ecoregion][y + PlugIn.ModelCore.CurrentTime])
                {
                    EcoregionData.SetAnnualClimate(PlugIn.ModelCore.Ecoregion[site], y);
                    EcoregionData.ClimateUpdates[ecoregion][y + PlugIn.ModelCore.CurrentTime] = true;
                }                
                // if spin-up phase, allow each initial community to have a unique climate
                if(PlugIn.ModelCore.CurrentTime == 0)
                {
                    EcoregionData.SetAnnualClimate(PlugIn.ModelCore.Ecoregion[site], y);
                }
                */
                preGrowthBiomass = SiteVars.TotalBiomass[site];
                SiteVars.Cohorts[site].Grow(site, y == years && isSuccessionTimestep, true);
                AddNewCohortsPostGrowth(site);
                SiteVars.soils[site].BiomassOutput(site, 0);
                // update total biomass, before sending this to the soil routines.
                SiteVars.TotalBiomass[site] = Library.UniversalCohorts.Cohorts.ComputeNonYoungBiomass(SiteVars.Cohorts[site]);
                SiteVars.soils[site].ProcessSoils(site, SiteVars.TotalBiomass[site], preGrowthBiomass, 0);
            }
        }

        private static void AddNewCohortsPostGrowth(ActiveSite site)
        {
            var toAdd = siteCohortsToAdd.Where(x => x.site == site).ToList();
            // Add the cohorts
            foreach (var cohort in toAdd)
            {
                int newBiomass = CohortBiomass.CalcInitCohortBiomass(cohort.species, SiteVars.Cohorts[site], site);
                SiteVars.Cohorts[site].AddNewCohort(cohort.species, 1, newBiomass, new System.Dynamic.ExpandoObject());
                SiteVars.soils[site].CollectBiomassMortality(cohort.species, 0, 0, 0, 0);
                double TotalRoots = Roots.CalcRootBiomass(site, cohort.species, newBiomass);
                SiteVars.soils[site].CollectRootBiomass(TotalRoots, 1);
            }
        }

        /// <summary>
        /// Determines if there is sufficient light at a site for a 
        /// species to germinate/resprout. 
        /// 
        /// MG 20250916 moved bulk of method to Light class.  
        /// 
        /// MG 20250916 description edited: the following does not appear 
        /// valid here: 
        /// 
        /// Also accounts for SITE level N limitations.  N 
        /// limits could not be accommodated in the Establishment Probability as 
        /// that is an ecoregion x spp property. Therefore, would better be 
        /// described as "SiteLevelDeterminantReproduction".
        /// 
        /// </summary>
        public bool IsSufficientLight(ISpecies species,
                                      ActiveSite site)
        {
            bool isSufficientLight = Light.IsSufficientLight(species, site, sufficientLight, modelCore);
            return isSufficientLight;
        }

        /// <summary>
        /// Calculate shade class at a site. Name is inherited from
        /// Succession Library, so it's not renamed "CalcShade" as 
        /// I (MG) would normally do.  
        /// 
        /// MG 20250916 moved bulk of method to Light class. 
        /// </summary>
        public override byte ComputeShade(ActiveSite site)
        {
            byte shadeClass = Light.CalcShadeClass(site, modelCore);
            return shadeClass;
        }

        /// <summary>
        /// Determines if a species can establish on a site according
        /// to a random value threshold.
        /// </summary>
        public bool CanEstablish(ISpecies species,
                                 ActiveSite site)
        {
            IEcoregion ecoregion = modelCore.Ecoregion[site];
            double establishProbability = SpeciesData.ProbEstablishment[species][ecoregion];
            return modelCore.GenerateUniform() < establishProbability;
        }

        /// <summary>
        /// Determines if there is a mature cohort at a site.  
        /// </summary>
        public bool IsMaturePresent(ISpecies species,
                                    ActiveSite site)
        {
            return SiteVars.Cohorts[site].IsMaturePresent(species);
        }

        public override void InitializeSites(string initialCommunitiesText,
                                             string initialCommunitiesMap,
                                             ICore modelCore)
        {
            ModelCore.UI.WriteLine("   Loading initial communities from file \"{0}\" ...", initialCommunitiesText);
            DatasetParser parser = new DatasetParser(Timestep, ModelCore.Species, additionalCohortParameters, initialCommunitiesText);
            IDataset communities = Landis.Data.Load<IDataset>(initialCommunitiesText, parser);
            ModelCore.UI.WriteLine("   Reading initial communities map \"{0}\" ...", initialCommunitiesMap);
            IInputRaster<UIntPixel> map;
            map = ModelCore.OpenRaster<UIntPixel>(initialCommunitiesMap);
            using (map)
            {
                UIntPixel pixel = map.BufferPixel;
                foreach (Site site in ModelCore.Landscape.AllSites)
                {
                    map.ReadBufferPixel();
                    uint mapCode = pixel.MapCode.Value;
                    if (!site.IsActive)
                        continue;
                    ActiveSite activeSite = (ActiveSite)site;
                    initialCommunity = communities.Find(mapCode);
                    if (initialCommunity == null)
                        throw new ApplicationException(string.Format("Unknown map code for initial community: {0}", mapCode));
                    InitializeSite(activeSite);
                }
            }
        }

        /// <summary>
        /// NEW DYNAMIC COHORT PARAMETERS GO HERE
        /// </summary>
        public override void AddCohortData()
        {
            return;
        }
    }
}
