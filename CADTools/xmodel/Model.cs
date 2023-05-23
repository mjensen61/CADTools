using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CADTools
{
    internal class Model
    {
        public string libraryfilepath = "";
        public string localfilepath = "";
        public string tplpath = "";
        public string blkpath = "";
        public string pdfpath = "";
        public string dwgpath = "";
        public string layfile = "";
        public string blkextension = ".dwg";
        public string tplextension = ".dwt";
        public string pdfextension = ".pdf";
        public string lnkextension = ".lnk";

        public Boolean templatesLoaded = false;
        public Boolean blocksLoaded = false;
        public Boolean layersLoaded = false;
        public Boolean standardsloaded = false;

        public int InitializePaths()
        {
            int countpaths = 0;

            // Initialize library paths
            libraryfilepath = "";

            INIConfig cfg = new INIConfig("");

            cfg.ReadConfig("Directories", "LibraryPath", ref libraryfilepath);
            if (Directory.Exists(libraryfilepath))
            {
                countpaths++;  // Countpaths = 1
                ACADConnector.WriteCADMessage("CADBP Master Libraary Path: \"" + libraryfilepath + "\"");
            }
            else
            {
                ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Unable to find Master Library Path: \"" + libraryfilepath + "\"");
            }

            tplpath = libraryfilepath + "\\Templates";
            if (Directory.Exists(tplpath))
            {
                countpaths++;  // Countpaths = 3
                ACADConnector.WriteCADMessage("CADBP Templates Path: \"" + tplpath + "\"");
            }
            else
            {
                ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Unable to find Templates Path: \"" + tplpath + "\"");
            }
            blkpath = libraryfilepath + "\\Blocks";
            if (Directory.Exists(blkpath))
            {
                countpaths++;  // Countpaths = 4
                ACADConnector.WriteCADMessage("CADBP Blocks Path: \"" + blkpath + "\"");
            }
            else
            {
                ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Unable to find Blocks Path: \"" + blkpath + "\"");
            }
            pdfpath = libraryfilepath + "\\Standards";
            if (Directory.Exists(pdfpath))
            {
                countpaths++;  // Countpaths = 5
                ACADConnector.WriteCADMessage("CADBP Standards Path: \"" + pdfpath + "\"");
            }
            else
            {
                ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Unable to find Standards Path: \"" + pdfpath + "\"");
            }
            dwgpath = libraryfilepath;
            if (Directory.Exists(dwgpath))
            {
                countpaths++;  // Countpaths = 6
                ACADConnector.WriteCADMessage("CADBP Drawings Path: \"" + dwgpath + "\"");
            }
            else
            {
                ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Unable to find Drawings Path: \"" + dwgpath + "\"");
            }
            layfile = libraryfilepath + "\\LayerStates\\Standard Layer Definitions.xml";
            if (File.Exists(layfile))
            {
                countpaths++;
                ACADConnector.WriteCADMessage("CADBP Layer File Path: \"" + layfile + "\"");
            }
            else
            {
                ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Unable to find Layer File Path: \"" + layfile + "\"");
            }

            if (countpaths != 6)
            {
                string iniFile = ACADConnector.AcadFindFile("CADTools.ini");
                ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Not all CADTOOLS library paths have been set.");
                ACADConnector.WriteCADMessage("CADBP               Please ensure the paths above exist and also");
                ACADConnector.WriteCADMessage("CADBP               review the CADTools.ini file found here...");
                ACADConnector.WriteCADMessage("CADBP               " + iniFile);

                string message = "Not all CADTOOLS library paths have been set.\nPlease review the errors in the output window.";
                string title = "***WARNING***";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning);
                return -1;
            }
            return 1;
        }
    }
}
