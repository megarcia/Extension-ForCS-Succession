// Authors: Caren Dymond, Sarah Beukema

// NOTE: IEcoregion --> Landis.Core
// NOTE: IEcoregionDataset --> Landis.Core
// NOTE: InputValue --> Landis.Utilities
// NOTE: InputValueException --> Landis.Utilities
// NOTE: ISpecies --> Landis.Core
// NOTE: ISpeciesDataset --> Landis.Core

using Landis.Core;
using Landis.Library.Parameters;
using Landis.Utilities;

namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public static class Util
    {
        public static Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<T>> CreateSpeciesEcoregionParm<T>(ISpeciesDataset speciesDataset, IEcoregionDataset ecoregionDataset)
        {
            Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<T>> newParm;
            newParm = new Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<T>>(speciesDataset);
            foreach (ISpecies species in speciesDataset)
                newParm[species] = new Library.Parameters.Ecoregions.AuxParm<T>(ecoregionDataset);
            return newParm;
        }

        public static Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<T[]>> CreateSpeciesEcoregionArrayParm<T>(ISpeciesDataset speciesDataset, IEcoregionDataset ecoregionDataset, int n)
        {
            Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<T[]>> newParm;
            newParm = new Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<T[]>>(speciesDataset);
            foreach (ISpecies species in speciesDataset)
            {
                newParm[species] = new Library.Parameters.Ecoregions.AuxParm<T[]>(ecoregionDataset);
                foreach (IEcoregion ecoregion in ecoregionDataset)
                    newParm[species][ecoregion] = new T[n];
            }
            return newParm;
        }

        /// <summary>
        /// Converts a table indexed by species and ecoregion into a
        /// 2-dimensional array.
        /// </summary>
        public static T[,] ToArray<T>(Library.Parameters.Species.AuxParm<Library.Parameters.Ecoregions.AuxParm<T>> table)
        {
            T[,] array = new T[PlugIn.ModelCore.Ecoregions.Count, PlugIn.ModelCore.Species.Count];
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
                    array[ecoregion.Index, species.Index] = table[species][ecoregion];
            }
            return array;
        }

        public static double CheckBiomassParm(InputValue<double> newValue,
                                              double minValue, double maxValue)
        {
            if (newValue != null)
            {
                if (newValue.Actual < minValue || newValue.Actual > maxValue)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between {1:0.0} and {2:0.0}",
                                                  newValue.String, minValue, maxValue);
            }
            return newValue.Actual;
        }

        public static double CheckBiomassParm(double newValue, double minValue,
                                              double maxValue, string name)
        {
            if (newValue < minValue || newValue > maxValue)
                throw new InputValueException(name,
                                              "{0} is not between {1:0.0} and {2:0.0}",
                                              name, minValue, maxValue);
            return newValue;
        }

        public static int CheckBiomassParm(InputValue<int> newValue,
                                           int minValue, int maxValue)
        {
            if (newValue != null)
            {
                if (newValue.Actual < minValue || newValue.Actual > maxValue)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between {1:0.0} and {2:0.0}",
                                                  newValue.String, minValue, maxValue);
            }
            return newValue.Actual;
        }
    }
}
