namespace Landis.Extension.Succession.ForC
{
    public interface IDisturbTransferFromPools
    {
        /// <param name="nPoolID">Pool ID, 1-based</param>
        IDisturbTransferFromPool GetDisturbTransfer(int nPoolID);
    }
}
