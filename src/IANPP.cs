namespace Landis.Extension.Succession.ForC
{
    public interface IANPP : ITimeInput
    {
        double GramsPerMetre2Year { get; }
        double StdDev { get; }
    }
}