namespace Landis.Extension.Succession.ForC
{
    public interface ISufficientLight
    {  
        byte ShadeClass { get; set; }
        double ProbabilityLight0 { get; set; }
        double ProbabilityLight1 { get; set; }
        double ProbabilityLight2 { get; set; }
        double ProbabilityLight3 { get; set; }
        double ProbabilityLight4 { get; set; }
        double ProbabilityLight5 { get; set; }
    }
}
