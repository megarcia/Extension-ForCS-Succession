//  Authors:  Caren Dymond, Sarah Beukema

namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// The parameters for ForC snag initialization.
    /// </summary>
    public interface IInputSnagParams
    {
        int[] SnagSpecies { get; } 
        int[] SnagAgeAtDeath { get; }
        int[] SnagTimeSinceDeath { get; }
        string[] SnagDisturb { get; }       
    }
}
