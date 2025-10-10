// Authors: Caren Dymond, Sarah Beukema

// NOTE: AtEndOfInput --> Landis.Utilities
// NOTE: CurrentLine --> Landis.Utilities
// NOTE: GetNextLine --> Landis.Utilities
// NOTE: IEcoregion --> Landis.Core
// NOTE: InputValue --> Landis.Utilities
// NOTE: InputValueException --> Landis.Utilities
// NOTE: InputVar --> Landis.Utilities
// NOTE: ISpecies --> Landis.Core
// NOTE: ISpeciesDataset --> Landis.Core
// NOTE: Percentage --> Landis.Utilities
// NOTE: ReadName --> Landis.Utilities
// NOTE: ReadValue --> Landis.Utilities
// NOTE: ReadVar --> Landis.Utilities
// NOTE: StringReader --> Landis.Utilities

using System.Collections.Generic;
using Landis.Core;
using Landis.Utilities;

namespace Landis.Extension.Succession.ForC
{
    /// <summary>
    /// A parser that reads biomass succession parameters from text input.
    /// </summary>
    public class InputSnagParser : Landis.Utilities.TextParser<IInputSnagParams>
    {
        private delegate void SetParmMethod<TParm>(ISpecies species,
                                                   IEcoregion ecoregion,
                                                   InputValue<TParm> newValue);
        private ISpeciesDataset speciesDataset;
        private Dictionary<string, int> speciesLineNums;
        private InputVar<string> speciesName;

        static InputSnagParser()
        {
            Percentage dummy = new Percentage();
        }

        public InputSnagParser()
        {
            speciesDataset = PlugIn.ModelCore.Species;
            speciesLineNums = new Dictionary<string, int>();
            speciesName = new InputVar<string>("Species");
        }

        protected override IInputSnagParams Parse()
        {
            InputVar<string> landisData = new InputVar<string>("LandisData");
            ReadVar(landisData);
            if (landisData.Value.Actual != Names.ExtensionName)
                throw new InputValueException(landisData.Value.String,
                                              "The value is not \"{0}\"", Names.ExtensionName);
            StringReader currentLine;
            Dictionary<string, int> lineNumbers = new Dictionary<string, int>();
            InputVar<int> dAgeAtDeath = new InputVar<int>("Age At Death");
            InputVar<int> dTimeSinceDeath = new InputVar<int>("Time Since Death");
            InputVar<string> sDisturbType = new InputVar<string>("Disturbance Type");
            InputSnagParams parameters = new InputSnagParams();  
            ReadName(Names.SnagData);
            int nread = 0;            
            while (! AtEndOfInput && (CurrentName != "No Section To Follow"))
            {
                currentLine = new StringReader(CurrentLine);
                ISpecies species = ReadSpecies(currentLine);
                ReadValue(dAgeAtDeath, currentLine);
                ReadValue(dTimeSinceDeath, currentLine);
                ReadValue(sDisturbType, currentLine);
                bool bOk = CheckDisturbanceType(sDisturbType);
                if (!bOk)
                    throw new InputValueException(sDisturbType.Name,
                                                  "Initial Snag Data {0} is not a valid disturbance type. Check that name is all lowercase.", sDisturbType.Value.Actual);
                if (nread > 19)
                    throw new InputValueException(Names.SnagData,
                                                  "Too many intial snag types were entered. The remaining ones will not be read");
                else
                    parameters.SetInitSnagInfo(species, dAgeAtDeath.Value, dTimeSinceDeath.Value, sDisturbType.Value, nread);
                nread++;
                GetNextLine();
            }
            return parameters; 
        }

        /// <summary>
        /// Reads a species name from the current line, and verifies the name.
        /// </summary>
        private ISpecies ReadSpecies(StringReader currentLine)
        {
            ReadValue(speciesName, currentLine);
            ISpecies species = speciesDataset[speciesName.Value.Actual];
            if (species == null)
                throw new InputValueException(speciesName.Value.String,
                                              "{0} is not a species name.",
                                              speciesName.Value.String);
            return species;
        }

        private ISpecies GetSpecies(InputValue<string> sName)
        {
            ISpecies species = speciesDataset[sName.Actual];
            if (species == null)
                throw new InputValueException(sName.String,
                                              "{0} is not an species name.", sName.String);
            return species;
        }

        private bool CheckDisturbanceType(InputVar<string> sName)
        {
            string[] DistType = new string[] { "fire", "harvest", "wind", "bda", "drought", "other", "land use" };
            int i = 0;
            for (i = DistType.GetUpperBound(0); i > 0; i--)
            {
                if (sName.Value == DistType[i])
                    break;
            }
            if (i == 0)
                return false;
            else
                return true;
        }
    }
}
