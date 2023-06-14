using System.IO;

namespace CADTools.model
{
    //! BlockNode class
    /*! 
        Instantiation class inheriting from DataNode for collection of file system Block data.
        \param fileInfo  FileSystem FileInfo to store at node.
    */
    internal class BlockNode : DataNode
    {
        internal FileInfo fileInfo { get; set; }
        internal string blockname { get; set; }
        internal string layer { get; set; }
        internal string annotative { get; set; }
        internal string nonzeroinsert { get; set; }
        internal string modelorlayout { get; set; }
        internal string rotation { get; set; }
        internal string explode { get; set; }


        //! CADTools DataNode constructor
        public BlockNode(FileInfo fi)
        {
            this.Text = fi.Name;
            this.ToolTipText = fi.DirectoryName;
            this.fileInfo = fi;
            this.datatype = NodeType.blocknode; ;

            this.layer = "";
            this.annotative = "";
            this.nonzeroinsert = "";
            this.modelorlayout = "";
            this.rotation = "";
            this.explode = "";
        }
    }
}
