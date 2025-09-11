//  Authors:  Caren Dymond, Sarah Beukema

using System;
using System.Diagnostics
using System.IO;

namespace Landis.Extension.Succession.ForC
{
    public class Soils
    {
        public enum SoilPoolType
        {
            VERYFASTAG = 0,
            VERYFASTBG,
            FASTAG,
            FASTBG,
            MEDIUM,
            SLOWAG,
            SLOWBG,
            SSTEMSNAG,
            SOTHERSNAG,
            BLACKCARBON
        };

        public enum SnagType
        {
            STEMSNAGS = 0,
            OTHERSNAG
        };

        public enum ComponentType  // The biomass component type.
        {
            MERCHANTABLE = 0,  // The merchantable biomass component.
            FOLIAGE,  // The foliage biomass component.
            OTHER,  // The other biomass component.
            SUBMERCHANTABLE,  // The submerchantable biomass component.
            COARSEROOT,  // The coarse root biomass component.
            FINEROOT  // The fine root biomass component.
        };  

        /// <summary>
        /// eBiomassPoolIDs - IDs used to index directly into the calculations.
        /// Note that unlike Soils.ComponentType, these are 1-based.
        /// </summary>
        public enum eBiomassPoolIDs
        {
            Merchantable = 1,
            Foliage,
            Other,
            SubMerchantable,
            CoarseRoot,
            FineRoot
        };

        private ActiveSite m_ActiveSite;
        private bool[] DistOccurred = new bool[Constants.NUMDISTURBANCES];      
        private bool[] SpeciesPresent = new bool[PlugIn.ModelCore.Species.Count]; // true if a species is or was ever present on the site
        // Variables collected or used for output purposes.
        private static StreamWriter logPools;
        private static StreamWriter logFlux;
        private static StreamWriter logBioPools;
        private static StreamWriter logFluxSum;
        private static StreamWriter logFluxDist;
        private static StreamWriter logFluxBio;
        private double[,] netCLoss = new double[Constants.NUMBIOMASSCOMPONENTS, PlugIn.ModelCore.Species.Count];
        private double[,] soilC = new double[Constants.NUMSOILPOOLS, PlugIn.ModelCore.Species.Count];      // The carbon in each soil pool attributable to each species.
        private double[] carbonToAir = new double[Constants.NUMSOILPOOLS];
        private double[] carbonToSlowPool = new double[Constants.NUMSOILPOOLS];
        private double[] totalDOMC = new double[Constants.NUMSOILPOOLS];
        private double[,] TotTransfer = new double[2, 3];   // first: 0=no dist,1=dist; second: 0=to DOM, 1=to Air, 3=to FPS
        private double snagToMedium;
        private double branchSnagToFastPool;
        private double oldBiomass;
        private double PreGrowthRootBiomass = 0.0;
        private double RootBiomass = 0.0;
        public int lastAge = 0;
        private bool LastSoilPass = false;
        // For snags: 
        public bool bKillNow = false;
        private double[,] BioSnag = new double[2, Constants.NUMSNAGS];
        public bool binitSnagPresent = false;

        /// <summary>
        /// default constructor
        /// </summary>
        public Soils()
        {
        }

        /// <summary>
        /// main constructor
        /// </summary>
        public Soils(IInputParameters iParams, ActiveSite site, IInputDMParameters iDMParams)
        {
            Debug.Assert(iParams != null);
            SoilVars.iParams = iParams;
            SoilVars.iParamsDM = iDMParams;
            m_ActiveSite = site;
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[m_ActiveSite];
            foreach (ISpecies species in PlugIn.ModelCore.Species)
                for (int idxDOMPool = 0; idxDOMPool < Constants.NUMSOILPOOLS; idxDOMPool++)
                    soilC[idxDOMPool, species.Index] = SoilVars.iParams.DOMPoolAmountT0[ecoregion][species][idxDOMPool];
            InitializeOutput();  // note that this will actually only be done for the first site.
        }

        /// <summary>
        /// Clone constructor
        /// Given all the issues around the ICloneable interface, I have chosen to 
        /// implement a copy constructor.
        /// http://stackoverflow.com/questions/78536/cloning-objects-in-c
        /// </summary>
        /// <param name="oSrc"></param>
        public Soils(Soils oSrc)
        {
            int i;
            for (int j = 0; j < PlugIn.ModelCore.Species.Count; j++)
            {
                for (i = 0; i < Constants.NUMBIOMASSCOMPONENTS; i++)
                    netCLoss[i, j] = oSrc.netCLoss[i, j];
                for (i = 0; i < Constants.NUMSOILPOOLS; i++)
                {
                    soilC[i, j] = oSrc.soilC[i, j];
                    if (j == 0)
                    {
                        carbonToAir[i] = oSrc.carbonToAir[i];
                        carbonToSlowPool[i] = oSrc.carbonToSlowPool[i];
                        totalDOMC[i] = oSrc.totalDOMC[i];
                    }
                    if (j < 3 && i < 2)
                        TotTransfer[i, j] = oSrc.TotTransfer[i, j];
                }
            }
            oldBiomass = oSrc.oldBiomass;
            snagToMedium = oSrc.snagToMedium;
            branchSnagToFastPool = oSrc.branchSnagToFastPool;
            for (i = 0; i < PlugIn.ModelCore.Species.Count; i++)
            {
                if (oSrc.SpeciesPresent[i])
                    SpeciesPresent[i] = true;
            }
        }

        /// <summary>
        /// calculate how much slow pool carbon moves from above ground pool to 
        /// below ground pool                  *
        /// </summary>
        /// <param name="species"></param>
        /// <param name="slowAG_to_slowBG_transferRate"></param>
        public void DoPoolBioMixing(ISpecies species, double slowAG_to_slowBG_transferRate)
        {
            carbonToSlowPool[(int)SoilPoolType.SLOWAG] += soilC[(int)SoilPoolType.SLOWAG, species.Index] * slowAG_to_slowBG_transferRate;
            soilC[(int)SoilPoolType.SLOWBG, species.Index] += soilC[(int)SoilPoolType.SLOWAG, species.Index] * slowAG_to_slowBG_transferRate;
            soilC[(int)SoilPoolType.SLOWAG, species.Index] = soilC[(int)SoilPoolType.SLOWAG, species.Index] * (1 - slowAG_to_slowBG_transferRate);
        }

