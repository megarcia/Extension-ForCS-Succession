namespace Landis.Extension.Succession.ForC
{
    public interface IDisturbTransferFromPool
    {
        int ID { get; }
        string Name { get; }
        double PropToAir { get; }
        double PropToFloor { get; }
        double PropToFPS { get; }
        double PropToDOM { get; }
    }
}
