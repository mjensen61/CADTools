using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CADTools.model
{
    //! CADTools Model class
    /*! 
        Model class provides the mapping of the data to the file system.
    */
    public class Model
    {
        #region Class Variable Declaration
        //! Path to root of Libray filesystem
        private string libraryfilepath = "";
        //! LibraryFilePath property
        public string LibraryFilePath   
        {
            //set { libraryfilepath = value; }
            get { return libraryfilepath; }
        }
        //! Path to root of local file system (Depricated)
        /*! 
            Currently in the process of removing this folder from the application.
        */
        private string localfilepath = "";
        //! LocalFilepath property [get]
        public string LocalFilepath   
        {
            get { return localfilepath; }
        }
        //! Path to root of Templates library
        private string tplpath = "";
        //! TplPath property [get]
        public string TplPath   
        {
            get { return tplpath; }
        }
        //! Path to root of Blocks Library
        private string blkpath = "";             
        //! BlkPath property [get]
        public string BlkPath   
        {
            get { return blkpath; }
        }
        //! Path to root of PDF Standards Library
        private string standardspath = "";
        //! PdfPath property [get]
        public string StandardsPath   
        {
            get { return standardspath; }
        }
        //! Path to root
        public string dwgpath = "";
        //! DwgPath property [get]
        //public string DwgPath   
        //{
            //set { dwgpath = value; }
        //    get { return dwgpath; }
        //}
        //! Path to Layer Standards file (XML file)
        private string layfile = "";
        //! LayFile property [get]
        public string LayFile   
        {
            get { return layfile; }
        }

        //! ModelState enumeration for various model states
        /*! 
            Model class provides the mapping of the data to the file system.
        */
        [Flags]
        private enum ModelState
        {
            None            = 0,
            libraryfound    = 1 << 1,
            templatesloaded = 1 << 2,
            blocksloaded    = 1 << 3,
            standardsloaded = 1 << 4,
            layersloaded    = 1 << 5
        }

        //! ModelState enumeration for various model types
        /*! 
            Model types provides the mapping of the data to the file types.
        */
        public enum ModelType
        {
            None = 0,
            template = 1,
            block = 2,
            standard = 3,
            link = 4,
            layer = 5
        }
        public static string GetExtension(ModelType mdtype)
        {
            switch (mdtype)
            {
                case ModelType.template: return ".tpl";
                case ModelType.block: return ".dwg";
                case ModelType.standard: return ".pdf";
                case ModelType.link: return ".lnk";
                case ModelType.layer: return "";
                default: return "";
            }
        }
        public Boolean IsLoaded(ModelType mdtype)
        {
            switch (mdtype)
            {
                case ModelType.template: return TemplatesLoaded;
                case ModelType.block: return BlocksLoaded;
                case ModelType.standard: return StandardsLoaded;
                case ModelType.layer: return LayersLoaded;
                default: return false;
            }
        }

        //! # ModelState enumeration for various model states
        /*! 
            Model class provides status the mapping of the data to the file system.  The integer contains enumerated bitwise flagsas set by the modelstate enumeration.
        */
        private ModelState modelState = 0; //! Flag to enumerate model load state
        public Boolean TemplatesLoaded   //! TemplatesLoaded property defines that the Templates folders have been loaded.
        {
            get { return modelState.HasFlag(ModelState.templatesloaded); }
        }
        public Boolean BlocksLoaded   //! BlocksLoaded property defines that the Blocks folders have been loaded.
        {
            get { return modelState.HasFlag(ModelState.templatesloaded); }
        }
        public Boolean LayersLoaded   //!LayersLoaded property defines that the Layers folders have been loaded.
        {
            get { return modelState.HasFlag(ModelState.templatesloaded); }
        }
        public Boolean StandardsLoaded   //! StandardsLoaded property defines that the Standards folders have been loaded.
        {
            get { return modelState.HasFlag(ModelState.templatesloaded); }
        }

        //! TreeNode templatesNode: Root node for Templates tree
            /*!

            */
        private TreeNode templatesNode;

        //! TreeNode blocksNode: Root node for Templates tree
        /*!
            
        */
        private TreeNode blocksNode;

        //! TreeNode LayNode: Root node for Layer tree
        /*!
            
        */
        private TreeNode layNode;

        //! TreeNode standardsNode: Root node for standards tree
        /*!
            
        */
         TreeNode standardsNode;

        #endregion

        //! Model constructor
        /*!
            \param  No paramaters.
            \return 1 = sucess, -1 = failure.
        */
        public Model()
        {
            InitializePaths();
        }
        //! Method to read the Library paths from the CADTools config file and set the required paths
        /*!
            \param  No paramaters.
            \return 1 = sucess, -1 = failure.
        */
        public TreeNode GetNode(ModelType modeltype)
        {
            switch(modeltype)
            {
                case ModelType.template: return templatesNode;
                case ModelType.block: return blocksNode;
                case ModelType.layer: return layNode;
                case ModelType.standard: return standardsNode;
                default: return null;
            }    
        }
        //! Method to read the Library paths from the CADTools config file and set the required paths
        /*!
            \param  No paramaters.
            \return 1 = sucess, -1 = failure.
        */
        private int InitializePaths()
        {
            // Initialize library paths
            libraryfilepath = "";

            INIConfig cfg = new INIConfig("");

            cfg.ReadConfig("Directories", "LibraryPath", ref libraryfilepath);
            if (Directory.Exists(libraryfilepath))
            {
                modelState = modelState | ModelState.libraryfound;

                ACADConnector.WriteCADMessage("CADBP Master Libraary Path: \"" + libraryfilepath + "\"");
            }
            else
            {
                ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Unable to find Master Library Path: \"" + libraryfilepath + "\"");
            }

            tplpath = libraryfilepath + "\\Templates";
            if (Directory.Exists(tplpath))
            {
                modelState = modelState | ModelState.templatesloaded;
                var rootDirectoryInfo = new DirectoryInfo(tplpath);
                templatesNode = new CADTools.model.DirectoryNode(rootDirectoryInfo, ModelType.template);
                ACADConnector.WriteCADMessage("CADBP Templates Path: \"" + tplpath + "\"");
            }
            else
            {
                ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Unable to find Templates Path: \"" + tplpath + "\"");
            }
            blkpath = libraryfilepath + "\\Blocks";
            if (Directory.Exists(blkpath))
            {
                modelState = modelState | ModelState.blocksloaded;
                var rootDirectoryInfo = new DirectoryInfo(blkpath);
                blocksNode = new CADTools.model.DirectoryNode(rootDirectoryInfo, ModelType.block);

                ACADConnector.WriteCADMessage("CADBP Blocks Path: \"" + blkpath + "\"");
            }
            else
            {
                ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Unable to find Blocks Path: \"" + blkpath + "\"");
            }
            standardspath = libraryfilepath + "\\Standards";
            if (Directory.Exists(standardspath))
            {
                modelState = modelState | ModelState.standardsloaded;
                var rootDirectoryInfo = new DirectoryInfo(standardspath);
                standardsNode = new CADTools.model.DirectoryNode(rootDirectoryInfo, ModelType.template);
                ACADConnector.WriteCADMessage("CADBP Standards Path: \"" + standardspath + "\"");
            }
            else
            {
                ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Unable to find Standards Path: \"" + standardspath + "\"");
            }
            //dwgpath = libraryfilepath;
            //if (Directory.Exists(dwgpath))
            //{
            //    countpaths++;  // Countpaths = 6
            //    ACADConnector.WriteCADMessage("CADBP Drawings Path: \"" + dwgpath + "\"");
            //}
            //else
            //{
            //    ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Unable to find Drawings Path: \"" + dwgpath + "\"");
            //}
            layfile = libraryfilepath + "\\LayerStates\\Standard Layer Definitions.xml";
            if (File.Exists(layfile))
            {
                modelState = modelState | ModelState.layersloaded;
                var rootDirectoryInfo = new DirectoryInfo(standardspath);
                layNode = new CADTools.model.DirectoryNode(rootDirectoryInfo, ModelType.layer);
                ACADConnector.WriteCADMessage("CADBP Layer File Path: \"" + layfile + "\"");
            }
            else
            {
                ACADConnector.WriteCADMessage("CADBP   ***ERROR*** Unable to find Layer File Path: \"" + layfile + "\"");
            }

            if ((int)modelState != 31)
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
                return (int)modelState;
            }
            return -1;
        }
        //! Method to read the Library paths from the CADTools config file and set the required paths
        /*!
            \param  No paramaters.
            \return 1 = sucess, -1 = failure.
        */

    }
}
