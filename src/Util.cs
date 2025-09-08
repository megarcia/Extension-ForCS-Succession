//  Authors:  Caren Dymond, Sarah Beukema

using Landis.SpatialModeling;
using Landis.Core;
using Landis.Utilities;

namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public static class Util
    {

        //---------------------------------------------------------------------

        public static Landis.Library.Parameters.Species.AuxParm<Landis.Library.Parameters.Ecoregions.AuxParm<T>> CreateSpeciesEcoregionParm<T>(ISpeciesDataset speciesDataset, IEcoregionDataset ecoregionDataset)
        {
            Landis.Library.Parameters.Species.AuxParm<Landis.Library.Parameters.Ecoregions.AuxParm<T>> newParm;
            newParm = new Landis.Library.Parameters.Species.AuxParm<Landis.Library.Parameters.Ecoregions.AuxParm<T>>(speciesDataset);
            foreach (ISpecies species in speciesDataset)
            {
                newParm[species] = new Landis.Library.Parameters.Ecoregions.AuxParm<T>(ecoregionDataset);
            }
            return newParm;
        }
        //---------------------------------------------------------------------

        public static Landis.Library.Parameters.Species.AuxParm<Landis.Library.Parameters.Ecoregions.AuxParm<T[]>> CreateSpeciesEcoregionArrayParm<T>(ISpeciesDataset speciesDataset, IEcoregionDataset ecoregionDataset, int n)
        {
            Landis.Library.Parameters.Species.AuxParm<Landis.Library.Parameters.Ecoregions.AuxParm<T[]>> newParm;
            newParm = new Landis.Library.Parameters.Species.AuxParm<Landis.Library.Parameters.Ecoregions.AuxParm<T[]>>(speciesDataset);
            foreach (ISpecies species in speciesDataset)
            {
                newParm[species] = new Landis.Library.Parameters.Ecoregions.AuxParm<T[]>(ecoregionDataset);
                foreach (IEcoregion ecoregion in ecoregionDataset)
                {
                    newParm[species][ecoregion] = new T[n];
                }
            }
            return newParm;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Converts a table indexed by species and ecoregion into a
        /// 2-dimensional array.
        /// </summary>
        public static T[,] ToArray<T>(Landis.Library.Parameters.Species.AuxParm<Landis.Library.Parameters.Ecoregions.AuxParm<T>> table)
        {
            T[,] array = new T[PlugIn.ModelCore.Ecoregions.Count, PlugIn.ModelCore.Species.Count];
            foreach (ISpecies species in PlugIn.ModelCore.Species) {
                foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions) {
                    array[ecoregion.Index, species.Index] = table[species][ecoregion];
                }
            }
            return array;
        }
        //---------------------------------------------------------------------

        // DEVNOTE: This is commented out, as it seems that it is not possible to fully develop
        // using templates in C#
        // Error	5	Operator '<' cannot be applied to operands of type 'T' and 'T'	D:\Projects\EN1781\Source\ForCS\utility\Util.cs	52	21	ForCS
        //public static T CheckRange<T>(InputValue<T> newValue, T tMin, T tMax, bool bMinClosed, bool bMaxClosed)
        //{
        //    if (newValue != null)
        //    {
        //        if (newValue.Actual < tMin || newValue.Actual > tMax)
        //            throw new InputValueException(newValue.String,
        //                                          "{0} is not between {1:0.0} and {2:0.0}",
        //                                          newValue.String, tMin, tMax);
        //    }
        //    return newValue.Actual;
        //}
        //---------------------------------------------------------------------

        public static double CheckBiomassParm(InputValue<double> newValue,
                                                    double minValue,
                                                    double maxValue)
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

        //---------------------------------------------------------------------

        public static double CheckBiomassParm(double newValue,
                                                    double minValue,
                                                    double maxValue,
                                                    string name)
        {
            if (newValue < minValue || newValue > maxValue)
                throw new InputValueException(name,
                                              "{0} is not between {1:0.0} and {2:0.0}",
                                              name, minValue, maxValue);
            return newValue;
        }
        //---------------------------------------------------------------------

        public static int CheckBiomassParm(InputValue<int> newValue,
                                                    int             minValue,
                                                    int             maxValue)
        {
            if (newValue != null) {
                if (newValue.Actual < minValue || newValue.Actual > maxValue)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between {1:0.0} and {2:0.0}",
                                                  newValue.String, minValue, maxValue);
            }
            return newValue.Actual;
        }
        //---------------------------------------------------------------------

        //public static Landis.Library.Parameters.Ecoregions.AuxParm<T> ConvertToActualValues<T>(Landis.Library.Parameters.Ecoregions.AuxParm<InputValue<T>> inputValues)
        //{
        //    Landis.Library.Parameters.Ecoregions.AuxParm<T> actualValues = new Landis.Library.Parameters.Ecoregions.AuxParm<T>(PlugIn.ModelCore.Ecoregions); //ecoregionDataset);
        //    foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
        //        if (inputValues[ecoregion] != null)
        //            actualValues[ecoregion] = inputValues[ecoregion].Actual;
        //    return actualValues;
        //}

        //---------------------------------------------------------------------

        //public static Landis.Library.Parameters.Species.AuxParm<T> ConvertToActualValues<T>(Landis.Library.Parameters.Species.AuxParm<InputValue<T>> inputValues)
        //{
        //    Species.AuxParm<T> actualValues = new Species.AuxParm<T>(PlugIn.ModelCore.Species);
        //    foreach (ISpecies species in PlugIn.ModelCore.Species)
        //        if (inputValues[species] != null)
        //            actualValues[species] = inputValues[species].Actual;
        //    return actualValues;
        //}
    }
}
