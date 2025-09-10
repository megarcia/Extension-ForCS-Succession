//  Authors:  Caren Dymond, Sarah Beukema

namespace Landis.Extension.Succession.ForC
{
    public class InputClimateParms : IInputClimateParms
    {
        private Library.Parameters.Ecoregions.AuxParm<ITimeCollection<IClimateAnnual>> m_ClimateAnnualCollection; 
        private IEcoregionDataset m_dsEcoregion;

        public InputClimateParms()
        {
            this.m_dsEcoregion = PlugIn.ModelCore.Ecoregions;
            this.m_ClimateAnnualCollection = new Library.Parameters.Ecoregions.AuxParm<ITimeCollection<IClimateAnnual>>(m_dsEcoregion);
            foreach (IEcoregion ecoregion in m_dsEcoregion)
                this.m_ClimateAnnualCollection[ecoregion] = new TimeCollection<IClimateAnnual>();
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
