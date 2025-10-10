// Authors: Caren Dymond, Sarah Beukema

// NOTE: IEcoregion --> Landis.Core
// NOTE: IEcoregionDataset --> Landis.Core

using Landis.Core;
using Landis.Library.Parameters;

namespace Landis.Extension.Succession.ForC
{
    public class InputClimateParams : IInputClimateParams
    {
        private Library.Parameters.Ecoregions.AuxParm<ITimeCollection<IClimateAnnual>> m_ClimateAnnualCollection;
        private IEcoregionDataset m_dsEcoregion;

        public InputClimateParams()
        {
            m_dsEcoregion = PlugIn.ModelCore.Ecoregions;
            m_ClimateAnnualCollection = new Library.Parameters.Ecoregions.AuxParm<ITimeCollection<IClimateAnnual>>(m_dsEcoregion);
            foreach (IEcoregion ecoregion in m_dsEcoregion)
                m_ClimateAnnualCollection[ecoregion] = new TimeCollection<IClimateAnnual>();
        }

        public Library.Parameters.Ecoregions.AuxParm<ITimeCollection<IClimateAnnual>> ClimateAnnualCollection
        {
            get
            {
                return m_ClimateAnnualCollection;
            }
        }
    }
}
