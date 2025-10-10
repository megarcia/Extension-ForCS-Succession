// Authors: Caren Dymond, Sarah Beukema (based on code from Robert Scheller)

// NOTE: IOutputRaster --> Landis.SpatialModeling
// NOTE: IntPixel --> Landis.SpatialModeling

using Landis.SpatialModeling;

namespace Landis.Extension.Succession.ForC
{
    public class Outputs
    {
        public static void WriteMaps(string outpath, int outint)
        {
            int nOutputFiles = 7;
            string[] pathname = new string[nOutputFiles];
            int printval = 0;
            pathname[0] = outpath + "/SoilC-{timestep}.tif";
            pathname[1] = outpath + "/BiomassC-{timestep}.tif";
            pathname[2] = outpath + "/NPP-{timestep}.tif";
            pathname[3] = outpath + "/NEP-{timestep}.tif";
            pathname[4] = outpath + "/NBP-{timestep}.tif";
            pathname[5] = outpath + "/RH-{timestep}.tif";
            pathname[6] = outpath + "/ToFPS-{timestep}.tif";
            for (int j = 0; j < nOutputFiles; j++)
            {
                // Skip map outputs if ForCSMapControl table indicates so
                if (j == 0 && SoilVars.iParams.OutputSDOMC == 0)
                    continue;
                if (j == 1 && SoilVars.iParams.OutputBiomassC == 0)
                    continue;
                if (j == 2 && SoilVars.iParams.OutputNPP == 0)
                    continue;
                if (j == 3 && SoilVars.iParams.OutputNEP == 0)
                    continue;
                if (j == 4 && SoilVars.iParams.OutputNBP == 0)
                    continue;
                if (j == 5 && SoilVars.iParams.OutputRH == 0)
                    continue;
                if (j == 6 && SoilVars.iParams.OutputToFPS == 0)
                    continue;
                string path1 = MapFileNames.ReplaceTemplateVars(@pathname[j], PlugIn.ModelCore.CurrentTime);
                using (IOutputRaster<IntPixel> outputRaster = PlugIn.ModelCore.CreateRaster<IntPixel>(path1, PlugIn.ModelCore.Landscape.Dimensions))
                {
                    IntPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive)
                        {
                            // put the right value into the variable that will be assigned to the map
                            switch (j)
                            {
                                case 0:
                                    printval = (int)SiteVars.SoilOrganicMatterC[site];
                                    break;
                                case 1:
                                    printval = (int)SiteVars.TotBiomassC[site];
                                    break;
                                case 2:
                                    printval = (int)SiteVars.NPP[site];
                                    break;
                                case 3:
                                    printval = (int)SiteVars.NEP[site];
                                    break;
                                case 4:
                                    printval = (int)SiteVars.NBP[site];
                                    break;
                                case 5:
                                    printval = (int)SiteVars.RH[site];
                                    break;
                                case 6:
                                    printval = (int)SiteVars.ToFPSC[site];
                                    break;
                                default:
                                    printval = 0;
                                    break;
                            }
                            //now print the value to the map
                            pixel.MapCode.Value = printval;
                        }
                        else
                        {
                            //  Inactive site
                            pixel.MapCode.Value = 0;
                        }
                        outputRaster.WriteBufferPixel();
                    }
                }
            }
        }
    }
}
