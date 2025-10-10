// Authors: Caren Dymond, Sarah Beukema

// NOTE: AtEndOfInput --> Landis.Utilities
// NOTE: CurrentLine --> Landis.Utilities
// NOTE: GetNextLine --> Landis.Utilities
// NOTE: IEcoregion --> Landis.Core
// NOTE: InputValue --> Landis.Utilities
// NOTE: InputValueException --> Landis.Utilities
// NOTE: ISpecies --> Landis.Core
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
    public class InputDisturbanceMatrixParser : Landis.Utilities.TextParser<IInputDisturbanceMatrixParams>
    {
        private delegate void SetParmMethod<TParm>(ISpecies species,
                                                   IEcoregion ecoregion,
                                                   InputValue<TParm> newValue);

        static InputDisturbanceMatrixParser()
        {
        }

        public InputDisturbanceMatrixParser()
        {
        }

        protected override IInputDisturbanceMatrixParams Parse()
        {
            InputVar<string> landisData = new InputVar<string>("LandisData");
            ReadVar(landisData);
            if (landisData.Value.Actual != Names.ExtensionName)
                throw new InputValueException(landisData.Value.String,
                                              "The value is not \"{0}\"", Names.ExtensionName);
            StringReader currentLine;
            Dictionary<string, int> lineNumbers = new Dictionary<string, int>();
            // InputVars common to different read routines.
            InputVar<int> nDOMPoolID = new InputVar<int>("DOMPoolID");
            InputVar<double> dFracAir = new InputVar<double>("Prop to Air");
            InputVar<double> dFracFloor = new InputVar<double>("Prop to Floor");
            InputVar<double> dFracFPS = new InputVar<double>("Prop to FPS");
            InputVar<double> dFracDOM = new InputVar<double>("Prop to DOM");
            DisturbTransferFromPools[] aDisturbTransferPools;
            Dictionary<string, IDisturbTransferFromPools> dictDisturbTransfer;
            InputVar<string> sDisturbType = new InputVar<string>("Disturbance Type");
            InputVar<int> nIntensity = new InputVar<int>("Intensity");
            InputVar<int> nBiomassPoolID = new InputVar<int>("Biomass Pool ID");
            int nread = 0;
            InputDisturbanceMatrixParams parameters = new InputDisturbanceMatrixParams();
            PlugIn.ModelCore.UI.WriteLine("Reading new DM input file.");
            // First set-up the DOM pools (This used to be done already, but we will do it manually now
            for (int n = 1; n < Constants.NUMSOILPOOLS + 1; n++)
                parameters.SetDOMPool(n, "not specified");
            // DisturbFireTransferDOM Parameters
            ReadName(Names.DisturbFireTransferDOM);
            aDisturbTransferPools = new DisturbTransferFromPools[Constants.FIREINTENSITYCOUNT];
            for (int n = 0; n < Constants.FIREINTENSITYCOUNT; n++)
            {
                aDisturbTransferPools[n] = new DisturbTransferFromPools(Names.DisturbTypeFire);
                aDisturbTransferPools[n].InitializeDOMPools(parameters.DOMPools);
            }
            string lastColumn = "the " + dFracFPS.Name + " column";
            nread = 0;
            while (! AtEndOfInput && (CurrentName != Names.DisturbOtherTransferDOM))
            {
                currentLine = new StringReader(CurrentLine);
                ReadValue(nIntensity, currentLine);
                // Validate input, as Intensity value is 1-based in range [1, Constants.FIREINTENSITYCOUNT]/
                if ((nIntensity.Value < 1) || (nIntensity.Value > Constants.FIREINTENSITYCOUNT))
                    throw new InputValueException(nIntensity.Name,
                                                  "DisturbFireTransferDOM: {0} is not a valid Intensity value.", nIntensity.Value.Actual);
                ReadValue(nDOMPoolID, currentLine);
                if ((nDOMPoolID.Value < 1) || (nDOMPoolID.Value > Constants.NUMSOILPOOLS))
                    throw new InputValueException(nDOMPoolID.Name,
                                                  "DisturbFireTransferDOM: {0} is not a valid DOM pool ID.", nDOMPoolID.Value.Actual);
                // Convert Intensity from 1-based in input file to 0-based simple array.
                DisturbTransferFromPool oDisturbTransfer = (DisturbTransferFromPool)((aDisturbTransferPools[nIntensity.Value - 1]).GetDisturbTransfer(nDOMPoolID.Value));
                ReadValue(dFracAir, currentLine);
                oDisturbTransfer.FracToAir = dFracAir.Value;
                ReadValue(dFracDOM, currentLine);
                oDisturbTransfer.FracToDOM = dFracDOM.Value;
                ReadValue(dFracFPS, currentLine);
                oDisturbTransfer.FracToFPS = dFracFPS.Value;
                if ((nDOMPoolID.Value == 2 || nDOMPoolID.Value == 4 || nDOMPoolID.Value == 7) && dFracFPS.Value > 0)
                    PlugIn.ModelCore.UI.WriteLine("DisturbFireTransferDOM: You have asked for belowground DOM pools to be transfered to the FPS after a fire. Please check.");
                if ((dFracAir.Value + dFracDOM.Value + dFracFPS.Value) > 1.0)
                    PlugIn.ModelCore.UI.WriteLine("DisturbFireTransferDOM: Proportions must not be greater than 1.");
                nread += 1;
                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }
            parameters.SetDisturbFireFromDOMPools(aDisturbTransferPools);
            if (nread < Constants.FIREINTENSITYCOUNT * (Constants.NUMSOILPOOLS - 1))
                PlugIn.ModelCore.UI.WriteLine("DisturbFireTransferDOM: Some rows are missing. C in these DOM pools will not be affected by the fire.");
            //  DisturbOtherTransferDOM Parameters
            ReadName(Names.DisturbOtherTransferDOM);
            dictDisturbTransfer = new Dictionary<string, IDisturbTransferFromPools>();
            lastColumn = "the " + dFracFPS.Name + " column";
            nread = 0;
            while (! AtEndOfInput && (CurrentName != Names.DisturbFireTransferBiomass))
            {
                currentLine = new StringReader(CurrentLine);
                ReadValue(sDisturbType, currentLine);
                DisturbTransferFromPools oDisturbTransferPools;
                if (dictDisturbTransfer.ContainsKey(sDisturbType.Value))
                    oDisturbTransferPools = (DisturbTransferFromPools)dictDisturbTransfer[sDisturbType.Value];
                else
                {
                    oDisturbTransferPools = new DisturbTransferFromPools(sDisturbType.Value);
                    oDisturbTransferPools.InitializeDOMPools(parameters.DOMPools);
                    dictDisturbTransfer.Add(sDisturbType.Value, oDisturbTransferPools);
                }
                ReadValue(nDOMPoolID, currentLine);
                if ((nDOMPoolID.Value < 1) || (nDOMPoolID.Value > Constants.NUMSOILPOOLS))
                    throw new InputValueException(nDOMPoolID.Name,
                                                  "DisturbOtherTransferDOM: {0} is not a valid DOM pool ID.", nDOMPoolID.Value.Actual);
                DisturbTransferFromPool oDisturbTransfer = (DisturbTransferFromPool)oDisturbTransferPools.GetDisturbTransfer(nDOMPoolID.Value);
                ReadValue(dFracAir, currentLine);
                oDisturbTransfer.FracToAir = dFracAir.Value;
                ReadValue(dFracDOM, currentLine);
                oDisturbTransfer.FracToDOM = dFracDOM.Value;
                ReadValue(dFracFPS, currentLine);
                oDisturbTransfer.FracToFPS = dFracFPS.Value;
                if ((nDOMPoolID.Value == 2 || nDOMPoolID.Value == 4 || nDOMPoolID.Value == 7) && dFracFPS.Value > 0)
                    PlugIn.ModelCore.UI.WriteLine("DisturbOtherTransferDOM: You have asked for belowground DOM pools to be transfered to the FPS after a disturbance. Please check.");
                if ((dFracAir.Value + dFracDOM.Value + dFracFPS.Value) > 1.0)
                    PlugIn.ModelCore.UI.WriteLine("DisturbOtherTransferDOM: Proportions must not be greater than 1.");
                nread += 1;
                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }
            parameters.SetDisturbOtherFromDOMPools(dictDisturbTransfer);
            if (nread < 3 * (Constants.NUMSOILPOOLS - 1))
                PlugIn.ModelCore.UI.WriteLine("DisturbOtherFromDOMPools: Some rows are missing. C in these DOM pools will not be affected by the disturbance.");
            //  DisturbFireTransferBiomass Parameters
            ReadName(Names.DisturbFireTransferBiomass);
            aDisturbTransferPools = new DisturbTransferFromPools[Constants.FIREINTENSITYCOUNT];
            for (int n = 0; n < Constants.FIREINTENSITYCOUNT; n++)
            {
                aDisturbTransferPools[n] = new DisturbTransferFromPools(Names.DisturbTypeFire);
                aDisturbTransferPools[n].InitializeBiomassPools();
            }
            lastColumn = "the " + dFracFPS.Name + " column";
            nread = 0;
            while (! AtEndOfInput && (CurrentName != Names.DisturbOtherTransferBiomass))
            {
                currentLine = new StringReader(CurrentLine);
                ReadValue(nIntensity, currentLine);
                // Validate input, as Intensity value is 1-based in range [1, Constants.FIREINTENSITYCOUNT]/
                if ((nIntensity.Value < 1) || (nIntensity.Value > Constants.FIREINTENSITYCOUNT))
                    throw new InputValueException(nIntensity.Name,
                                                  "DisturbFireTransferBiomass: {0} is not a valid Intensity value.", nIntensity.Value.Actual);
                ReadValue(nBiomassPoolID, currentLine);
                if ((nBiomassPoolID.Value < 1) || (nBiomassPoolID.Value > Constants.NUMBIOMASSCOMPONENTS))
                    throw new InputValueException(nBiomassPoolID.Name,
                                                  "DisturbFireTransferBiomass: {0} is not a valid biomass pool ID.", nBiomassPoolID.Value.Actual);
                // Convert Intensity from 1-based in input file to 0-based simple array.
                DisturbTransferFromPool oDisturbTransfer = (DisturbTransferFromPool)aDisturbTransferPools[nIntensity.Value - 1].GetDisturbTransfer(nBiomassPoolID.Value);
                ReadValue(dFracAir, currentLine);
                oDisturbTransfer.FracToAir = dFracAir.Value;
                ReadValue(dFracFPS, currentLine);
                oDisturbTransfer.FracToFPS = dFracFPS.Value;
                if (dFracFPS.Value > 0)
                    PlugIn.ModelCore.UI.WriteLine("DisturbFireTransferBiomass: Warning: you have asked for C to go to the FPS after a fire. Is this correct?");
                ReadValue(dFracDOM, currentLine);
                oDisturbTransfer.FracToDOM = dFracDOM.Value;
                if ((dFracAir.Value + dFracDOM.Value + dFracFPS.Value) != 1.0)
                    PlugIn.ModelCore.UI.WriteLine("DisturbFireTransferBiomass: Proportions must add to 1.");
                nread += 1;
                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }
            parameters.SetDisturbFireFromBiomassPools(aDisturbTransferPools);
            if (nread < Constants.FIREINTENSITYCOUNT * (Constants.NUMBIOMASSCOMPONENTS - 1))
                PlugIn.ModelCore.UI.WriteLine("DisturbFireTransferBiomass: Some combinations of Fire Intensity and biomass type are missing. When these biomass components are killed, C loss will not be captured.");
            //  DisturbOtherTransferBiomass Parameters
            ReadName(Names.DisturbOtherTransferBiomass);
            dictDisturbTransfer = new Dictionary<string, IDisturbTransferFromPools>();
            lastColumn = "the " + dFracFPS.Name + " column";
            nread = 0;
            while (! AtEndOfInput && (CurrentName != "No Section To Follow"))
            {
                currentLine = new StringReader(CurrentLine);
                ReadValue(sDisturbType, currentLine);
                DisturbTransferFromPools oDisturbTransferPools;
                if (dictDisturbTransfer.ContainsKey(sDisturbType.Value))
                    oDisturbTransferPools = (DisturbTransferFromPools)dictDisturbTransfer[sDisturbType.Value];
                else
                {
                    oDisturbTransferPools = new DisturbTransferFromPools(sDisturbType.Value);
                    oDisturbTransferPools.InitializeBiomassPools();
                    dictDisturbTransfer.Add(sDisturbType.Value, oDisturbTransferPools);
                }
                ReadValue(nBiomassPoolID, currentLine);
                if ((nBiomassPoolID.Value < 1) || (nBiomassPoolID.Value > Constants.NUMSOILPOOLS))
                    throw new InputValueException(nBiomassPoolID.Name,
                                                  "DisturbOtherTransferBiomass: {0} is not a valid biomass pool ID.", nBiomassPoolID.ToString());
                DisturbTransferFromPool oDisturbTransfer = (DisturbTransferFromPool)oDisturbTransferPools.GetDisturbTransfer(nBiomassPoolID.Value);
                ReadValue(dFracAir, currentLine);
                oDisturbTransfer.FracToAir = dFracAir.Value;
                ReadValue(dFracFPS, currentLine);
                oDisturbTransfer.FracToFPS = dFracFPS.Value;
                if (nBiomassPoolID.Value >= 5 && dFracFPS.Value > 0)
                    PlugIn.ModelCore.UI.WriteLine("DisturbOtherTransferBiomass: Warning: you have asked for root C to go to the FPS. Is this correct?");
                ReadValue(dFracDOM, currentLine);
                oDisturbTransfer.FracToDOM = dFracDOM.Value;
                if ((dFracAir.Value + dFracDOM.Value + dFracFPS.Value) != 1.0)
                    PlugIn.ModelCore.UI.WriteLine("DisturbOtherTransferBiomass: Proportions must add to 1.");
                nread += 1;
                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }
            parameters.SetDisturbOtherFromBiomassPools(dictDisturbTransfer);
            if (nread < 3 * (Constants.NUMBIOMASSCOMPONENTS - 1))
                PlugIn.ModelCore.UI.WriteLine("DisturbOtherTransferBiomass: Some biomass components are missing. When these biomass components are killed, C loss will not be captured.");
            return parameters; 
        }

        private bool CheckDisturbanceType(InputVar<string> sName)
        {
            string[] DistType = new string[] { "fire", "harvest", "wind", "bda", "drought", "other", "land use" };
            int i;
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
