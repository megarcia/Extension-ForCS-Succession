// Authors: Caren Dymond, Sarah Beukema

// NOTE: ActiveSite --> Landis.SpatialModeling
// NOTE: ICohort --> Landis.Library.UniversalCohorts
// NOTE: ICore --> Landis.Core
// NOTE: IEcoregion --> Landis.Core
// NOTE: InputValueException --> Landis.Utilities
// NOTE: ISpecies --> Landis.Core
// NOTE: ISpeciesCohorts --> Landis.Library.UniversalCohorts

using System;
using System.Collections.Generic;
using Landis.Core;
using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;
using Landis.Utilities;

namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// Definition of the probability of germination under different light levels for 5 shade classes.
    /// </summary>
    public class Light : ILight
    {
        private byte shadeClass;
        private double probSufficientLight0;
        private double probSufficientLight1;
        private double probSufficientLight2;
        private double probSufficientLight3;
        private double probSufficientLight4;
        private double probSufficientLight5;

        public Light()
        {
        }

        /// <summary>
        /// The shade class (between 1 and 5).
        /// </summary>
        public byte ShadeClass
        {
            get
            {
                return shadeClass;
            }
            set
            {
                if (value > 5 || value < 1)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 1 and 5.");
                shadeClass = value;
            }
        }

        public double ProbSufficientLight0
        {
            get
            {
                return probSufficientLight0;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0 and 1");
                probSufficientLight0 = value;
            }
        }

        public double ProbSufficientLight1
        {
            get
            {
                return probSufficientLight1;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0 and 1");
                probSufficientLight1 = value;
            }
        }

        public double ProbSufficientLight2
        {
            get
            {
                return probSufficientLight2;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0 and 1");
                probSufficientLight2 = value;
            }
        }

        public double ProbSufficientLight3
        {
            get
            {
                return probSufficientLight3;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0 and 1");
                probSufficientLight3 = value;
            }
        }

        public double ProbSufficientLight4
        {
            get
            {
                return probSufficientLight4;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0 and 1");
                probSufficientLight4 = value;
            }
        }

        public double ProbSufficientLight5
        {
            get
            {
                return probSufficientLight5;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0 and 1");
                probSufficientLight5 = value;
            }
        }

        /// <summary>
        /// Determines if there is sufficient light at a site for a 
        /// species to germinate/resprout. 
        /// 
        /// MG 20250916 description edited: the following does not 
        /// appear valid here: 
        /// 
        /// Also accounts for SITE level N limitations.  N 
        /// limits could not be accommodated in the Establishment Probability as 
        /// that is an ecoregion x spp property. Therefore, would better be 
        /// described as "SiteLevelDeterminantReproduction".
        /// </summary>
        public static bool IsSufficientLight(ISpecies species,
                                             ActiveSite site,
                                             List<ILight> sufficientLight,
                                             ICore modelCore)
        {
            byte siteShade = modelCore.GetSiteVar<byte>("Shade")[site];
            double lightProbability = 0.0;
            bool found = false;
            foreach (ILight lights in sufficientLight)
            {
                if (lights.ShadeClass == SpeciesData.ShadeTolerance[species])
                {
                    if (siteShade == 0)
                        lightProbability = lights.ProbSufficientLight0;
                    if (siteShade == 1)
                        lightProbability = lights.ProbSufficientLight1;
                    if (siteShade == 2)
                        lightProbability = lights.ProbSufficientLight2;
                    if (siteShade == 3)
                        lightProbability = lights.ProbSufficientLight3;
                    if (siteShade == 4)
                        lightProbability = lights.ProbSufficientLight4;
                    if (siteShade == 5)
                        lightProbability = lights.ProbSufficientLight5;
                    found = true;
                }
            }
            if (!found)
                modelCore.UI.WriteLine("A Sufficient Light value was not found for {0}.", species.Name);
            return modelCore.GenerateUniform() < lightProbability;
        }

        /// <summary>
        /// Calculate shade class at a site  
        /// </summary>
        public static byte CalcShadeClass(ActiveSite site,
                                          ICore modelCore)
        {
            IEcoregion ecoregion = modelCore.Ecoregion[site];
            double B_MAX = EcoregionData.B_MAX[ecoregion];
            double B_ACT = 0.0;
            if (SiteVars.Cohorts[site] != null)
            {
                foreach (ISpeciesCohorts sppCohorts in SiteVars.Cohorts[site])
                    foreach (ICohort cohort in sppCohorts)
                        if (cohort.Data.Age > 5)
                            B_ACT += cohort.Data.Biomass;
            }
            int lastMortality = SiteVars.PreviousYearMortality[site];
            B_ACT = Math.Min(EcoregionData.B_MAX[ecoregion] - lastMortality, B_ACT);
            // Relative living biomass (ratio of actual to maximum site biomass).
            double B_AM = B_ACT / B_MAX;
            for (byte shadeClass = 5; shadeClass >= 1; shadeClass--)
            {
                if (EcoregionData.ShadeBiomass[shadeClass][ecoregion] <= 0)
                {
                    string mesg = string.Format("Minimum relative biomass has not been defined for ecoregion {0}", ecoregion.Name);
                    throw new ApplicationException(mesg);
                }
                if (B_AM >= EcoregionData.ShadeBiomass[shadeClass][ecoregion])
                    return shadeClass;
            }
            return 0;
        }
    }
}
