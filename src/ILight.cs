namespace Landis.Extension.Succession.ForC
{
    public interface ILight
    {  
        byte ShadeClass { get; set; }
        double ProbSufficientLight0 { get; set; }
        double ProbSufficientLight1 { get; set; }
        double ProbSufficientLight2 { get; set; }
        double ProbSufficientLight3 { get; set; }
        double ProbSufficientLight4 { get; set; }
        double ProbSufficientLight5 { get; set; }
    }
}
