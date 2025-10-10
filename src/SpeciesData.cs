// Authors: Caren Dymond, Sarah Beukema

// NOTE: IEcoregion --> Landis.Core
// NOTE: ISpecies --> Landis.Core

using System.IO;
using Landis.Core;
using Landis.Library.Parameters;

namespace Landis.Extension.Succession.ForC
{
    public class SpeciesData
    {
        public static Library.Parameters.Species.AuxParm<int> FuncType;
        public static Library.Parameters.Species.AuxParm<double> LeafLongevity;
        public static Library.Parameters.Species.AuxParm<bool> Epicormic;
        public static Library.Parameters.Species.AuxParm<double> MortCurveShape;
        public static Library.Parameters.Species.AuxParm<int> MerchStemsMinAge;
        public static Library.Parameters.Species.AuxParm<double> MerchCurveParmA;
        public static Library.Parameters.Species.AuxParm<double> MerchCurveParmB;
        public static Library.Parameters.Species.AuxParm<double> FracNonMerch;
        public static Library.Parameters.Species.AuxParm<double> GrowthCurveShapeParm;
        public static Library.Parameters.Species.AuxParm<byte> FireTolerance;
        public static Library.Parameters.Species.AuxParm<byte> ShadeTolerance;

        public static Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<double>> ProbEstablishment;
        public static Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<double>> ANPP_MAX_Spp;
        public static Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<int>> B_MAX_Spp;

        //  Establishment probability modifier for each species in each ecoregion (from biomass succession)
        public static Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<double>> ProbEstablishModifier;

        // Root parameters
        public static Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<double[]>> MinWoodyBio;
        public static Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<double[]>> Ratio;
        public static Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<double[]>> FracFine;
        public static Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<double[]>> FineTurnover;
        public static Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<double[]>> CoarseTurnover;
        private static IInputParams m_iParams;
        private static bool bWroteMsg1;
        

