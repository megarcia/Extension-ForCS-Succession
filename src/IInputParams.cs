// Authors: Caren Dymond, Sarah Beukema

// NOTE: SeedingAlgorithms --> Landis.Library.Succession
// NOTE: Percentage --> Landis.Utilities

using System.Collections.Generic;
using Landis.Library.Parameters;
using Landis.Library.Succession;
using Landis.Utilities;

namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// The parameters for biomass succession.
    /// </summary>
    public interface IInputParams
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
        Library.Parameters.Ecoregions.AuxParm<Percentage>[] MinRelativeBiomass { get; }
        List<ILight> LightClassProbabilities { get; }
        Library.Parameters.Species.AuxParm<int> SppFunctionalType { get; }
        Library.Parameters.Species.AuxParm<double> LeafLongevity { get; }
        Library.Parameters.Species.AuxParm<bool> Epicormic { get; }
        Library.Parameters.Species.AuxParm<byte> FireTolerance { get; }
        Library.Parameters.Species.AuxParm<byte> ShadeTolerance { get; }
        Library.Parameters.Species.AuxParm<double> MortCurveShape { get; }
        Library.Parameters.Species.AuxParm<int> MerchStemsMinAge { get; }
        Library.Parameters.Species.AuxParm<double> MerchCurveParmA { get; }
        Library.Parameters.Species.AuxParm<double> MerchCurveParmB { get; }
        Library.Parameters.Species.AuxParm<double> FracNonMerch { get; }
        Library.Parameters.Species.AuxParm<double> GrowthCurveShapeParm { get; }
        Library.Parameters.Ecoregions.AuxParm<double> FieldCapacity { get; }
        Library.Parameters.Ecoregions.AuxParm<double> Latitude { get; }
        IDictionary<int, IDOMPool> DOMPools { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> DOMDecayRates { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> DOMPoolAmountT0 { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> DOMPoolQ10 { get; }
        double FracBiomassFine { get; }
        double FracBiomassCoarse { get; }
        double FracDOMSlowAGToSlowBG { get; }
        double FracDOMStemSnagToMedium { get; }
        double FracDOMBranchSnagToFastAG { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> MinWoodyBio { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> Ratio { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> FracFine { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> FineTurnover { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<double[]>> CoarseTurnover { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<ITimeCollection<IANPP>>> ANPPTimeCollection { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<ITimeCollection<IMaxBiomass>>> MaxBiomassTimeCollection { get; }
        Library.Parameters.Ecoregions.AuxParm<Library.Parameters.Species.AuxParm<ITimeCollection<IProbEstablishment>>> ProbEstablishmentTimeCollection { get; }
        Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<double>> ProbEstablishment { get; }
    }
}
