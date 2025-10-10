// Authors: Caren Dymond, Sarah Beukema

// NOTE: AtEndOfInput --> Landis.Utilities
// NOTE: CurrentLine --> Landis.Utilities
// NOTE: GetNextLine --> Landis.Utilities
// NOTE: IEcoregion --> Landis.Core
// NOTE: IEcoregionDataset --> Landis.Core
// NOTE: InputValue --> Landis.Utilities
// NOTE: InputValueException --> Landis.Utilities
// NOTE: InputVar --> Landis.Utilities
// NOTE: ISpecies --> Landis.Core
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
    /// A parser that reads ForCS specific climate parameters from text input.
    /// </summary>
    public class InputClimateParser : Landis.Utilities.TextParser<IInputClimateParams>
    {
        private delegate void SetParmMethod<TParm>(ISpecies species,
                                                   IEcoregion ecoregion,
                                                   InputValue<TParm> newValue);

        private IEcoregionDataset ecoregionDataset;

        static InputClimateParser()
        {
            Percentage dummy = new Percentage();
        }

        public InputClimateParser()
        {
           ecoregionDataset = PlugIn.ModelCore.Ecoregions;
        }

        protected override IInputClimateParams Parse()
        {
            InputVar<string> landisData = new InputVar<string>("LandisData");
            ReadVar(landisData);
            if (landisData.Value.Actual != Names.ExtensionName)
                throw new InputValueException(landisData.Value.String,
                                              "The value is not \"{0}\"", Names.ExtensionName);
            StringReader currentLine;
            Dictionary<string, int> lineNumbers = new Dictionary<string, int>();
            InputVar<int> nYear = new InputVar<int>("Time Step");
            InputVar<string> sEcoregion = new InputVar<string>("Eco");
            InputVar<double> dAvgT = new InputVar<double>("AvgT");
            InputClimateParams parameters = new InputClimateParams();  
            ReadName(Names.ClimateTable);
            int nread = 0;            
            while (! AtEndOfInput && (CurrentName != "No Section To Follow"))
            {
                currentLine = new StringReader(CurrentLine);
                ReadValue(nYear, currentLine);
                if (nYear.Value < 0)
                    throw new InputValueException(nYear.Name,
                                                  "{0} is not a valid year.", nYear.ToString());
                ReadValue(sEcoregion, currentLine);
                IEcoregion ecoregion = GetEcoregion(sEcoregion.Value);
                ReadValue(dAvgT, currentLine);
                // Create a ClimateTable object.
                parameters.ClimateAnnualCollection[ecoregion].Add(new ClimateAnnual(nYear.Value, dAvgT.Value));
                nread += 1;
                GetNextLine();
            }
            return parameters; 
        }

        private IEcoregion GetEcoregion(InputValue<string> ecoregionName)
        {
            IEcoregion ecoregion = ecoregionDataset[ecoregionName.Actual];
            if (ecoregion == null)
                throw new InputValueException(ecoregionName.String, "{0} is not an ecoregion name.", ecoregionName.String);
            return ecoregion;
        }
    }
}
