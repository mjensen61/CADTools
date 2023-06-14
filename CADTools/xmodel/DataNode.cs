using System.Windows.Forms;

namespace CADTools.model
{
    //! DataType enumeration for various node types
    /*! 
        DataType.
    */
    enum NodeType
    {
        unknown = 0,
        datanode = 1,
        filenode = 2,
        directorynode = 3,
        standardsnode = 4,
        blocknode = 5,
        linknode = 6,
        layernode = 7
    }
    //! CADTools DataNode class
    /*! 
        Abstract class inheriting from TreeNode for collection of file system data.
    */
    public abstract class DataNode : TreeNode
    {
        //!NodeType holds enumeration describing usage of the node.
        internal NodeType datatype = NodeType.unknown;
        internal NodeType DataType   //! DataType property defines type of the node.
        {
            set { datatype = value; }
            get { return datatype; }
        }

        //! DataNode constructor
        public DataNode()
        {
            datatype = NodeType.datanode;
        }
        //! Returns Dataode type as string
        public string GetNodeType()
        {
            switch (datatype)
            {
                case NodeType.datanode: return "TemplateNode";
                case NodeType.filenode: return "FileNode";
                case NodeType.directorynode: return "DirectoryNode";
                case NodeType.standardsnode: return "StandardsNode";
                case NodeType.blocknode: return "BlockNode";
                case NodeType.linknode: return "LinkNode";
                default: return "";
            }
        }
    }
}
