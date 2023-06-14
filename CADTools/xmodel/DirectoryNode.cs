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
    //! DirectoryNode class
    /*! 
        Instantiation class inheriting from DataNode for collection of file system Directory data.
        \param directoryInfo  DirectoryInfo The root folder from where to collect the data.
        \param modeltype  ModelType Enumeration of the data type to collect.
    */
    internal class DirectoryNode : DataNode
    {
        internal DirectoryInfo directoryInfo { get; set; }

        //! CADTreeNode constructor
        public DirectoryNode(DirectoryInfo di, Model.ModelType modeltype)
        {

            this.Text = di.Name;
            this.ToolTipText = di.FullName;
            this.directoryInfo = di;
            this.datatype = NodeType.directorynode;

            this.ImageIndex = 0;
            this.BackColor = Color.White;

            foreach (var directory in directoryInfo.GetDirectories())
            {
                DirectoryNode newnode = new DirectoryNode(directory, modeltype);
                this.Nodes.Add(newnode);
            }
            foreach (var file in directoryInfo.GetFiles())
            {
                if (file.Extension == Model.GetExtension(modeltype))
                {
                    if (modeltype == Model.ModelType.block)
                    {
                        var filenode = new BlockNode(file);
                        this.Nodes.Add(filenode);
                        filenode.ImageIndex = 1;
                    }
                    else
                    {
                        var filenode = new FileNode(file);
                        this.Nodes.Add(filenode);
                        filenode.ImageIndex = 1;
                    }
                }
                else if (file.Extension == Model.GetExtension(Model.ModelType.link))
                {
                    var filenode = new LinkNode(file);
                    this.Nodes.Add(filenode);
                    filenode.ImageIndex = 1;
                }
            }
        }
    }
}