        /// <summary>
        /// Performs soil dynamics.
        /// Note that this routine is called for each cohort within a site. Thus, it is called seperately
        /// for different species, and we need to remove the species loops from within this routine.
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <param name="species"></param>
        /// <param name="site"></param>
        /// <param name="soilInfo"></param>
        public void DoSoilDynamics(IEcoregion ecoregion, ISpecies species, ActiveSite site)
        {
            // Variable definitions.
            double bFactor = Math.Log(0.5 / 0.15);  // The b factor.
            int currPool;       // A loop counter of the current soil pool.
            double aboveC;      // The amount of carbon added to a soil pool that is attributable to aboveground biomass of an arbitrary species.
            double belowC;      // The amount of carbon added to a soil pool that is attributable to belowground biomass of an arbitrary species.
            double totalC;      // The amount of carbon added to a soil pool that is attributable to an arbitrary species.
            double totalLostC;  // The total amount of carbon lost from a soil pool.
            double lostC;       // The amount of carbon lost from a soil pool that is attributable to an arbitrary species.
            // Separated soil very fast above and below ground pool
            double veryfastAGC;
            double veryfastBGC;
            double lostCA;
            double lostCB;
            double toAir;
            // Variable definitions: (Used exclusively by very fast soil pool dynamics code.)
            const int numberABSlowPool = 2;  // number of above- and below-ground slow soil carbon pool
            const int aboveSlowPool = 0;     // above-ground slow carbon pool 
            const int belowSlowPool = 1;     // below-ground slow soil carbon pool
            double[] carbonToABG_SlowPool = new double[numberABSlowPool]; // to store the slow carbon from different sources
            double totalLostC_AS;
            double totalLostC_BS;
            double[,] snagPools = new double[PlugIn.ModelCore.Species.Count, Constants.NUMSNAGPOOLS];
            branchSnagToFastPool = 0.0;
            snagToMedium = 0.0;
            for (int i = 0; i < PlugIn.ModelCore.Species.Count; i++)
            {
                for (int j = 0; j < Constants.NUMSNAGPOOLS; j++)
                    snagPools[i, j] = 0F;
            }
            // initialize the slow pool array
            for (int p = 0; p < numberABSlowPool; p++)
                carbonToABG_SlowPool[p] = 0F;
            if (species.Index != 1 && ecoregion.Index == 0)
                snagPools[0, 0] = 0F;
            // CBM Comparison notes: 
            // We don't need this routine any more since Landis is already catching 
            // changes in biomass CalculateLossFromBiomassReduction(newTotalBioC, 
            // oldTotalBioC, changeInBioC). Also, the call to calculate decay rates 
            // is outside this routine so that it happens when we know about cohort 
            // biomass.
            //
            // Do the very fast soil pool dynamics
            if (netCLoss[(int)ComponentType.FOLIAGE, species.Index] > 0 || netCLoss[(int)ComponentType.FINEROOT, species.Index] > 0
                || soilC[(int)SoilPoolType.VERYFASTAG, species.Index] > 0 || soilC[(int)SoilPoolType.VERYFASTBG, species.Index] > 0)
            {
                aboveC = netCLoss[(int)ComponentType.FOLIAGE, species.Index];
                belowC = netCLoss[(int)ComponentType.FINEROOT, species.Index];
                totalC = aboveC + belowC;
                veryfastAGC = aboveC + Constants.FINEROOTSABOVERATIO * belowC;
                veryfastBGC = (1 - Constants.FINEROOTSABOVERATIO) * belowC;
                // We determine the new carbon in the very fast soil pool by adding 
                // in the biomass carbon turned over and subtracting out the carbon 
                // lost due to decay attributable to the current species.
                totalLostC = 0F;
                lostCA = 0F;
                lostCB = 0F;
                totalLostC_AS = 0F;
                totalLostC_BS = 0F;
                soilC[(int)SoilPoolType.VERYFASTAG, species.Index] += veryfastAGC;
                soilC[(int)SoilPoolType.VERYFASTBG, species.Index] += veryfastBGC;
                lostCA = soilC[(int)SoilPoolType.VERYFASTAG, species.Index] * SoilVars.decayRates[(int)SoilPoolType.VERYFASTAG, species.Index];
                soilC[(int)SoilPoolType.VERYFASTAG, species.Index] -= lostCA;
                totalLostC_AS += lostCA;
                lostCB = soilC[(int)SoilPoolType.VERYFASTBG, species.Index] * SoilVars.decayRates[(int)SoilPoolType.VERYFASTBG, species.Index];
                soilC[(int)SoilPoolType.VERYFASTBG, species.Index] -= lostCB;
                totalLostC_BS += lostCB;
                // A proportion of the total carbon lost from the very fast pool is
                // released into the atmosphere.  The rest is given to the slow soil
                // pool.
                toAir = totalLostC_AS * SoilVars.iParams.DOMPools[(int)eDOMPoolIDs.VeryFastAG].FracAir;
                carbonToAir[(int)SoilPoolType.VERYFASTAG] += toAir;
                carbonToABG_SlowPool[aboveSlowPool] += totalLostC_AS - toAir;
                carbonToSlowPool[(int)SoilPoolType.VERYFASTAG] += totalLostC_AS - toAir;
                toAir = totalLostC_BS * SoilVars.iParams.DOMPools[(int)eDOMPoolIDs.VeryFastBG].FracAir;
                carbonToAir[(int)SoilPoolType.VERYFASTBG] += toAir;
                carbonToABG_SlowPool[belowSlowPool] += totalLostC_BS - toAir;
                carbonToSlowPool[(int)SoilPoolType.VERYFASTBG] += totalLostC_BS - toAir;
            }
            // Do the fast soil pool dynamics
            if (netCLoss[(int)ComponentType.SUBMERCHANTABLE, species.Index] > 0 || netCLoss[(int)ComponentType.OTHER, species.Index] > 0
                || netCLoss[(int)ComponentType.COARSEROOT, species.Index] > 0
                || soilC[(int)SoilPoolType.FASTAG, species.Index] > 0 || soilC[(int)SoilPoolType.FASTBG, species.Index] > 0
                || soilC[(int)SoilPoolType.SOTHERSNAG, 0] > 0)
            {
                /* Variable definitions:  (Used only by fast soil pool dynamics code.) */
                double fastCAG;
                double fastCBG;
                double fastLoseAG;
                double fastLoseBG;
                // CBM comparison notes:
                // The proportion of coarse roots and branches turned over annually
                // for the current species comes from Landis. Augment the branch 
                // turnover proportion by applying an appropriate multiplier for 
                // the maturity state which the current species is in. This code 
                // was all removed because multipliers are all 1, and not accessable
                // to the user.
                //
                // Branch snags go to fast soil pool.
                branchSnagToFastPool = soilC[(int)SoilPoolType.SOTHERSNAG, species.Index] * SoilVars.iParams.FracDOMBranchSnagToFastAG;
                soilC[(int)SoilPoolType.SOTHERSNAG, species.Index] -= branchSnagToFastPool;
                // Next, the amount of above and belowground carbon added to the 
                // fast soil pool by the species' branches (i.e. submerchantable 
                // and miscellaneous components) and coarse roots turnover is 
                // tabulated. The total carbon added to the pool by the current 
                // species is merely the sum of the above- and below-ground carbon
                // added.
                aboveC = netCLoss[(int)ComponentType.SUBMERCHANTABLE, species.Index] + netCLoss[(int)ComponentType.OTHER, species.Index];
                belowC = netCLoss[(int)ComponentType.COARSEROOT, species.Index];
                totalC = aboveC + belowC;
                snagPools[species.Index, (int)SnagType.OTHERSNAG] = aboveC * (1.0 - SpeciesData.FracNonMerch[species]);
                fastCAG = (aboveC * SpeciesData.FracNonMerch[species]) + Constants.COARSEROOTABOVERATIO * belowC + branchSnagToFastPool;
                fastCBG = (1 - Constants.COARSEROOTABOVERATIO) * belowC;
                // We determine the new carbon in the fast soil pool by adding 
                // in the biomass carbon turned over and subtracting out the 
                // carbon lost due to decay attributable to the current species.
                totalLostC = 0F;
                totalLostC_AS = 0F;
                totalLostC_BS = 0F;
                soilC[(int)SoilPoolType.FASTAG, species.Index] += fastCAG;
                soilC[(int)SoilPoolType.FASTBG, species.Index] += fastCBG;
                fastLoseAG = soilC[(int)SoilPoolType.FASTAG, species.Index] * SoilVars.decayRates[(int)SoilPoolType.FASTAG, species.Index];
                soilC[(int)SoilPoolType.FASTAG, species.Index] -= fastLoseAG;
                totalLostC_AS += fastLoseAG;
                fastLoseBG = soilC[(int)SoilPoolType.FASTBG, species.Index] * SoilVars.decayRates[(int)SoilPoolType.FASTBG, species.Index];
                soilC[(int)SoilPoolType.FASTBG, species.Index] -= fastLoseBG;
                totalLostC_BS += fastLoseBG;
                // A proportion of the total carbon lost from the fast pool is 
                // released into the atmosphere.  The rest is given to the slow 
                // soil pool.
                toAir = totalLostC_AS * SoilVars.iParams.DOMPools[(int)eDOMPoolIDs.FastAG].FracAir;
                carbonToAir[(int)SoilPoolType.FASTAG] += toAir;
                carbonToABG_SlowPool[aboveSlowPool] += totalLostC_AS - toAir;
                carbonToSlowPool[(int)SoilPoolType.FASTAG] += totalLostC_AS - toAir;
                toAir = totalLostC_BS * SoilVars.iParams.DOMPools[(int)eDOMPoolIDs.FastBG].FracAir;
                carbonToAir[(int)SoilPoolType.FASTBG] += toAir;
                carbonToABG_SlowPool[belowSlowPool] += totalLostC_BS - toAir;
                carbonToSlowPool[(int)SoilPoolType.FASTBG] += totalLostC_BS - toAir;
            }
            // withdraw and update the original snag pool size
            double StemSnagLost;
            double BranSnagLost;
            double totalStemSnagLost = 0.0;
            double totalBranSnagLost = 0.0;
            double snagToAir;
            snagToMedium = 0F;
            // do the snag dynamics if there already are snags (soilC) or if 
            // there is input to the snag pools (from netCloss or from input 
            // pools calculated above -Jan2020)
            if (netCLoss[(int)ComponentType.MERCHANTABLE, species.Index] > 0
                || soilC[(int)SoilPoolType.SSTEMSNAG, species.Index] > 0
                || soilC[(int)SoilPoolType.SOTHERSNAG, 0] > 0 
                || snagPools[species.Index, (int)SnagType.STEMSNAGS] > 0
                || snagPools[species.Index, (int)SnagType.OTHERSNAG] > 0
                )
            {
                // calculate how much snag goes to medium soil pool
                snagToMedium = soilC[(int)SoilPoolType.SSTEMSNAG, species.Index] * SoilVars.iParams.FracDOMStemSnagToMedium;
                soilC[(int)SoilPoolType.SSTEMSNAG, species.Index] -= snagToMedium;
                snagPools[species.Index, (int)SnagType.STEMSNAGS] = netCLoss[(int)ComponentType.MERCHANTABLE, species.Index];
                soilC[(int)SoilPoolType.SSTEMSNAG, species.Index] += snagPools[species.Index, (int)SnagType.STEMSNAGS];
                soilC[(int)SoilPoolType.SOTHERSNAG, species.Index] += snagPools[species.Index, (int)SnagType.OTHERSNAG];
                StemSnagLost = soilC[(int)SoilPoolType.SSTEMSNAG, species.Index] * SoilVars.decayRates[(int)SoilPoolType.SSTEMSNAG, species.Index];
                soilC[(int)SoilPoolType.SSTEMSNAG, species.Index] -= StemSnagLost;
                totalStemSnagLost += StemSnagLost;
                BranSnagLost = soilC[(int)SoilPoolType.SOTHERSNAG, species.Index] * SoilVars.decayRates[(int)SoilPoolType.SOTHERSNAG, species.Index];
                soilC[(int)SoilPoolType.SOTHERSNAG, species.Index] -= BranSnagLost;
                totalBranSnagLost += BranSnagLost;
                // collect information into variables for output
                snagToAir = StemSnagLost * SoilVars.iParams.DOMPools[(int)eDOMPoolIDs.SoftStemSnag].FracAir;
                carbonToAir[(int)SoilPoolType.SSTEMSNAG] += snagToAir;
                carbonToABG_SlowPool[aboveSlowPool] += StemSnagLost - snagToAir;
                carbonToSlowPool[(int)SoilPoolType.SSTEMSNAG] += StemSnagLost - snagToAir;
                snagToAir = BranSnagLost * SoilVars.iParams.DOMPools[(int)eDOMPoolIDs.SoftBranchSnag].FracAir;
                carbonToAir[(int)SoilPoolType.SOTHERSNAG] += snagToAir;
                carbonToABG_SlowPool[aboveSlowPool] += BranSnagLost - snagToAir;
                carbonToSlowPool[(int)SoilPoolType.SOTHERSNAG] += BranSnagLost - snagToAir;
            }
            if (snagToMedium > 0 || soilC[(int)SoilPoolType.MEDIUM, species.Index] > 0)
            {
                // Do the medium soil pool dynamics, working individually with each 
                // species. Since only stem turnover (ie. merchantable component 
                // carbon) goes into the medium soil pool, we calculate the total 
                // carbon additions to the pool directly without worrying about 
                // above- and below-ground partitions.
                totalC = snagToMedium;
                // We determine the new carbon in the medium soil pool by adding 
                // in the biomass carbon turned over and subtracting out the carbon 
                // lost due to decay attributable to the current species.
                totalLostC = 0F;
                lostC = 0F;
                soilC[(int)SoilPoolType.MEDIUM, species.Index] += totalC;
                lostC = soilC[(int)SoilPoolType.MEDIUM, species.Index] * SoilVars.decayRates[(int)SoilPoolType.MEDIUM, species.Index];
                soilC[(int)SoilPoolType.MEDIUM, species.Index] -= lostC;
                totalLostC += lostC;
                // A proportion of the total carbon lost from the medium pool is 
                // released into the atmosphere.  The rest is given to the slow 
                // soil pool.
                toAir = totalLostC * SoilVars.iParams.DOMPools[(int)eDOMPoolIDs.Medium].FracAir;
                carbonToAir[(int)SoilPoolType.MEDIUM] += toAir;
                carbonToABG_SlowPool[aboveSlowPool] += totalLostC - toAir;
                carbonToSlowPool[(int)SoilPoolType.MEDIUM] += totalLostC - toAir;
            }
            // do black carbon dynamics (note that there is currently nothing 
            // going into black carbon)
            if (soilC[(int)SoilPoolType.BLACKCARBON, species.Index] > 0)
            {
                double blackC_lost;
                blackC_lost = soilC[(int)SoilPoolType.BLACKCARBON, species.Index] * SoilVars.decayRates[(int)SoilPoolType.BLACKCARBON, species.Index];
                soilC[(int)SoilPoolType.BLACKCARBON, species.Index] -= blackC_lost;
                carbonToAir[(int)SoilPoolType.BLACKCARBON] += blackC_lost * SoilVars.iParams.DOMPools[(int)eDOMPoolIDs.BlackCarbon].FracAir;
                carbonToABG_SlowPool[aboveSlowPool] += blackC_lost - blackC_lost * SoilVars.iParams.DOMPools[(int)eDOMPoolIDs.BlackCarbon].FracAir;
                carbonToSlowPool[(int)SoilPoolType.BLACKCARBON] += blackC_lost - blackC_lost * SoilVars.iParams.DOMPools[(int)eDOMPoolIDs.BlackCarbon].FracAir;
            }
            // Do the slow soil pool dynamics.
            // First we calculate the carbon input to the soil pool 
            // (as "donated" by the very fast, fast, and medium soil pools).
            // We determine the new carbon in the slow soil pool by adding in 
            // the carbon input and subtracting out the carbon lost due to decay.
            totalLostC_AS = 0.0;
            totalLostC_BS = 0.0;
            double lostC_AS;
            double lostC_BS;
            if (soilC[(int)SoilPoolType.SLOWAG, species.Index] > 0 || soilC[(int)SoilPoolType.SLOWBG, species.Index] > 0
                || carbonToABG_SlowPool[aboveSlowPool] > 0 || carbonToABG_SlowPool[belowSlowPool] > 0)
            {
                soilC[(int)SoilPoolType.SLOWAG, species.Index] += carbonToABG_SlowPool[aboveSlowPool];
                lostC_AS = soilC[(int)SoilPoolType.SLOWAG, species.Index] * SoilVars.decayRates[(int)SoilPoolType.SLOWAG, species.Index];
                soilC[(int)SoilPoolType.SLOWAG, species.Index] -= lostC_AS;
                totalLostC_AS += lostC_AS;
                soilC[(int)SoilPoolType.SLOWBG, species.Index] += carbonToABG_SlowPool[belowSlowPool];
                lostC_BS = soilC[(int)SoilPoolType.SLOWBG, species.Index] * SoilVars.decayRates[(int)SoilPoolType.SLOWBG, species.Index];
                soilC[(int)SoilPoolType.SLOWBG, species.Index] -= lostC_BS;
                totalLostC_BS += lostC_BS;
                // Any carbon output from the slow pool automatically goes into the atmosphere.
                carbonToAir[(int)SoilPoolType.SLOWAG] = totalLostC_AS;
                carbonToAir[(int)SoilPoolType.SLOWBG] = totalLostC_BS;
            }
            // Method to do biomixing
            DoPoolBioMixing(species, SoilVars.iParams.FracDOMSlowAGToSlowBG);
            // total the soil pools for output purposes
            for (currPool = 0; currPool < Constants.NUMSOILPOOLS; currPool++)
                totalDOMC[currPool] += soilC[currPool, species.Index];
            return;
        }

