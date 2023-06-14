using System.IO;

namespace CADTools.model
{
    //! CADTools FileNode class
    /*! 
        Instantiation class inheriting from DataNode for collection of file system File data.
        \param fileInfo  FileSystem FileInfo to store at node.
    */
    internal class FileNode : DataNode
    {
        //! fileInfo variable
        /*! 
            Instantiation class inheriting from DataNode for collection of file system data .
        */
        internal FileInfo fileInfo { get; set; }

        //! DataNode constructor
        public FileNode(FileInfo fi)
        {
            this.Text = fi.Name;
            this.ToolTipText = fi.DirectoryName;
            this.fileInfo = fi;
            this.datatype = NodeType.filenode;
        }
    }
}
