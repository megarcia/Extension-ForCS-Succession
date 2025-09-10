//  Authors:  Caren Dymond, Sarah Beukema

namespace Landis.Extension.Succession.ForC
{
    public class FireEffects
    {
        public const int mk_nIntensityCount = 5;

        /// <summary>
        /// Crown scorching, when a cohort loses its foliage but is not killed.
        /// </summary>
        /// <param name="cohort"></param>
        /// <param name="siteSeverity"></param>
        /// <returns></returns>
        public static double CrownScorching(ICohort cohort, byte siteSeverity)
        {
            int difference = (int)siteSeverity - SpeciesData.FireTolerance[cohort.Species];
            double ageFraction = 1.0 - ((double)cohort.Data.Age / (double)cohort.Species.Longevity);
            if (SpeciesData.Epicormic[cohort.Species])
            {
                if (difference < 0)
                    return 0.5 * ageFraction;
                if (difference == 0)
                    return 0.75 * ageFraction;
                if (difference > 0)
                    return 1.0 * ageFraction;
            }
            return 0.0;
        }
    }
}
