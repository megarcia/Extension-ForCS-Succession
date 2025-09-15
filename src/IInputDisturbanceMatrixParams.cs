//  Authors:  Caren Dymond, Sarah Beukema

using System.Collections.Generic;

namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// The parameters for Disturbance Matrices.
    /// </summary>
    public interface IInputDisturbanceMatrixParams
    {
        IDictionary<int, IDOMPool> DOMPools { get; }

        /// <summary>
        /// Returns an array of IDisturbTransferFromPools objects, indexed by a 0-based severity level.
        /// </summary>
        IDisturbTransferFromPools[] DisturbFireFromDOMPools { get; }

        /// <summary>
        /// Returns a dictionary of IDisturbTransferFromPools objects, indexed by name of the disturbance (e.g. "Harvest").
        /// </summary>
        IDictionary<string, IDisturbTransferFromPools> DisturbOtherFromDOMPools { get; }

        /// <summary>
        /// Returns an array of IDisturbTransferFromPools objects, indexed by a 0-based severity level.
        /// </summary>
        IDisturbTransferFromPools[] DisturbFireFromBiomassPools { get; }

        /// <summary>
        /// Returns a dictionary of IDisturbTransferFromPools objects, indexed by name of the disturbance (e.g. "Harvest").
        /// </summary>
        IDictionary<string, IDisturbTransferFromPools> DisturbOtherFromBiomassPools { get; }
    }
}
