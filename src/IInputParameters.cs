//  Authors:  Caren Dymond, Sarah Beukema

using System.Collections.Generic;

namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// The parameters for biomass succession.
    /// </summary>
    public interface IInputParameters
    {
        int Timestep { get; set; }
        SeedingAlgorithms SeedAlgorithm { get; set; }
        string InitialCommunities { get; set; }
        string InitialCommunitiesMap { get; set; }
        bool CalibrateMode { get; set; }
        double SpinupMortalityFraction { get; set; }
        string ClimateFile { get; set; }
        string ClimateFile2 { get; set; }
        string InitSnagFile { get; set; }
        string DMFile { get; set; }
        int OutputBiomass { get; }
        int OutputDOMPools { get; }
        int OutputFlux { get; }
        int OutputSummary { get; }
        int OutputMap { get; }
        string OutputMapPath { get; }
        int OutputBiomassC { get; }
        int OutputSDOMC { get; }
        int OutputNBP { get; }
        int OutputNEP { get; }
        int OutputNPP { get; }
        int OutputRH { get; }
        int OutputToFPS { get; }
        int SoilSpinUpFlag { get; }
        int BiomassSpinUpFlag { get; }
        double SpinUpTolerance { get; }
        int SpinUpIterations { get; }
        /// The maximum relative biomass for each shade class.
        Library.Parameters.Ecoregions.AuxParm<Percentage>[] MinRelativeBiomass { get; }
        /// Definitions of sufficient light probabilities.
        List<ISufficientLight> LightClassProbabilities { get; }
        Library.Parameters.Species.AuxParm<int> SppFunctionalType { get; }
        Library.Parameters.Species.AuxParm<double> LeafLongevity { get; }
        Library.Parameters.Species.AuxParm<bool> Epicormic { get; }
        Library.Parameters.Species.AuxParm<byte> FireTolerance { get; }
        Library.Parameters.Species.AuxParm<byte> ShadeTolerance { get; }
        Library.Parameters.Species.AuxParm<double> MortCurveShape { get; }
        Library.Parameters.Species.AuxParm<int> MerchStemsMinAge { get; }
        Library.Parameters.Species.AuxParm<double> MerchCurveParmA { get; }
        Library.Parameters.Species.AuxParm<double> MerchCurveParmB { get; }
        Library.Parameters.Species.AuxParm<double> PropNonMerch { get; }
        Library.Parameters.Species.AuxParm<double> GrowthCurveShapeParm { get; }
        Library.Parameters.Ecoregions.AuxParm<double> FieldCapacity { get; }
        Library.Parameters.Ecoregions.AuxParm<double> Latitude { get; }
        IDictionary<int, IDOMPool> DOMPools { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> DOMDecayRates { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> DOMPoolAmountT0 { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> DOMPoolQ10 { get; }
        double PropBiomassFine { get; }
        double PropBiomassCoarse { get; }
        double PropDOMSlowAGToSlowBG { get; }
        double PropDOMStemSnagToMedium { get; }
        double PropDOMBranchSnagToFastAG { get; }
        //root parameters
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> MinWoodyBio { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> Ratio { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> PropFine { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> FineTurnover { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> CoarseTurnover { get; }

        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<ITimeCollection<IANPP>>> ANPPTimeCollection { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<ITimeCollection<IMaxBiomass>>> MaxBiomassTimeCollection { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<ITimeCollection<IEstabProb>>> EstabProbTimeCollection { get; }
        Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<double>> EstablishProbability { get; }
    }
}
