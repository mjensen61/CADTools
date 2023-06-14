using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace CADTools.model
{
    internal class Layers
    {
        //! Layers class
        /*! 
            Layers.
        */
            public List<LayerNode> _layers = new List<LayerNode>();

            public void CreateLayers()
            {
                foreach (LayerNode layer in this._layers)
                {
                    if (layer.Checked == true) layer.CreateCADLayer();
                }
            }
            public void Insert(LayerNode layer)
            {
                LayerNode newlayer = new LayerNode();
                newlayer._grouping = layer._grouping;
                this._layers.Insert(this._layers.LastIndexOf(layer), newlayer);
            }
            public void Remove(LayerNode layer)
            {
                this._layers.Remove(layer);
            }
            public void Sort()
            {
                _layers = _layers.OrderBy(x => x.Name).ToList();
                _layers = _layers.OrderBy(x => x._grouping).ToList();
            }

            public CADClassFileState ReadFile(String filename)
            {
                CADClassFileState retval = CADClassFileState.Error;
                XmlTextReader reader = null;

                try
                {
                    // Load the reader with the data file and ignore all white space nodes.
                    reader = new XmlTextReader(filename);
                    reader.WhitespaceHandling = WhitespaceHandling.None;
                    retval = CADClassFileState.FileOpened;

                    // Parse the file and display each of the nodes.
                    while (reader.Read())
                    {
                        retval = CADClassFileState.Reading;
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Name == "Layers")
                                {
                                    while (reader.Read())
                                    {
                                        switch (reader.NodeType)
                                        {
                                            case XmlNodeType.Element:
                                                if (reader.Name == "Layer")
                                                {
                                                    LayerNode layer = new LayerNode(reader);
                                                    _layers.Add(layer);
                                                }
                                                break;
                                            case XmlNodeType.EndElement:
                                                break;
                                        }
                                    }
                                }
                                break;
                            case XmlNodeType.EndElement:
                                MessageBox.Show(String.Format("</{0}>", reader.Name));
                                break;
                        }
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                    reader = null;
                    retval = CADClassFileState.OK;
                }
                return retval;
            }
            public CADClassFileState WriteXml(String newfilename)
            {
                CADClassFileState retval = CADClassFileState.Error;
                XmlWriter writer = null;

                try
                {
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
                    {
                        Indent = true,
                        IndentChars = "  ",
                        NewLineOnAttributes = false
                    };

                    // Load the reader with the data file and ignore all white space nodes.
                    writer = XmlWriter.Create(newfilename, xmlWriterSettings);
                    retval = CADClassFileState.FileOpened;

                    writer.WriteStartElement("Layers");
                    foreach (LayerNode layer in this._layers)
                    {
                        retval = CADClassFileState.Writing;
                        layer.WriteXML(writer);
                    }
                    writer.WriteEndElement();
                }
                finally
                {
                    if (writer != null)
                        writer.Close();
                    writer = null;
                    retval = CADClassFileState.OK;
                }
                return retval;
            }
            public CADClassTreeState Populate(TreeView treeView)
            {
                CADClassTreeState retval = CADClassTreeState.Error;
                treeView.Nodes.Clear();
                retval = CADClassTreeState.NoNodes;

                foreach (LayerNode layer in this._layers)
                {
                    String[] levels = layer._grouping.Split(':');

                    TreeNode level1 = new TreeNode(); // look for existing level1 branch in level0 branch
                    if (levels.Count() == 1)          // Only one level so add data node to the root of the treeview
                    {
                        treeView.Nodes.Add(layer);
                        retval = CADClassTreeState.OK;
                    }
                    else
                    if (levels.Count() > 1)           // has at least two levels
                    {
                        if (GetTreeNode(levels[0], treeView, ref level1) == CADClassNodeState.NotFound)             // if it doesn't exist then create it as a new level1 branch
                        {
                            level1 = new TreeNode(levels[0]);
                            level1.Name = levels[0];
                            level1.ImageIndex = 4;
                            level1.SelectedImageIndex = 4;
                            level1.ToolTipText = "ROOT NODE";
                            treeView.Nodes.Add(level1); // then create it and add it to the treeview
                        }

                        if (levels.Count() == 2) // Only two levels so add data node to the level1 branch
                        {
                            level1.Nodes.Add(layer);
                            retval = CADClassTreeState.OK;
                        }
                        else
                        if (levels.Count() > 2) // has at least three levels
                        {
                            TreeNode level2 = new TreeNode(); // look for existing level2 branch in level1 branch
                            if (GetNode(levels[1], level1, ref level2) == CADClassNodeState.NotFound)             // if it doesn't exist then create it as a new level2 branch
                            {
                                level2 = new TreeNode(levels[1]);
                                level2.Name = levels[1];
                                level2.ImageIndex = 4;
                                level2.SelectedImageIndex = 4;
                                level2.ToolTipText = layer._grouping;
                                level1.Nodes.Add(level2); // then create it and add it to the level1 branch
                            }

                            if (levels.Count() == 3) // Only three levels so add data node to the level2 branch
                            {
                                level2.Nodes.Add(layer);
                                retval = CADClassTreeState.OK;
                            }
                            else
                            if (levels.Count() > 3) // has at least four levels
                            {
                                TreeNode level3 = new TreeNode();// look for existing level3 node in level2
                                if (GetNode(levels[2], level2, ref level3) == CADClassNodeState.NotFound)             // if it doesn't exist exist then create it as a new level3 branch
                                {
                                    level3 = new TreeNode(levels[2]);
                                    level3.Name = levels[2];
                                    level3.ImageIndex = 4;
                                    level3.SelectedImageIndex = 4;
                                    level3.ToolTipText = layer._grouping;
                                    level2.Nodes.Add(level3); // then create it and add it to the level2 branch
                                }

                                if (levels.Count() == 4) // Only four levels so add data node to the level3 branch
                                {
                                    level3.Nodes.Add(layer);
                                    retval = CADClassTreeState.OK;
                                }
                                else
                                if (levels.Count() > 4) // has at least five levels
                                {
                                    TreeNode level4 = new TreeNode();// look for existing level4 node in level3  
                                    if (GetNode(levels[3], level3, ref level4) == CADClassNodeState.NotFound)             // if it doesn't exist exist then create it as a new level4 branch
                                    {
                                        level4 = new TreeNode(levels[3]);
                                        level4.Name = levels[3];
                                        level4.ImageIndex = 4;
                                        level4.SelectedImageIndex = 4;
                                        level4.ToolTipText = layer._grouping;
                                        level3.Nodes.Add(level4); // then create it and add it to the level3 branch
                                    }

                                    if (levels.Count() == 5) // Only five levels so add data node to the level4 branch
                                    {
                                        level4.Nodes.Add(layer);
                                        retval = CADClassTreeState.OK;
                                    }
                                    else
                                    {
                                        level4.Nodes.Add(layer); // We won't delve any deeper here.
                                        retval = CADClassTreeState.OK;
                                    }
                                }
                            }
                        }
                    }
                }
                return retval;
            }
            public enum DXFReaderState
            {
                None = 0,
                LAYERSTATEDICTIONARY = 1,
                LAYERSTATE = 2,
                LAYER = 3,
                Error = 4
            }
            public CADClassFileState ImportLayers(String filename)
            {
                CADClassFileState retval = CADClassFileState.Error;
                DXFReaderState readerstate = DXFReaderState.None;
                LayerNode newlayer = new LayerNode();

                String dxfCode = "";
                String dxfData = "";
                char[] charsToTrim = { ' ' };

                String branchValue = Path.GetFileNameWithoutExtension(filename);

                using (StreamReader sr = new StreamReader(filename))
                {
                    while (!sr.EndOfStream)
                    {
                        dxfCode = sr.ReadLine();
                        dxfCode = dxfCode.Trim(charsToTrim);
                        if (!sr.EndOfStream)
                        {
                            dxfData = sr.ReadLine();
                            dxfData = dxfData.Trim(charsToTrim);
                        }
                        switch (readerstate)
                        {
                            case DXFReaderState.None:
                                switch (dxfData)
                                {
                                    case "LAYERSTATEDICTIONARY": // start a new dictionary
                                        readerstate = DXFReaderState.LAYERSTATEDICTIONARY;
                                        break;
                                    default: // invalid state.  Shouldn't occur.
                                        break;
                                }// finish case DXFReaderState.None
                                break;
                            case DXFReaderState.LAYERSTATEDICTIONARY:
                                switch (dxfData)
                                {
                                    case "LAYERSTATE":
                                        readerstate = DXFReaderState.LAYERSTATE;
                                        break;

                                    default: // invalid state.  Shouldn't occur.
                                        break;
                                }// finish case DXFReaderState.LAYERSTATEDICTIONARY
                                break;
                            case DXFReaderState.LAYERSTATE:
                                switch (dxfCode)
                                {
                                    case "8": // start a new layer
                                        readerstate = DXFReaderState.LAYER;
                                        //***********************************
                                        // Start a new layer
                                        //***********************************
                                        newlayer = new LayerNode();
                                        newlayer.Name = dxfData;
                                        break;

                                    default: // invalid state.  Shouldn't occur.
                                        break;
                                }// finish case DXFReaderState.LAYERSTATE
                                break;
                            case DXFReaderState.LAYER:
                                switch (dxfCode)
                                {
                                    case "8": // start a new layer
                                              //***********************************
                                              // Save the previous layer if it exists
                                              //***********************************
                                        newlayer.Text = newlayer.Name;
                                        newlayer._grouping = branchValue + " : " + newlayer.Name;
                                        if (newlayer._line_file == "") newlayer._line_file = "unknown.lin";
                                        if (newlayer._line_type == "CONTINUOUS") newlayer._line_file = "";
                                        newlayer.ToolTipText = newlayer.Name + ", " + newlayer._line_type + ", " + newlayer._line_file + ", " + newlayer._line_weight + ", " + newlayer._plot_style;
                                        _layers.Add(newlayer);
                                        //***********************************
                                        // Start a new layer
                                        //***********************************
                                        newlayer = new LayerNode();
                                        newlayer.Name = dxfData;

                                        break;
                                    case "62":  // Store Colour
                                        newlayer._colour = Int16.Parse(dxfData);
                                        break;
                                    case "6":   // Store Line style
                                        if (dxfData.ToUpper() == "CONTINUOUS") dxfData = dxfData.ToUpper(); // Store continuous as uppercase
                                        newlayer._line_type = dxfData;
                                        break;
                                    case "370": // Store Line weight
                                        Int16 dxfInt = Int16.Parse(dxfData);
                                        if (dxfInt == -3)
                                        {
                                            newlayer._line_weight = "Default";
                                        }
                                        else if (dxfInt == 0)
                                        {
                                            newlayer._line_weight = "LineWeight000";
                                        }
                                        else if ((dxfInt > 0) && (dxfInt < 10))
                                        {
                                            newlayer._line_weight = "LineWeight00" + dxfInt.ToString();
                                        }
                                        else if (dxfInt < 100)
                                        {
                                            newlayer._line_weight = "LineWeight0" + dxfInt.ToString();
                                        }
                                        else if (dxfInt < 100)
                                        {
                                            newlayer._line_weight = "LineWeight" + dxfInt.ToString();
                                        }
                                        break;
                                    case "2":   // plot style
                                        newlayer._plot_style = dxfData;
                                        break;
                                    case "90":  // layer states
                                        if ((Int16.Parse(dxfData) & 1) == 1) newlayer._on_off = true;
                                        if ((Int16.Parse(dxfData) & 2) == 2) newlayer._frozen = true;
                                        if ((Int16.Parse(dxfData) & 4) == 4) newlayer._locked = true;
                                        if ((Int16.Parse(dxfData) & 8) == 8) newlayer._plot_state = false;
                                        break;
                                }// finish case DXFReaderState.LAYER
                                break;
                        }// finish switch readerstate
                    }// end of stream found

                }// end streamreader
                DialogResult dlgresult = MessageBox.Show("Do you wish to sort the layer tree?",
                          "Sort Layers?",
                          MessageBoxButtons.YesNo);
                if (dlgresult == DialogResult.Yes)
                {
                    _layers.Sort();
                }
                return retval;
            }
            public enum CADClassNodeState
            {
                OK = 0,
                Found = 1,
                NotFound = 2,
                Error = 3
            }
            private CADClassNodeState GetTreeNode(string name, TreeView treeView, ref TreeNode treeNode)
            {
                CADClassNodeState retval = CADClassNodeState.Error;

                foreach (TreeNode node in treeView.Nodes)
                {
                    if (node.Text.Equals(name))
                    {
                        treeNode = node;
                        retval = CADClassNodeState.Found;
                        return retval;
                    }
                    TreeNode next = null;
                    retval = GetNode(name, node, ref next);
                    if (next != null)
                    {
                        treeNode = next;
                        retval = CADClassNodeState.Found;
                        return retval;
                    }
                }
                retval = CADClassNodeState.NotFound;
                return retval;
            }

        private CADClassNodeState GetNode(string name, TreeNode rootNode, ref TreeNode treeNode)
        {
            CADClassNodeState retval = CADClassNodeState.Error;

            foreach (TreeNode node in rootNode.Nodes)
            {
                if (node.Name.Equals(name))
                {
                    treeNode = node;
                    retval = CADClassNodeState.Found;
                    return retval;
                }
                TreeNode next = null;
                retval = GetNode(name, node, ref next);
                if (next != null)
                {
                    treeNode = next;
                    retval = CADClassNodeState.Found;
                    return retval;
                }
            }
            retval = CADClassNodeState.NotFound;
            return retval;
        }
    }
}
