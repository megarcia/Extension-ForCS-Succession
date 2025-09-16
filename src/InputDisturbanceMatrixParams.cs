//  Authors:  Caren Dymond, Sarah Beukema

using System.Collections.Generic;

namespace Landis.Extension.Succession.ForC
{
    public class InputDisturbanceMatrixParams : IInputDisturbanceMatrixParams
    {
        private IDictionary<int, IDOMPool> m_dictDOMPools;
        private IDisturbTransferFromPools[] m_aDisturbFireFromDOMPools;
        private IDictionary<string, IDisturbTransferFromPools> m_dictDisturbOtherFromDOMPools;
        private IDisturbTransferFromPools[] m_aDisturbFireFromBiomassPools;
        private IDictionary<string, IDisturbTransferFromPools> m_dictDisturbOtherFromBiomassPools;

        public IDictionary<int, IDOMPool> DOMPools 
        { 
            get 
            { 
                return m_dictDOMPools; 
            } 
        }

        /// <summary>
        /// Returns an array of IDisturbTransferFromPools objects, indexed by a 0-based severity level.
        /// </summary>
        public IDisturbTransferFromPools[] DisturbFireFromDOMPools 
        { 
            get 
            { 
                return m_aDisturbFireFromDOMPools;
            }
        }

        /// <summary>
        /// Returns a dictionary of IDisturbTransferFromPools objects, indexed by name of the disturbance (e.g. "Harvest").
        /// </summary>
        public IDictionary<string, IDisturbTransferFromPools> DisturbOtherFromDOMPools 
        {
            get 
            { 
                return m_dictDisturbOtherFromDOMPools;
            }
        }

        /// <summary>
        /// Returns an array of IDisturbTransferFromPools objects, indexed by a 0-based severity level.
        /// </summary>
        public IDisturbTransferFromPools[] DisturbFireFromBiomassPools 
        { 
            get 
            { 
                return m_aDisturbFireFromBiomassPools;
            }
        }

        /// <summary>
        /// Returns a dictionary of IDisturbTransferFromPools objects, indexed by name of the disturbance (e.g. "Harvest").
        /// </summary>
        public IDictionary<string, IDisturbTransferFromPools> DisturbOtherFromBiomassPools 
        { 
            get 
            {
                return m_dictDisturbOtherFromBiomassPools;
            }
        }

        public void SetDisturbFireFromDOMPools(IDisturbTransferFromPools[] aDisturbTransfer)
        {
            m_aDisturbFireFromDOMPools = aDisturbTransfer;
        }

        public void SetDisturbOtherFromDOMPools(IDictionary<string, IDisturbTransferFromPools> dictDisturbTransfer)
        {
            m_dictDisturbOtherFromDOMPools = dictDisturbTransfer;
        }

        public void SetDisturbFireFromBiomassPools(IDisturbTransferFromPools[] aDisturbTransfer)
        {
            m_aDisturbFireFromBiomassPools = aDisturbTransfer;
        }

        public void SetDisturbOtherFromBiomassPools(IDictionary<string, IDisturbTransferFromPools> dictDisturbTransfer)
        {
            m_dictDisturbOtherFromBiomassPools = dictDisturbTransfer;
        }

        public void SetDOMPool(int nID, string sName)
        {
            DOMPool pool = new DOMPool(nID, sName);
            m_dictDOMPools.Add(pool.ID, pool);
        }

        public InputDisturbanceMatrixParams()
        {
            m_dictDOMPools = new Dictionary<int, IDOMPool>();
        }
    }
}
