//  Authors:  Caren Dymond, Sarah Beukema

namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// The parameters for ForC climate initialization.
    /// </summary>
    public interface IInputClimateParams
    {
        Library.Parameters.Ecoregions.AuxParm<ITimeCollection<IClimateAnnual>> ClimateAnnualCollection { get; }
    }
}