        /// <summary>
        /// Collect biomass mortality and turnover from Landis and put into appropriate arrays for Soil Routines 
        /// This now just collects the basic mortality, not disturbance related mortality.
        /// (note: New routine - not part of CBM)                                         
        /// </summary>
        /// <param name="species"></param>
        /// <param name="age"></param>
        /// <param name="mortality_wood"></param>
        /// <param name="mortality_nonwood"></param>
        /// <param name="AboveBelow"></param>
        public void CollectBiomassMortality(ISpecies species, int age, double mortality_wood, double mortality_nonwood, int AboveBelow)
        {
            int idxSpecies = species.Index;
            int idxAge = 0;         // use 0 in non-spinup years to hold the stuff for printing
            if (!SpeciesPresent[idxSpecies])
                SpeciesPresent[idxSpecies] = true;
            if (mortality_wood == 0 && mortality_nonwood == 0)  // this should only happen during initialization
                return;
            double nonwoodC_mort = mortality_nonwood * Constants.BIOTOC;   // turns biomass into C
            double woodC_mort = mortality_wood * Constants.BIOTOC;
            if (PlugIn.ModelCore.CurrentTime == 0)
                idxAge = age;               // set to the age in spin-up years (works as it did before)
            double FracStem = 0;
            if (mortality_wood > 0)
                PropStem = DeadStemToSnagRates(species, age, woodC_mort);
            if (AboveBelow == 0)        // aboveground wood
            {
                netCLoss[(int)ComponentType.FOLIAGE, idxSpecies] += nonwoodC_mort;
                SoilVars.BioInput[(int)ComponentType.FOLIAGE, idxSpecies, idxAge] += nonwoodC_mort;
                // determine the amount of this that is merchantable
                if (mortality_wood > 0)
                {
                    PropStem = DeadStemToSnagRates(species, age, woodC_mort);
                    netCLoss[(int)ComponentType.MERCHANTABLE, idxSpecies] += woodC_mort * PropStem;
                    netCLoss[(int)ComponentType.OTHER, idxSpecies] += woodC_mort * (1 - PropStem);
                    SoilVars.BioInput[(int)ComponentType.MERCHANTABLE, idxSpecies, idxAge] += woodC_mort * PropStem;
                    SoilVars.BioInput[(int)ComponentType.OTHER, idxSpecies, idxAge] += woodC_mort * (1 - PropStem);
                }
            }
            else if (PlugIn.ModelCore.CurrentTime == 0 && AboveBelow == 3)
            {
                SoilVars.BioLive[(int)ComponentType.MERCHANTABLE, idxSpecies] += woodC_mort * PropStem;
                SoilVars.BioLive[(int)ComponentType.OTHER, idxSpecies] += woodC_mort * (1 - PropStem);
            }
            else if (PlugIn.ModelCore.CurrentTime == 0 && AboveBelow == 4)
            {
                SoilVars.BioLive[(int)ComponentType.FINEROOT, idxSpecies] += nonwoodC_mort;
                SoilVars.BioLive[(int)ComponentType.COARSEROOT, idxSpecies] += woodC_mort;
            }
            else if (PlugIn.ModelCore.CurrentTime == 0 && AboveBelow >= 5)
            {
                // we do not want to use the C version, since it gets turned into carbon in the dist impacts routine.
                int idx = AboveBelow - 5;       //idx will tell a later routine which species-age-disturbance group this belongs to.
                BioSnag[0, idx] += mortality_wood;
                BioSnag[1, idx] += mortality_nonwood;
                binitSnagPresent = true;        //flag to say that we have intial snags (so if we don't, we don't need to try to initialize them)
            }
            else
            {
                // belowground wood
                netCLoss[(int)ComponentType.FINEROOT, idxSpecies] += nonwoodC_mort;
                netCLoss[(int)ComponentType.COARSEROOT, idxSpecies] += woodC_mort;
                SoilVars.BioInput[(int)ComponentType.FINEROOT, idxSpecies, idxAge] += nonwoodC_mort;
                SoilVars.BioInput[(int)ComponentType.COARSEROOT, idxSpecies, idxAge] += woodC_mort;
            }
            // totals for the flux summary table
            if (AboveBelow < 3)
                TotTransfer[0, 0] += nonwoodC_mort + woodC_mort;     //0=no dist, 0=to DOM
            return;
        }

