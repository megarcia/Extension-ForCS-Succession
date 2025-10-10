// NOTE: InputValueException --> Landis.Utilities

using System.Collections.Generic;
using System.Diagnostics;
using Landis.Utilities;

namespace Landis.Extension.Succession.ForC
{
    class DisturbTransferFromPools : IDisturbTransferFromPools
    {
        private string m_sName;

        /// <summary>
        /// Implemented as a dictionary, however it could also be a simple array.
        /// </summary>
        private Dictionary<int, DisturbTransferFromPool> m_dict;

        public DisturbTransferFromPools(string sName)
        {
            Debug.Assert(!string.IsNullOrEmpty(sName));
            m_sName = sName;
            m_dict = new Dictionary<int, DisturbTransferFromPool>();
        }

        public void InitializeDOMPools(IDictionary<int, IDOMPool> dictDOMPools)
        {
            m_dict.Clear();
            foreach (KeyValuePair<int, IDOMPool> kvp in dictDOMPools)
                m_dict.Add(kvp.Value.ID, new DisturbTransferFromPool(kvp.Value.ID, kvp.Value.Name));
        }

        public void InitializeBiomassPools()
        {
            m_dict.Clear();
            m_dict.Add((int)eBiomassPoolIDs.Merchantable, new DisturbTransferFromPool((int)eBiomassPoolIDs.Merchantable, "Merchantable"));
            m_dict.Add((int)eBiomassPoolIDs.Foliage, new DisturbTransferFromPool((int)eBiomassPoolIDs.Foliage, "Foliage"));
            m_dict.Add((int)eBiomassPoolIDs.Other, new DisturbTransferFromPool((int)eBiomassPoolIDs.Other, "Other"));
            m_dict.Add((int)eBiomassPoolIDs.SubMerchantable, new DisturbTransferFromPool((int)eBiomassPoolIDs.SubMerchantable, "Sub-Merchantable"));
            m_dict.Add((int)eBiomassPoolIDs.CoarseRoot, new DisturbTransferFromPool((int)eBiomassPoolIDs.CoarseRoot, "Coarse Root"));
            m_dict.Add((int)eBiomassPoolIDs.FineRoot, new DisturbTransferFromPool((int)eBiomassPoolIDs.FineRoot, "Fine Root"));
        }

        /// <param name="nPoolID">Pool ID, 1-based</param>
        public IDisturbTransferFromPool GetDisturbTransfer(int nPoolID)
        {
            if (!m_dict.ContainsKey(nPoolID))
                throw new InputValueException(nPoolID.ToString(),
                                              "Pool ID cannot be found.  Has Initialize*() been called?");
            return m_dict[nPoolID];
        }
    }
}