        //---------------------------------------------------------------------
        public static void Initialize(IInputParams parameters)
        {
            FuncType = parameters.SppFunctionalType;
            LeafLongevity = parameters.LeafLongevity;
            Epicormic = parameters.Epicormic;
            MortCurveShape = parameters.MortCurveShape;
            MerchStemsMinAge = parameters.MerchStemsMinAge;
            MerchCurveParmA = parameters.MerchCurveParmA;
            MerchCurveParmB = parameters.MerchCurveParmB;
            FracNonMerch = parameters.FracNonMerch;
            GrowthCurveShapeParm = parameters.GrowthCurveShapeParm;
            FireTolerance = parameters.FireTolerance;
            ShadeTolerance = parameters.ShadeTolerance;

            // Roots
            Ratio = Util.CreateSpeciesEcoregionArrayParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions, 5);
            MinWoodyBio = Util.CreateSpeciesEcoregionArrayParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions, 5);
            FracFine = Util.CreateSpeciesEcoregionArrayParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions, 5);
            FineTurnover = Util.CreateSpeciesEcoregionArrayParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions, 5);
            CoarseTurnover = Util.CreateSpeciesEcoregionArrayParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions, 5);

            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (EcoregionData.ActiveSiteCount[ecoregion] == 0)
                    continue;
                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Ratio[species][ecoregion][i] = parameters.Ratio[ecoregion][species][i];
                        MinWoodyBio[species][ecoregion][i] = parameters.MinWoodyBio[ecoregion][species][i];
                        FracFine[species][ecoregion][i] = parameters.FracFine[ecoregion][species][i];
                        FineTurnover[species][ecoregion][i] = parameters.FineTurnover[ecoregion][species][i];
                        CoarseTurnover[species][ecoregion][i] = parameters.CoarseTurnover[ecoregion][species][i];
                    }
                }
            }
            m_iParams = parameters;
            bWroteMsg1 = false;
            // The initial ANPP and max biomass:
            GenerateNewANPPandMaxBiomass(parameters.Timestep, 0);
        }

        public static void GenerateNewANPPandMaxBiomass(int years, int spinupyear)
        {
            ANPP_MAX_Spp = Util.CreateSpeciesEcoregionParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);
            B_MAX_Spp = Util.CreateSpeciesEcoregionParm<int>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);
            ProbEstablishment = Util.CreateSpeciesEcoregionParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);
            ProbEstablishModifier = Util.CreateSpeciesEcoregionParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);
            // double MeanAnnualTemperature = 0.0;
            int usetime = PlugIn.ModelCore.CurrentTime;
            if (spinupyear < 0)
                usetime = spinupyear;
            for (int y = 0; y < years; ++y) 
            {
                foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions) 
                {
                    if (EcoregionData.ActiveSiteCount[ecoregion] == 0)
                        continue;
                     /* CODE RELATED TO THE USE OF ONE OF THE BIGGER LANDIS CLIMATE LIBRARIES
                    AnnualClimate_Monthly ecoClimate = EcoregionData.AnnualWeather[ecoregion]; //Climate Library v2
                    //AnnualClimate ecoClimate = EcoregionData.AnnualClimateArray[ecoregion][y];//Climate Library on GitHub
                    if(ecoClimate == null)
                        throw new System.ApplicationException("Error: CLIMATE NULL.");
                    if (usetime >= 0)
                        MeanAnnualTemperature = (double)ecoClimate.CalculateMeanAnnualTemp(usetime); //Climate Library v2
                        //MeanAnnualTemperature = (double)ecoClimate.MeanAnnualTemp(usetime);  //Climate Library on GitHub
                    else
                        MeanAnnualTemperature = (double)ecoClimate.CalculateMeanAnnualTemp(0); //Climate Library v2
                        //MeanAnnualTemperature = (double)ecoClimate.MeanAnnualTemp(0);  //Climate Library on GitHub
                    */
                    foreach(ISpecies species in PlugIn.ModelCore.Species)
                    {
                        IANPP anpp;
                        ProbEstablishModifier[species][ecoregion] = 1.0;
                        if (m_iParams.ANPPTimeCollection[ecoregion][species].TryGetValue(usetime + y, out anpp))
                        {
                            PlugIn.ModelCore.NormalDistribution.Mu = anpp.GramsPerMetre2Year;
                            PlugIn.ModelCore.NormalDistribution.Sigma = anpp.StdDev;
                            double sppANPP = PlugIn.ModelCore.NormalDistribution.NextDouble();
                            sppANPP = PlugIn.ModelCore.NormalDistribution.NextDouble();
                            ANPP_MAX_Spp[species][ecoregion] += sppANPP;
                        }
                        else if (usetime < 0)
                        {
                            // user didn't enter anything for all the spin-up time, and we must use the year 0 time.
                            if (m_iParams.ANPPTimeCollection[ecoregion][species].TryGetValue(0, out anpp))
                            {
                                if (!bWroteMsg1)
                                {
                                    PlugIn.ModelCore.UI.WriteLine("ANPP values were not entered for the earliest spin-up years. Year 0 values will be used.");
                                    bWroteMsg1 = true;
                                }
                                PlugIn.ModelCore.NormalDistribution.Mu = anpp.GramsPerMetre2Year;
                                PlugIn.ModelCore.NormalDistribution.Sigma = anpp.StdDev;
                                double sppANPP = PlugIn.ModelCore.NormalDistribution.NextDouble();
                                sppANPP = PlugIn.ModelCore.NormalDistribution.NextDouble();
                                ANPP_MAX_Spp[species][ecoregion] += sppANPP;
                            }
                        }
                        IMaxBiomass maxbio;
                        if (m_iParams.MaxBiomassTimeCollection[ecoregion][species].TryGetValue(usetime + y, out maxbio))
                            B_MAX_Spp[species][ecoregion] = (int)maxbio.MaxBio;
                        else if (usetime < 0)
                        {
                            // user didn't enter anything for all the spin-up time, and we must use the year 0 time.                            if (!bWroteMsg2)
                            PlugIn.ModelCore.UI.WriteLine("MaxBiomass values were not entered for the earliest spin-up years. Year 0 values will be used.");
                            if (m_iParams.MaxBiomassTimeCollection[ecoregion][species].TryGetValue(0, out maxbio))
                                B_MAX_Spp[species][ecoregion] = (int)maxbio.MaxBio;
                        }
                        // Establishmenet is only used in projection, not spin-up. 
                        // Don't check for values when usetime < 0
                        IProbEstablishment probEstablishment;
                        if (usetime >= 0)
                        {
                            if (m_iParams.ProbEstablishmentTimeCollection[ecoregion][species].TryGetValue(usetime + y, out probEstablishment))
                                ProbEstablishment[species][ecoregion] = (double)probEstablishment.Establishment;
                        }
                    }
                }
            }            
            EcoregionData.UpdateB_MAX();
            return;           
        }
    }
}
