using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CADTools.model
{
    //! LinkNode class
    /*! 
        Instantiation class inheriting from DataNode for collection of file system Link data.
        \param fileInfo  FileSystem FileInfo to store at node.
    */
    internal class LinkNode : DataNode
    {
        internal FileInfo fileInfo { get; set; }

        //! DataNode constructor
        public LinkNode(FileInfo fi)
        {
            //this.Text = Path.GetFileNameWithoutExtension(fi.FullName) + MathUtilities.HexToChar("1F517");
            this.Text = Path.GetFileNameWithoutExtension(fi.FullName) + " " + MathUtilities.HexToChar("27A4");
            this.ToolTipText = fi.Name;
            this.fileInfo = fi;
            this.datatype = NodeType.linknode;
        }
    }
}
