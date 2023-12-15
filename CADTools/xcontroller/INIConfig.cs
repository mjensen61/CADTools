using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CADTools
{
    public enum IniState
    {
        OK = 0,
        Error = 1
    }
    public class INIConfig
    {
       public string path;

        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                 string key, string def, StringBuilder retVal,
            int size, string filePath);
        public INIConfig(string Path)
        {
            path = Path;
        }

        public IniState IniWriteValue(string Section, string Key, string Value)
        {
            IniState retval = IniState.OK;

            WritePrivateProfileString(Section, Key, Value, this.path);

            return retval;
        }

        public IniState IniReadValue(string Section, string Key, ref string Value)
        {
            IniState retval = IniState.Error;

            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            if (i > 0) // Characters found
            {
                Value = temp.ToString();
                retval = IniState.OK;
            }

            return retval;

        }

        public IniState ReadConfig(string section, string key, ref string value)
        {
            IniState retVal = IniState.Error;
            string iniFile = ACADConnector.AcadFindFile("CADTools.ini");

            if (File.Exists(iniFile))
            {
                INIConfig ini = new INIConfig(iniFile);
                if (ini.IniReadValue(section, key, ref value) == IniState.OK)
                {
                    retVal = IniState.OK;
                }
            }
            else
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.Description = "Select location of Master CAD Library.";

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    String basefolder = folderBrowserDialog.SelectedPath;

                    MessageBox.Show(basefolder);

                    iniFile = "C:\\Uses\\mjensen3\\OneDrive-GHD\\GHD\\Autodesk\\ACAD User\\MAJCAD\\CADTools.ini";
                    INIConfig ini = new INIConfig(iniFile);

                    ini.IniWriteValue("CADTools", "Version", "1.00.00");
                    ini.IniWriteValue("Directories", "LibraryPath", basefolder);
                    ini.IniWriteValue("Directories", "LocalPath", "C:\\Uses\\mjensen3\\OneDrive-GHD\\GHD\\Autodesk\\ACAD User\\MAJCAD");
                    ini.IniWriteValue("Files", "layfile",
                        "C:\\Uses\\mjensen3\\OneDrive-GHD\\GHD\\Autodesk\\ACAD User\\MAJCAD\\LayerStates\\Standard Layer Definitions.xml");
                    //ini.IniWriteValue("Extentions", "TemplateExtention", ".dwt");
                    //ini.IniWriteValue("Extentions", "BlockExtention", ".dwg");
                    ini.IniWriteValue("SUPPORT PATHS", "PrinterStyleSheetPath", "PlotStyles");
                }
            }

            return retVal;
        }

        public static IniState WriteConfig(string section, string key, string value)
        {
            IniState retVal = IniState.OK;

            string basePath = System.Environment.CurrentDirectory;
            INIConfig ini = new INIConfig(basePath + "\\" + "CADTools.ini");
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);

            }
            ini.IniWriteValue(section, key, value);
            return retVal;
        }
    }
}