        /// <summary>
        /// new routine for disturbance impacts on biomass.
        /// Pass in the wood and non-wood biomass of the stand, and calculate the impact here
        /// </summary>
        /// <param name="site"></param>
        /// <param name="species"></param>
        /// <param name="age"></param>
        /// <param name="wood"></param>
        /// <param name="nonwood"></param>
        /// <param name="DistTypeName"></param>
        /// <param name="tmpFireSeverity"></param>
        public void DisturbanceImpactsBiomass(ActiveSite site, ISpecies species, int age, double wood, double nonwood, string DistTypeName, int tmpFireSeverity)
        {
            int idxSpecies = species.Index;
            int idxDist = DistTypeIndex(DistTypeName);
            DisturbTransferFromPools oDisturbTransferPoolsBiomass;
            DisturbTransferFromPool oDisturbTransfer;
            byte severity = 0;
            string TransferName = "null";
            double FracStem;
            double totroot = Roots.CalcRootBiomass(site, species, wood + nonwood);
            double crsRoot = Roots.CoarseRoot * Constants.BIOTOC;
            double fineRoot = Roots.FineRoot * Constants.BIOTOC;
            double nonwoodC = nonwood * Constants.BIOTOC;   // turns biomass into C
            double woodC = wood * Constants.BIOTOC;
            DistOccurred[idxDist] = true;
            if (!SpeciesPresent[idxSpecies])
                SpeciesPresent[idxSpecies] = true;
            int start = DistTypeName.IndexOf(":") + 1;
            if (start < DistTypeName.Length)
                DistTypeName = DistTypeName.Substring(start);
            if (DistTypeName.Equals(Names.DisturbTypeFire, StringComparison.OrdinalIgnoreCase))
            {
                if (tmpFireSeverity == 0)   // usual case
                {
                    Debug.Assert(SiteVars.FireSeverity != null);
                    severity = SiteVars.FireSeverity[site];
                    if (severity == 0)
                        SiteVars.FireSeverity = PlugIn.ModelCore.GetSiteVar<byte>("Fire.Severity");
                    if (severity == 0)
                        return;     // no impacts from a fire severity = 0
                }
                else
                    severity = (byte)tmpFireSeverity; // called during spin-up
                Debug.Assert((severity >= 0) && (severity <= Constants.FIREINTENSITYCOUNT));
                oDisturbTransferPoolsBiomass = (DisturbTransferFromPools)SoilVars.iParamsDM.DisturbFireFromBiomassPools[severity - 1];
            }
            else
            {
                TransferName = DistTypeName;
                if (DistTypeName.Equals(Names.DisturbTypeHarvest, StringComparison.OrdinalIgnoreCase))
                {
                    SiteVars.HarvestPrescriptionName = PlugIn.ModelCore.GetSiteVar<string>("Harvest.PrescriptionName");
                    TransferName = SiteVars.HarvestPrescriptionName[site];
                    // see if the prescription name is used, if not, reset the transfer to be harvest
                    if (!SoilVars.iParamsDM.DisturbOtherFromBiomassPools.ContainsKey(TransferName))
                        TransferName = DistTypeName;
                }
                if (!SoilVars.iParamsDM.DisturbOtherFromBiomassPools.ContainsKey(TransferName))
                    return;
                oDisturbTransferPoolsBiomass = (DisturbTransferFromPools)SoilVars.iParamsDM.DisturbOtherFromBiomassPools[TransferName];
            }
            // Pools
            // 1. Merchantable 
            // 2. Foliage
            // 3. Other 
            // 4. Sub-Merchantable
            // 5. Coarse Root
            // 6. Fine Root
            if (PlugIn.ModelCore.CurrentTime > 0)
            {
                // Flux Output
                logFluxBio.Write("{0},{1},{2},{3},{4},",
                                 PlugIn.ModelCore.CurrentTime,
                                 site.Location.Row,
                                 site.Location.Column,
                                 PlugIn.ModelCore.Ecoregion[site].MapCode,
                                 species.Name);
                logFluxBio.Write("{0},", idxDist);
            }
            // before we start, determine the amount of this that is merchantable
            PropStem = 0;
            if (woodC > 0)
                PropStem = DeadStemToSnagRates(species, age, woodC);
            double amtC;
            double totToFPS = 0;
            for (int ipool = 0; ipool < 6; ipool++)
            {
                amtC = woodC;
                if (ipool == 1)              // foliage
                    amtC = nonwoodC;
                else if (ipool == 0)         // merchantable
                    amtC = woodC * PropStem;
                else if (ipool == 2)         // other
                    amtC = woodC * (1 - PropStem);
                else if (ipool == 3)         // sub merch - not being used
                {
                    amtC = 0;
                    continue;
                }
                else if (ipool == 4)         // coarse roots
                    amtC = crsRoot;
                else if (ipool == 5)         // fine roots
                    amtC = fineRoot;
                oDisturbTransfer = (DisturbTransferFromPool)oDisturbTransferPoolsBiomass.GetDisturbTransfer(ipool + 1);
                netCLoss[ipool, idxSpecies] += amtC * oDisturbTransfer.FracToDOM;
                totToFPS += amtC * oDisturbTransfer.FracToFPS;
                // totals for the flux summary table
                TotTransfer[1, 0] += amtC * oDisturbTransfer.FracToDOM;     // 0=to DOM, 1=toAir, 2=toFPS
                TotTransfer[1, 1] += amtC * oDisturbTransfer.FracToAir;     // 0=to DOM, 1=toAir, 2=toFPS
                TotTransfer[1, 2] += amtC * oDisturbTransfer.FracToFPS;     // 0=to DOM, 1=toAir, 2=toFPS
                if (PlugIn.ModelCore.CurrentTime > 0)
                {
                    //Flux output
                    logFluxBio.Write("{0:0.000},", amtC * oDisturbTransfer.FracToDOM);
                    logFluxBio.Write("{0:0.000},", amtC * oDisturbTransfer.FracToAir);
                }
            }
            if (PlugIn.ModelCore.CurrentTime > 0)
            {
                //Flux output
                logFluxBio.Write("{0:0.000}", totToFPS);    // toFPS from Bio
                logFluxBio.WriteLine("");
            }
            return;
        }

