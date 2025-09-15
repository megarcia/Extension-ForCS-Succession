namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// eBiomassPoolIDs - IDs used to index directly into the calculations.
    /// Note that unlike Soils.ComponentType, these are 1-based.
    /// </summary>
    public enum eBiomassPoolIDs
    {
        Merchantable = 1,
        Foliage,
        Other,
        SubMerchantable,
        CoarseRoot,
        FineRoot
    };
}