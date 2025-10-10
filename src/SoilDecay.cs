// NOTE: IEcoregion --> Landis.Core
// NOTE: ISpecies --> Landis.Core

using System;
using System.Diagnostics;
using Landis.Core;

namespace Landis.Extension.Succession.ForC
{
    public class SoilDecay
    {
        /// <summary>
        /// Calculates the decay rates of each pool and species.
        /// </summary>
        /// <param name="ecoregion"></param>
        /// <param name="species"></param>
        public static void CalcDecayRates(IEcoregion ecoregion, ISpecies species)
        {
            /* CODE RELATED TO THE USE OF ONE OF THE BIGGER LANDIS CLIMATE LIBRARIES
            int year = PlugIn.ModelCore.CurrentTime;
            AnnualClimate_Monthly ecoClimate = EcoregionData.AnnualWeather[ecoregion]; //Climate Library v2.0
            //AnnualClimate ecoClimate = EcoregionData.AnnualWeather[ecoregion];  //Climate Library on GitHub
            double MeanAnnualTemperature = (double)ecoClimate.CalculateMeanAnnualTemp(year); //Climate Library v2.0
            //double MeanAnnualTemperature = (double)ecoClimate.MeanAnnualTemp(year);  //Climate Library on GitHub
            double AnnualPrecipitation = ecoClimate.TotalAnnualPrecip; //Climate Library v2.0
            //double AnnualPrecipitation = ecoClimate.TotalAnnualPrecip();  //Climate Library on GitHub
            //logFluxSum.WriteLine("{0},{1},{2},{3:0.000},", year, site.DataIndex, ecoregion.Index, MeanAnnualTemperature);
            */

            double MeanAnnualTemperature = EcoregionData.AnnualTemperature[ecoregion];
            double AnnualPrecipitation = 1.0;           // not actually used
            // First we assign precipitation and temperature effects to the 
            // decay rates.  The very fast, fast, medium, and slow pools are 
            // all influenced by these effects.  Note that decay rates can 
            // vary by species now
            for (int currPool = 0; currPool < Constants.NUMSOILPOOLS - 1; currPool++)
            {
                // DEVNOTE: DOMSoilVars.decayRates use a 0-based index into an array,
                // whereas DOMPools requires a key that is 1-based.
                Debug.Assert(SoilVars.iParams.DOMPools.ContainsKey(currPool + 1));
                double DOMdecayrate = SoilVars.iParams.DOMDecayRates[ecoregion][species][currPool];
                double decayFTemp = CalcDecayFTemp(MeanAnnualTemperature, SoilVars.iParams.DOMPoolQ10[ecoregion][species][currPool]);
                double decayFPrecip = CalcDecayFPrecip(AnnualPrecipitation);
                SoilVars.decayRates[currPool, species.Index] = DOMdecayrate * decayFTemp * decayFPrecip;
            }
            return;            
        }

        /// <summary>
        /// Calculate the temperature modifier on decay rate.
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="q10Rate"></param>
        /// <returns></returns>
        public static double CalcDecayFTemp(double temperature,
                                            double q10Rate)
        {
            double decayFTemp = Math.Exp((temperature - Constants.DECAYREFTEMP) * Math.Log(q10Rate) * 0.1);
            return decayFTemp;
        }

        /// <summary>
        /// Calculate the precipitation modifier on decay rate.
        /// </summary>
        /// <param name="precipitation"></param>
        /// <returns></returns>
        public static double CalcDecayFPrecip(double precipitation)
        {
            // Currently, precipitation does not influence the decay rates.
            double decayFPrecip = 1.0;
            return decayFPrecip;
        }
    }
}