        /// <summary>
        /// Reduces the different soil layers accordingly
        /// (note: In Landis originally done in FireEffects)                                                    *
        /// </summary>
        /// <param name="site"></param>
        /// <param name="DistTypeName"></param>
        /// <param name="tmpFireSeverity"></param>
        public void DisturbanceImpactsDOM(ActiveSite site, string DistTypeName, int tmpFireSeverity)
        {
            double loss = 0;
            double tofps = 0;
            double tofloor = 0;
            byte severity;
            int idxDist = 0;
            idxDist = DistTypeIndex(DistTypeName);
            string TransferName = "null";
            if (DistOccurred[idxDist])          // check to see if this disturbance has already occurred on this site in this year
                return;                         // if so then don't disturb this site again. 
            DistOccurred[idxDist] = true;
            int start = DistTypeName.IndexOf(":") + 1;
            if (start < DistTypeName.Length)
                DistTypeName = DistTypeName.Substring(start);
            DisturbTransferFromPools oDisturbTransferPoolsDOM;
            DisturbTransferFromPools oDisturbTransferPoolsBiomass;
            if (DistTypeName.Equals(Names.DisturbTypeFire, StringComparison.OrdinalIgnoreCase))
            {
                if (tmpFireSeverity == 0)   //usual case
                {
                    SiteVars.FireSeverity = PlugIn.ModelCore.GetSiteVar<byte>("Fire.Severity");
                    Debug.Assert(SiteVars.FireSeverity != null);
                    severity = SiteVars.FireSeverity[site];
                    if (severity == 0)
                        SiteVars.FireSeverity = PlugIn.ModelCore.GetSiteVar<byte>("Fire.Severity");
                    if (severity == 0)
                        return;     // no impacts from a fire severity = 0
                }
                else
                    severity = (byte)tmpFireSeverity; // called during spin-up
                Debug.Assert((severity >= 0) && (severity <= Constants.FIREINTENSITYCOUNT));
                oDisturbTransferPoolsDOM = (DisturbTransferFromPools)SoilVars.iParamsDM.DisturbFireFromDOMPools[severity - 1];
                oDisturbTransferPoolsBiomass = (DisturbTransferFromPools)SoilVars.iParamsDM.DisturbFireFromBiomassPools[severity - 1];
            }
            else
            {
                TransferName = DistTypeName;
                if (DistTypeName.Equals(Names.DisturbTypeHarvest, StringComparison.OrdinalIgnoreCase))
                {
                    SiteVars.HarvestPrescriptionName = PlugIn.ModelCore.GetSiteVar<string>("Harvest.PrescriptionName");
                    TransferName = SiteVars.HarvestPrescriptionName[site];
                    // see if the prescription name is used, if not, reset the transfer to be harvest
                    if (!SoilVars.iParamsDM.DisturbOtherFromDOMPools.ContainsKey(TransferName))
                        TransferName = DistTypeName;
                }
                // If this key cannot be found, then there are no transfers to perform.
                if (!SoilVars.iParamsDM.DisturbOtherFromDOMPools.ContainsKey(TransferName))
                    return;
                oDisturbTransferPoolsDOM = (DisturbTransferFromPools)SoilVars.iParamsDM.DisturbOtherFromDOMPools[TransferName];
            }
            // Make sure we use the eDOMPoolIDs (1-based) not the 0-based SoilPoolType enum.
            // Now that we have the information, let's actually do the transfers.
            // set up the printing flags
            bool bPrintFlux = false;
            if ((PlugIn.ModelCore.CurrentTime % SoilVars.iParams.OutputFlux == 0) || PlugIn.ModelCore.CurrentTime == 1)
                bPrintFlux = true;
            if (PlugIn.ModelCore.CurrentTime == 0)
                bPrintFlux = false;
            DisturbTransferFromPool oDisturbTransfer;
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                if (SpeciesPresent[(int)species.Index])    // only process if the species has ever been on the site...
                {
                    logFluxDist.Write("{0},{1},{2},{3},{4},",
                        PlugIn.ModelCore.CurrentTime, site.Location.Row, site.Location.Column, PlugIn.ModelCore.Ecoregion[site].MapCode, species.Name);
                    logFluxDist.Write("{0},", idxDist);
                    double FPSsnag = 0;
                    double FPSdom = 0;
                    for (int ipool = 0; ipool < Constants.NUMSOILPOOLS; ipool++)
                    {
                        // Do all reductions to air and fps
                        loss = 0;
                        tofps = 0;
                        tofloor = 0;
                        oDisturbTransfer = (DisturbTransferFromPool)oDisturbTransferPoolsDOM.GetDisturbTransfer(ipool + 1);
                        loss = soilC[ipool, species.Index] * oDisturbTransfer.FracToAir;
                        tofps = soilC[ipool, species.Index] * oDisturbTransfer.FracToFPS;
                        if (ipool == (int)SoilPoolType.SSTEMSNAG || ipool == (int)SoilPoolType.SOTHERSNAG)
                            FPSsnag += tofps;
                        else
                            FPSdom += tofps;
                        logFluxDist.Write("{0:0.000},", loss); // CtoAirFromDist
                        // totals for the flux summary table
                        TotTransfer[1, 1] += loss;     // 0=to DOM, 1=toAir, 2=toFPS
                        TotTransfer[1, 2] += tofps;     // 0=to DOM, 1=toAir, 2=toFPS
                        // All DOM pools should not have anything going to "floor" since they are already there.
                        // So, just do the transfers for the snags into floor
                        // stem snag goes to the ground in medium
                        if (ipool == (int)SoilPoolType.SSTEMSNAG)
                        {
                            tofloor = soilC[ipool, species.Index] * oDisturbTransfer.FracToDOM;
                            soilC[(int)SoilPoolType.MEDIUM, species.Index] += tofloor;
                        }
                        // branch snag goes to the ground in fast
                        else if (ipool == (int)SoilPoolType.SOTHERSNAG)
                        {
                            tofloor = soilC[ipool, species.Index] * oDisturbTransfer.FracToDOM;
                            soilC[(int)SoilPoolType.FASTAG, species.Index] += tofloor;
                        }
                        if (ipool == (int)SoilPoolType.SSTEMSNAG || ipool == (int)SoilPoolType.SOTHERSNAG)
                            logFluxDist.Write("{0:0.000},", tofloor); // litterInput (from disturbance of snags)
                        soilC[ipool, species.Index] -= loss + tofps + tofloor;
                        if (soilC[ipool, species.Index] < 0)
                            soilC[ipool, species.Index] = 0;
                    }
                    logFluxDist.Write("{0:0.000},", FPSsnag); // CtoFPSFromDist SNAG
                    logFluxDist.Write("{0:0.000},", FPSdom); // CtoFPSFromDist DOM
                    logFluxDist.WriteLine("");
                }
            }

        }

