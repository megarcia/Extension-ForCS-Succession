namespace Landis.Extension.Succession.ForC
{
    public interface IDisturbTransferFromPool
    {
        int ID { get; }
        string Name { get; }
        double FracToAir { get; }
        double FracToFloor { get; }
        double FracToFPS { get; }
        double FracToDOM { get; }
    }
}
