using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADTools
{
    #region  ========================================================= Develop directory file trees =========================================================
    abstract class DataNode : TreeNode
    {
        internal string datatype { get; set; }

        internal DataNode()
        {
            this.datatype = "DataNode";
        }
    }
    class FileNode : DataNode
    {
        internal FileInfo fileInfo { get; set; }

        internal FileNode(FileInfo fi)
        {
            this.Text = fi.Name;
            this.ToolTipText = fi.DirectoryName;
            this.fileInfo = fi;
            this.datatype = "FileNode";
        }
    }

    class BlockNode : DataNode
    {
        internal FileInfo fileInfo { get; set; }
        internal string layer { get; set; }
        internal string annotative { get; set; }
        internal string nonzeroinsert { get; set; }
        internal string modelorlayout { get; set; }
        internal string rotation { get; set; }
        internal string explode { get; set; }


        internal BlockNode(FileInfo fi)
        {
            this.Text = fi.Name;
            this.ToolTipText = fi.DirectoryName;
            this.fileInfo = fi;
            this.datatype = "BlockNode";

            this.layer = "";
            this.annotative = "";
            this.nonzeroinsert = "";
            this.modelorlayout = "";
            this.rotation = "";
            this.explode = "";
        }
    }

    class LinkNode : DataNode
    {
        internal FileInfo fileInfo { get; set; }

        internal LinkNode(FileInfo fi)
        {
            //this.Text = Path.GetFileNameWithoutExtension(fi.FullName) + MathUtilities.HexToChar("1F517");
            this.Text = Path.GetFileNameWithoutExtension(fi.FullName) + " " + MathUtilities.HexToChar("27A4");
            this.ToolTipText = fi.Name;
            this.fileInfo = fi;
            this.datatype = "LinkNode";
        }
    }

    class DirectoryNode : DataNode
    {
        public DirectoryInfo directoryInfo { get; set; }
        public DirectoryNode(DirectoryInfo di)
        {
            this.Text = di.Name;
            this.ToolTipText = di.FullName;
            this.directoryInfo = di;
            this.datatype = "DirectoryNode";
        }
    }
    #endregion
}