        /// <summary>
        /// Calculate the amount of biomass in each cohort and total up for the site.
        /// This will be used for output purposes.
        /// (note: New routine - not part of CBM) 
        /// </summary>
        /// <param name="site"></param>
        /// <param name="Year0"></param>
        public void BiomassOutput(ActiveSite site, int Year0)
        {
            if (PlugIn.ModelCore.CurrentTime <= 0 && Year0 != 1)
               return;
            if ((PlugIn.ModelCore.CurrentTime % SoilVars.iParams.OutputBiomass != 0) && PlugIn.ModelCore.CurrentTime != 1 && Year0 != 1)
                return;
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            // If it the year 0 pass (initialization), then also call the soil output.
            if (Year0 == 1)
            {
                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    if (SpeciesPresent[(int)species.Index])    //Only print species that have once been on the site
                    {
                        for (int currPool = 0; currPool < Constants.NUMSOILPOOLS; currPool++)
                            totalDOMC[currPool] = soilC[currPool, species.Index];
                        SoilOutput(site, species, 1);      //pool and flux output by species
                    }
                }
            }
            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site]) {
                foreach (ICohort cohort in speciesCohorts)
                {
                    double foliar = (double)cohort.ComputeNonWoodyBiomass(site);
                    double wood = (double)cohort.Data.Biomass - foliar;
                    double crsRoot = 0.0;
                    double fineRoot = 0.0;
                    double bbio = Roots.CalcRootBiomass(site, cohort.Species, cohort.Data.Biomass);
                    // now change everything to C
                    foliar *= Constants.BIOTOC;
                    wood *= Constants.BIOTOC;
                    crsRoot = Roots.CoarseRoot * Constants.BIOTOC;
                    fineRoot = Roots.FineRoot * Constants.BIOTOC;
                    logBioPools.Write("{0},{1},{2},", PlugIn.ModelCore.CurrentTime, site.Location.Row, site.Location.Column);
                    logBioPools.Write("{0},{1},{2},", ecoregion.MapCode, cohort.Species.Name, cohort.Data.Age);
                    logBioPools.Write("{0:0.0},", wood);
                    logBioPools.Write("{0:0.0},", foliar);
                    logBioPools.Write("{0:0.0},", crsRoot);
                    logBioPools.Write("{0:0.0} ", fineRoot);
                    logBioPools.WriteLine("");
                }
            }
            return;
        }

        /// <summary>
        /// Description: Controlling routine to loop over all the species in the Model and call the soil dynamics routine
        /// (note: New routine - not part of CBM)
        /// </summary>
        /// <param name="site"></param>
        /// <param name="totalBiomass"></param>
        /// <param name="preGrowthBiomass"></param>
        /// <param name="LastPass"></param>
        public void ProcessSoils(ActiveSite site, double totalBiomass, double preGrowthBiomass, int LastPass)
        {
            // note codes: 
            // LastPass = -1 means soil spin-up phase. 
            // LastPass = 1 means last soil pass. 
            // LastPass = 0 means all other.
            int i;
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            SiteVars.LitterMass[site].Mass = 0.0;
            SiteVars.DeadWoodMass[site].Mass = 0.0;
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                if (!SpeciesPresent[(int)species.Index])    // if the species has never been on the site, we don't want to bother processing it
                {
                    if (PlugIn.ModelCore.CurrentTime == 1 || LastPass == 1)
                        for (i = 0; i < Constants.NUMSOILPOOLS; i++)
                            soilC[i, (int)species.Index] = 0.0;     // zero out the pools for species that have never been present at the beginning of the projection
                    continue;
                }
                else if (PlugIn.ModelCore.CurrentTime == 0 && lastAge == 0)
                    for (i = 0; i < Constants.NUMBIOMASSCOMPONENTS; i++)
                        SoilVars.BioLive[i, (int)species.Index] = 0.0;   // zero out the live biomass component, unless the last year of spin-up
                // Calculate soil carbon pool decay rates as a function of 
                // temperature and weather    
                SoilDecay.CalcDecayRates(ecoregion, species);  // MG 20250911 updated procedure removes site argument
                // Initialize C transferred to air, C transferred to the slow pool.
                for (i = 0; i < Constants.NUMSOILPOOLS; i++)
                {
                    carbonToAir[i] = 0.0;
                    carbonToSlowPool[i] = 0.0;
                    totalDOMC[i] = 0.0;
                }
                // we don't want to do the soil dynamics during the initialization - this needs to be saved
                // for the last soil pass (the one where we capture the snags, if present
                if ((PlugIn.ModelCore.CurrentTime == 0 && (LastSoilPass || LastPass == -1)) || (PlugIn.ModelCore.CurrentTime > 0))
                    DoSoilDynamics(ecoregion, species, site);
                SiteVars.LitterMass[site].Mass += totalDOMC[0] + totalDOMC[1]; //litter = very fast above and below ground
                if (totalDOMC[0] <= 0.01)
                    i = 0;
                for (i = 0; i < Constants.NUMSOILPOOLS; i++)
                {
                    // totals for the flux summary table
                    TotTransfer[0, 1] += carbonToAir[i];     // 0=no dist, 1=toAir
                    if (i >= 2)
                        SiteVars.DeadWoodMass[site].Mass += totalDOMC[i];
                }
                SoilOutput(site, species, 0);      // pool and flux output by species
            }
            if (LastPass == 0)  // Don't call the summary output if it is the last pass - we get the wrong root biomass values (or if it is soil spin-up)
                SummaryOutput(site, totalBiomass, (SiteVars.LitterMass[site].Mass + SiteVars.DeadWoodMass[site].Mass), preGrowthBiomass);
            for (i = 0; i < 5; i++)     // loop over disturbance types
            {
                DistOccurred[i] = false;      // reset, ready for next year. Note that this must be done here, because it is not species specific
                if (i <= 2)                 // moved from SummaryOutput, so that it is always done, even if SummaryOutput is not called.
                {
                    TotTransfer[0, i] = 0.0;
                    TotTransfer[1, i] = 0.0;
                }
            }
            return;
        }

        /// <summary>
        /// Returns proportion of the stem that goes to the snag stem pool.
        /// The remainder will go to snagother.
        /// (note: New routine - not part of CBM)
        /// </summary>
        /// <param name="ispecies"></param>
        /// <param name="age"></param>
        /// <param name="StemBio"></param>
        /// <returns></returns>
        public double DeadStemToSnagRates(ISpecies species, int age, double StemBio)
        {
            double dFracStem = 0.0;
            if (age >= SoilVars.iParams.MerchStemsMinAge[species])
            {
                dFracStem = SoilVars.iParams.MerchCurveParmA[species] * (1 - Math.Pow(SoilVars.iParams.MerchCurveParmB[species], age));
                // Throw an assertion first so that if debugging, this can be examined.
                Debug.Assert((dFracStem >= 0.0) && (dFracStem <= 1.0));
                if (dFracStem < 0.0 || dFracStem > 1.0)
                    throw new ApplicationException("Error: Proportion Stem Biomass to Snag Stem is not between 0 and 1");
            }
            return dFracStem;
        }
        
        /// <summary>
        /// Print the various soil fluxes, for now by year and site
        /// (note: New routine - not part of CBM)
        /// </summary>
        /// <param name="site"></param>
        /// <param name="species"></param>
        /// <param name="Year0"></param>
        private void SoilOutput(ActiveSite site, ISpecies species, int Year0)
        {
            int i;
            bool bPrintDOM;
            bool bPrintFlux;
            //set up the printing flags
            bPrintDOM = false; 
            bPrintFlux = false;
            if ((PlugIn.ModelCore.CurrentTime % SoilVars.iParams.OutputDOMPools == 0) || PlugIn.ModelCore.CurrentTime == 1 || Year0 == 1)
                bPrintDOM = true;
            if ((PlugIn.ModelCore.CurrentTime % SoilVars.iParams.OutputFlux == 0) || PlugIn.ModelCore.CurrentTime == 1)
                bPrintFlux = true;
            if (!bPrintFlux || PlugIn.ModelCore.CurrentTime == 0)    // we aren't printing any fluxes, so make sure to 0 out the applicable arrays
            {
                for (int j = 0; j < 5; j++)
                {
                    DistOccurred[j] = false;      // reset, ready for next year 
                    for (i = 0; i < Constants.NUMBIOMASSCOMPONENTS; i++)
                    {
                        netCLoss[i, species.Index] = 0.0;     
                        SoilVars.BioInput[i, species.Index, 0] = 0.0;
                    }
                }
            }
            if (PlugIn.ModelCore.CurrentTime == 0 && Year0 != 1)
                return;
            if (Year0 == 1)
                bPrintFlux = false;     // we do not want to print the fluxes in the first year, as this is meaningless
            if (!bPrintDOM && !bPrintFlux)
                return;
            if (bPrintDOM)
            {
                logPools.Write("{0},{1},{2},{3},{4},",
                    PlugIn.ModelCore.CurrentTime, site.Location.Row, site.Location.Column, PlugIn.ModelCore.Ecoregion[site].MapCode, species.Name);
            }
            for (int j = 0; j < 1; j++)
            {
                if (j > 0 && !DistOccurred[j])  // We only need to print disturbances when and where they have occurred.
                    continue;
                if (bPrintFlux)
                {
                    logFlux.Write("{0},{1},{2},{3},{4},",
                         PlugIn.ModelCore.CurrentTime, site.Location.Row, site.Location.Column, PlugIn.ModelCore.Ecoregion[site].MapCode, species.Name);
                    logFlux.Write("{0},", j);
                }
                for (i = 0; i < Constants.NUMSOILPOOLS; i++)
                {
                    if (j == 0)
                    {
                        if (bPrintDOM)
                            logPools.Write("{0:0.000},", totalDOMC[i]);
                        if (bPrintFlux)
                        {
                            logFlux.Write("{0:0.000},", carbonToAir[i]);
                            logFlux.Write("{0:0.000},", carbonToSlowPool[i]);
                            if (i == 7)     //SSTEMSNAG 
                                logFlux.Write("{0:0.000},", snagToMedium);
                            else if (i == 8)    //SSTEMBRANCH 
                                logFlux.Write("{0:0.000},", branchSnagToFastPool);
                        }
                    }
                }
                if (bPrintFlux)
                {
                    for (i = 0; i < Constants.NUMBIOMASSCOMPONENTS; i++)
                    {
                        if (i != 3)     // (don't print out the sub-merch, since we aren't using it)
                        {
                            if (j == 0)
                                logFlux.Write("{0:0.000},", SoilVars.BioInput[i, species.Index,0]); //the input that is not from disturbance
                            netCLoss[i, species.Index] = 0.0;
                            SoilVars.BioInput[i, species.Index, 0] = 0.0;
                        }
                    }
                    logFlux.WriteLine("");
                }
            }
            if (bPrintDOM)
                logPools.WriteLine("");
        }

        /// <summary>
        /// Initialize the various soil output files.
        /// (note: New routine - not part of CBM)
        /// </summary>
        /// <exception cref="ApplicationException"></exception>
        private void InitializeOutput()
        {
            Debug.Assert(SoilVars.iParams != null);
            int i;
            if (m_ActiveSite.DataIndex > 1)
                return;
            string logFileName = "log_Pools.csv";
            try
            {
                logPools = Landis.Data.CreateTextFile(logFileName);
            }
            catch (Exception err)
            {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }
            logPools.AutoFlush = true;
            logFileName = "log_Flux.csv";
            try
            {
                logFlux = Landis.Data.CreateTextFile(logFileName);
            }
            catch (Exception err)
            {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }
            logFlux.AutoFlush = true;
            logFileName = "log_FluxDOM.csv";
            try
            {
                logFluxDist = Landis.Data.CreateTextFile(logFileName);
            }
            catch (Exception err)
            {
                string mesg = string.Format("{0}", err.Message);
                throw new ApplicationException(mesg);
            }
            logFluxDist.AutoFlush = true;
            logFileName = "log_FluxBio.csv";
            try
            {
                logFluxBio = Landis.Data.CreateTextFile(logFileName);
            }
            catch (Exception err)
            {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }
            logFluxBio.AutoFlush = true;
            logFileName = "log_BiomassC.csv";
            try
            {
                logBioPools = Landis.Data.CreateTextFile(logFileName);
            }
            catch (Exception err)
            {
                string mesg = string.Format("{0}", err.Message);
                throw new ApplicationException(mesg);
            }
            logBioPools.AutoFlush = true;
            logFileName = "log_Summary.csv";
            try
            {
                logFluxSum = Landis.Data.CreateTextFile(logFileName);
            }
            catch (Exception err)
            {
                string mesg = string.Format("{0}", err.Message);
                throw new ApplicationException(mesg);
            }
            logFluxSum.AutoFlush = true;
            //Now write the headers for each of the output files
            logPools.Write("Time, row, column, ecoregion, species, ");
            logFlux.Write("Time, row, column, ecoregion, species, Dist, ");
            logFluxDist.Write("Time, row, column, ecoregion, species, Dist, ");
            logFluxBio.Write("Time, row, column, ecoregion, species, Dist, ");
            logBioPools.Write("Time, row, column, ecoregion, species, Age, ");
            logBioPools.Write("Wood, Leaf, CrsRoot, FineRoot");
            logFluxSum.Write("Time,row,column,ecoregion,ABio,BBio,TotalDOM,DelBio,Turnover,NetGrowth,NPP,Rh,NEP,NBP,ToFPS");
            string[] colSoil = new string[] { "VF_A", "VF_B", "Fast_A", "Fast_B", "MED", "Slow_A", "Slow_B", "Sng_Stem", "Sng_Oth", "Extra" };
            string[] colLitter = new string[] { "MERCH", "FOL", "OtherWoody", "CrsRt", "FRt" };
            for (i = 0; i < Constants.NUMSOILPOOLS; i++)
            {
                logFlux.Write("{0}_toAir, ", colSoil[i]);
                logFlux.Write("{0}_toSlow, ", colSoil[i]);
                if (i == 7)     //SSTEMSNAG
                    logFlux.Write("SngStemToMed, ");
                else if (i == 8)    //SOTHERSNAG
                    logFlux.Write("SngOthToFast, ");
                logFluxDist.Write("{0}_toAir, ", colSoil[i]);
                if (i == 7)     //SSTEMSNAG
                    logFluxDist.Write("SngStemToMed, ");
                else if (i == 8)    //SOTHERSNAG
                    logFluxDist.Write("SngOthToFast, ");
                logPools.Write("{0},", colSoil[i]);
            }
            logFluxDist.Write("SnagsToFPS, DOMtoFPS");
            for (i = 0; i < Constants.NUMBIOMASSCOMPONENTS - 1; i++)
            {
                logFlux.Write("{0}_ToDOM, ", colLitter[i]);
                logFluxBio.Write("{0}_ToDOM, ", colLitter[i]);
                logFluxBio.Write("{0}_ToAir, ", colLitter[i]);
            }
            logFluxBio.Write("BioToFPS");
            logPools.WriteLine("");
            logFlux.WriteLine("");
            logBioPools.WriteLine("");
            logFluxSum.WriteLine("");
            logFluxDist.WriteLine("");
            logFluxBio.WriteLine("");
        }

        /// <summary>
        /// Print the summary fluxes (and later the summary pools)
        /// (note: New routine - not part of CBM)
        /// </summary>
        /// <param name="site"></param>
        /// <param name="TotBiomass"></param>
        /// <param name="totalSoil"></param>
        /// <param name="preGrowthBiomass"></param>
        private void SummaryOutput(ActiveSite site, double TotBiomass, double totalSoil, double preGrowthBiomass)
        {
            int i;
            double NBP;
            double NEP;
            double totNPP;
            double delBio;
            double abio;
            double bbio;
            double NetGrowth;
            bool bPrint;
            double totalBiomass = TotBiomass;
            //add in the belowground biomass to the total biomass value
            bbio = RootBiomass * Constants.BIOTOC;
            totalBiomass *= Constants.BIOTOC;         //change to C
            abio = totalBiomass;            //total biomass is currently only aboveground
            totalBiomass += bbio;           //add in the belowground
            //preGrowthBiomass is aboveground only, so calculate additional root component
            preGrowthBiomass += PreGrowthRootBiomass;
            preGrowthBiomass *= Constants.BIOTOC;     //change to C
            //update the site variables (used for printing maps)
            //so also move the calculations here, where needed
            //allBiomass = totalBiomass + TotTransfer[1, 0] + TotTransfer[1, 1] + TotTransfer[1, 2];        //add in any biomass that was removed this year from disturbance
            delBio = totalBiomass - oldBiomass;
            NetGrowth = totalBiomass - preGrowthBiomass;         //change in biomass from growth only
            totNPP = NetGrowth + TotTransfer[0, 0];                //change in biomass + input to DOM not from disturances
            if (totNPP < 0)
                totNPP = 0.0;
            NEP = totNPP - TotTransfer[0, 1];                   //NPP - to air from decomp (Rh)
            NBP = NEP - TotTransfer[1, 2] - TotTransfer[1, 1];   //NEP - toFPS (from disturbances) - toAir from disturbances
            SiteVars.NPP[site] = totNPP;
            SiteVars.RH[site] = TotTransfer[0, 1];
            SiteVars.NEP[site] = NEP;
            SiteVars.NBP[site] = NBP;
            SiteVars.TotBiomassC[site] = totalBiomass;
            SiteVars.SoilOrganicMatterC[site] = totalSoil;
            SiteVars.ToFPSC[site] = TotTransfer[1, 2];
            //set up the printing flags
            bPrint = false;
            if ((PlugIn.ModelCore.CurrentTime % SoilVars.iParams.OutputSummary == 0) || PlugIn.ModelCore.CurrentTime == 1)
                bPrint = true;
            if (!bPrint || PlugIn.ModelCore.CurrentTime == 0)    //we aren't printing the summary, so make sure to 0 out the applicable arrays
            {
                for (i = 0; i <= 2; i++)
                {
                    TotTransfer[0, i] = 0.0;
                    TotTransfer[1, i] = 0.0;
                }
                oldBiomass = totalBiomass;
                PreGrowthRootBiomass = 0.0;
                RootBiomass = 0.0;
            }
            if (PlugIn.ModelCore.CurrentTime == 0)
                return;
            if (!bPrint)
                return;
            if (bPrint)
            {
                logFluxSum.Write("{0},{1},{2},{3},",
                    PlugIn.ModelCore.CurrentTime, site.Location.Row, site.Location.Column, PlugIn.ModelCore.Ecoregion[site].Name);
                logFluxSum.Write("{0:0.0},", abio);
                logFluxSum.Write("{0:0.0},", bbio);
                logFluxSum.Write("{0:0.0},", totalSoil);
                logFluxSum.Write("{0:0.0},", delBio);
                logFluxSum.Write("{0:0.0},", TotTransfer[0, 0]);    //input to DOM not from disturbance
                logFluxSum.Write("{0:0.0},", NetGrowth);
                logFluxSum.Write("{0:0.0},", totNPP);             
                logFluxSum.Write("{0:0.0},", TotTransfer[0, 1]);    //sum of emissions to air with no disturbance = Rh
                logFluxSum.Write("{0:0.0},", NEP);                  
                logFluxSum.Write("{0:0.0},", NBP);
                logFluxSum.Write("{0:0.0}", TotTransfer[1, 2]);     //To FPS
                logFluxSum.WriteLine("");
                for (i = 0; i <= 2; i++)
                {
                    TotTransfer[0, i] = 0.0;
                    TotTransfer[1, i] = 0.0;
                }
            }
            oldBiomass = totalBiomass;
            PreGrowthRootBiomass = 0.0;
            RootBiomass = 0.0;
        }

        /// <summary>
        /// Pass in the disturbance type and return the related index 
        /// </summary>
        /// <param name="DistTypeName"></param>
        /// <returns></returns>
        public int DistTypeIndex(string DistTypeName)
        {
            int i = 0;
            for (i = SoilVars.DistType.GetUpperBound(0); i > 0; i--)
                if (DistTypeName.Contains(SoilVars.DistType[i]))
                    break;
            return i;
        }

        /// <summary>
        /// Collects the pregrowth root biomass for storage and printing to summary output file 
        /// </summary>
        /// <param name="AllRoots"></param>
        /// <param name="PrePostGrowth"></param>
        public void CollectRootBiomass(double AllRoots, int PrePostGrowth)
        {
            if (PrePostGrowth == 0)
                PreGrowthRootBiomass += AllRoots;
            else
                RootBiomass += AllRoots;
        }

        /// <summary>
        /// Simulate input and decay to soil pools until the slow pools stabilize.
        /// </summary>
        /// <param name="site"></param>
        public void SpinupSoils(ActiveSite site)
        {
            int idxSpecies;
            int icnt = 0;
            double wood;
            double nonwood;
            double initSlowPool = 0.0;
            double newSlowPool = 0.0;
            double diff = 0.0;
            double frac = 0.0;
            int maxage = lastAge;
            if (SoilVars.iParams.SoilSpinUpFlag == 0)
                return;
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                idxSpecies = (int)species.Index;
                initSlowPool += soilC[5, idxSpecies] + soilC[6, idxSpecies];
            }
            do
            {
                for (int iage = 0; iage <= maxage; iage++)
                {
                    foreach (ISpecies species in PlugIn.ModelCore.Species)
                    {
                        idxSpecies = (int)species.Index;
                        if (SpeciesPresent[idxSpecies])
                        {
                            wood = 0.0;
                            nonwood = 0.0;
                            for (int i = 0; i < Constants.NUMBIOMASSCOMPONENTS; i++)
                            {
                                netCLoss[i, idxSpecies] = SoilVars.BioInput[i, idxSpecies, iage];
                                if (i == 0)
                                    wood += SoilVars.BioLive[i, idxSpecies];
                                else
                                    nonwood += SoilVars.BioLive[i, idxSpecies];
                            }
                            // simulate the input from a stand-replacing event.
                            // NOTE: we are assuming a severity 4 fire.
                            if (iage == maxage)
                                DisturbanceImpactsBiomass(site, species, iage, wood / Constants.BIOTOC, nonwood / Constants.BIOTOC, ":fire", 4);
                        }
                    }
                    if (iage == maxage)
                        DisturbanceImpactsDOM(site, ":fire", 4);

                    ProcessSoils(site, Library.UniversalCohorts.Cohorts.ComputeNonYoungBiomass(SiteVars.Cohorts[site]), 0, -1);
                }
                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    idxSpecies = (int)species.Index;
                    newSlowPool += soilC[5, idxSpecies] + soilC[6, idxSpecies];
                }
                diff = newSlowPool - initSlowPool;
                prop = 100 * diff / initSlowPool;
                initSlowPool = newSlowPool;
                newSlowPool = 0.0;
                icnt++;
                PlugIn.ModelCore.UI.WriteLine("Spinup: cycles={0}, newSlowPool={1}, diff={2}, prop={3}", icnt, initSlowPool, diff, prop);
            }
            while (icnt < SoilVars.iParams.SpinUpIterations && (prop > SoilVars.iParams.SpinUpTolerance || prop < -SoilVars.iParams.SpinUpTolerance));
            return;
        }

         /// <summary>
         /// This is the last initialization soil pass. It is always done, even if the soil spin up is not being run.
         /// Its main purpose is to get the initial snags created and decayed for the right amount of time.
         /// Note that because of this pass, the first year that the soils are printed, they will be different from the entered initial conditions.
         /// </summary>
         /// <param name="site"></param>
        public void LastInitialSoilPass(ActiveSite site)
        {
            int idxSpecies;
            int maxage = lastAge;
            string sDist;
            int distIdx;
            LastSoilPass = true;
            for (int iage = 0; iage <= maxage; iage++)
            {
                distIdx = -999;
                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    idxSpecies = (int)species.Index;
                    if (SpeciesPresent[idxSpecies])
                    {
                        for (int i = 0; i < Constants.NUMBIOMASSCOMPONENTS; i++)
                        {
                            netCLoss[i, idxSpecies] = SoilVars.BioInput[i, idxSpecies, iage];
                            SoilVars.BioInput[i, idxSpecies, iage] = 0.0;
                        }
                        // Add in the biomass for the snags. This needs to be added 
                        // in the number of years before the start that the snag has
                        // been around.
                        if (binitSnagPresent)
                        {
                            for (int jdx = 0; jdx < Constants.NUMSNAGS; jdx++)
                            {
                                if (Snags.bSnagsUsed[jdx])
                                {
                                    if (iage == maxage - Snags.initSnagAge[jdx] && idxSpecies == Snags.initSpecIdx[jdx])
                                    {
                                        //add in the biomass for the snags - note that we have to pass it the age at death, not the current age
                                        sDist = ":" + Snags.initSnagDist[jdx];
                                        DisturbanceImpactsBiomass(site, species, Snags.DiedAt[jdx], BioSnag[0, jdx], BioSnag[1, jdx], sDist, 0);
                                        distIdx = jdx;
                                    }
                                    if (Snags.DiedAt[jdx] == 0 || Snags.DiedAt[jdx] > maxage) break;
                                }
                            }
                        }
                    }
                }
                // the call to disturbing the DOM needs to be outside the species loop
                if (binitSnagPresent && distIdx >= 0)
                {
                    // only disturb once in a year... this could be a problem if 
                    // there are more than one species disturbed with different 
                    // disturbance types.
                    sDist = ":" + Snags.initSnagDist[distIdx];
                    DisturbanceImpactsDOM(site, sDist, 0);
                }

                ProcessSoils(site, SiteVars.TotalBiomass[site], 0, 1);
            }
            BioSnag = null;
            LastSoilPass = false;
            for (int jdx = 0; jdx < Constants.NUMSNAGS; jdx++)
                Snags.bSnagsUsed[jdx] = false;
            return;
        }
    }
}
