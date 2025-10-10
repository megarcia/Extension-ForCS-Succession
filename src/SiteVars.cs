// Authors: Robert M. Scheller, James B. Domingo

// NOTE: ActiveSite --> Landis.SpatialModeling
// NOTE: ISiteVar --> Landis.SpatialModeling
// NOTE: Pool --> Landis.Library.UniversalCohorts
// NOTE: Process --> System.Diagnostics
// NOTE: SiteCohorts --> Landis.Library.UniversalCohorts

using System.Diagnostics;
using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// The pools of dead biomass for the landscape's sites.
    /// </summary>
    public static class SiteVars
    {
        private static ISiteVar<SiteCohorts> universalCohorts;
        private static ISiteVar<int> timeOfLast; // Time of last succession simulation:
        private static ISiteVar<int> previousYearMortality;
        private static ISiteVar<int> currentYearMortality;
        private static ISiteVar<int> totalBiomass;

        // Soil and litter variables:
        public static ISiteVar<Pool> DeadWoodMass;
        public static ISiteVar<Pool> LitterMass;
        public static ISiteVar<double> SoilOrganicMatterC;
        public static ISiteVar<double> DeadWoodDecayRate;
        public static ISiteVar<double> LitterDecayRate;
        public static ISiteVar<double> AbovegroundNPPcarbon;

        // Disturbance variables
        public static ISiteVar<byte> fireSeverity;
        public static ISiteVar<string> HarvestPrescriptionName;

        public static ISiteVar<Soils> soils;
        public static ISiteVar<double> capacityReduction;

        // Site-level values for printing maps
        public static ISiteVar<double> NPP;
        public static ISiteVar<double> RH;
        public static ISiteVar<double> NEP;
        public static ISiteVar<double> NBP;
        public static ISiteVar<double> TotBiomassC;
        public static ISiteVar<double> ToFPSC;

        /// <summary>
        /// Initializes the module.
        /// </summary>
        public static void Initialize(IInputParams iParams, IInputDisturbanceMatrixParams iDMParams)
        {
            System.Diagnostics.Debug.Assert(iParams != null);
            universalCohorts = PlugIn.ModelCore.Landscape.NewSiteVar<SiteCohorts>();
            timeOfLast = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            previousYearMortality = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            currentYearMortality = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            totalBiomass = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            DeadWoodMass = PlugIn.ModelCore.Landscape.NewSiteVar<Pool>();
            LitterMass = PlugIn.ModelCore.Landscape.NewSiteVar<Pool>();
            SoilOrganicMatterC = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            DeadWoodDecayRate = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            LitterDecayRate = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            AbovegroundNPPcarbon = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            NPP = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            RH = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            NEP = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            NBP = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            TotBiomassC = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            ToFPSC = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            fireSeverity = PlugIn.ModelCore.GetSiteVar<byte>("Fire.Severity");
            HarvestPrescriptionName = PlugIn.ModelCore.GetSiteVar<string>("Harvest.PrescriptionName");
            soils = PlugIn.ModelCore.Landscape.NewSiteVar<Soils>();
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                //  site cohorts are initialized by the PlugIn.InitializeSite method
                DeadWoodMass[site] = new Pool();
                LitterMass[site] = new Pool();
            }
            PlugIn.ModelCore.RegisterSiteVar(universalCohorts, "Succession.UniversalCohorts");
            PlugIn.ModelCore.RegisterSiteVar(DeadWoodMass, "Succession.WoodyDebris");
            PlugIn.ModelCore.RegisterSiteVar(LitterMass, "Succession.Litter");
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                // Note: we need this both here and in Plug-in
                Process currentProcess = Process.GetCurrentProcess();  //temporary - for memory testing SEPT
                double totalMBOfPhysicalMemory = currentProcess.WorkingSet64 / 100000.0;
                double totalMBOfVirtualMemory = currentProcess.VirtualMemorySize64 / 100000.0;
                soils[site] = new Soils(iParams, site, iDMParams);
                totalMBOfPhysicalMemory = currentProcess.WorkingSet64 / 100000.0;
                totalMBOfVirtualMemory = currentProcess.VirtualMemorySize64 / 100000.0;
            }
        }

        public static void ResetAnnualValues(Site site)
        {
            AbovegroundNPPcarbon[site] = 0.0;
            TotalBiomass[site] = 0;
            TotalBiomass[site] = Library.UniversalCohorts.Cohorts.ComputeNonYoungBiomass(SiteVars.Cohorts[site]);
            PreviousYearMortality[site] = CurrentYearMortality[site];
            CurrentYearMortality[site] = 0;
        }

        /// <summary>
        /// Biomass cohorts at each site.
        /// </summary>
        public static ISiteVar<SiteCohorts> Cohorts
        {
            get
            {
                return universalCohorts;
            }
            set
            {
                universalCohorts = value;
            }
        }

        public static ISiteVar<int> TimeOfLast
        {
            get
            {
                return timeOfLast;
            }
        }

        public static ISiteVar<byte> FireSeverity
        {
            get
            {
                return fireSeverity;
            }
            set
            {
                fireSeverity = value;
            }

        }

        public static ISiteVar<double> CapacityReduction
        {
            get
            {
                return capacityReduction;
            }
            set
            {
                capacityReduction = value;
            }
        }

        public static ISiteVar<int> TotalBiomass
        {
            get
            {
                return totalBiomass;
            }
        }

        /// <summary>
        /// Previous Year Site Mortality.
        /// </summary>
        public static ISiteVar<int> PreviousYearMortality
        {
            get
            {
                return previousYearMortality;
            }
        }

        /// <summary>
        /// Current Year Site Mortality.
        /// </summary>
        public static ISiteVar<int> CurrentYearMortality
        {
            get
            {
                return currentYearMortality;
            }
        }
    }
}
