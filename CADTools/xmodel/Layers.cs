using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CADTools
{
    public enum CADClassFileState
    {
        OK = 0,
        NoFile = 1,
        FileOpened = 2,
        Reading = 3,
        Writing = 4,
        Error = 5
    }
    public enum CADClassTreeState
    {
        OK = 0,
        NoNodes = 1,
        Error = 2
    }
    #region ========================================================= Layer manager classes =========================================================
    class Layers
    {
        public List<Layer> _layers = new List<Layer>();

        public void CreateLayers()
        {
#if UseACAD // Can only create layers inside the AutoCAD application
            foreach (Layer layer in this._layers)
            {
                if (layer.Checked == true) layer.CreateCADLayer();
            }
#endif
        }
        public void Insert(Layer layer)
        {
            Layer newlayer = new Layer();
            newlayer._grouping = layer._grouping;
            this._layers.Insert(this._layers.LastIndexOf(layer), newlayer);
        }
        public void Remove(Layer layer)
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
                                                Layer layer = new Layer(reader);
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
                foreach (Layer layer in this._layers)
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

            foreach (Layer layer in this._layers)
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
            Layer newlayer = new Layer();

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
                                    newlayer = new Layer();
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
                                    newlayer = new Layer();
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
    class Layer : DataNode
    {
        public String _line_type = "CONTINUOUS";
        public String _line_file = "";
        public String _line_weight = "Default";
        public String _plot_style = "Default";
        public Int16 _colour = 7;
        public String _grouping = "";
        public Boolean _on_off = false;
        public Boolean _frozen = false;
        public Boolean _locked = false;
        public Boolean _plot_state = true;

        internal Layer()
        {
            this.datatype = "LayerNode";
            this.Name = "New Layer";
            this._line_type = "CONTINUOUS";
            this._line_file = "";
            this._line_weight = "Default";
            this._plot_style = "Default";
            this._colour = 7;
            this._grouping = "NEW";
            this.Text = "New :    " + this.Name;
            this._on_off = false;
            this._frozen = false;
            this._locked = false;
            this._plot_state = true;
        }

        internal Layer(XmlTextReader reader)
        {
            String tmpTxt = "";
            this.datatype = "LayerNode";

            reader.MoveToContent();
            this.Name = reader.GetAttribute("name");
            this._line_type = reader.GetAttribute("line-type");
            this._line_file = reader.GetAttribute("line-file");
            this._line_weight = reader.GetAttribute("line-weight");
            this._plot_style = reader.GetAttribute("plot-style");
            tmpTxt = reader.GetAttribute("colour");
            this._colour = Int16.Parse(tmpTxt);
            this._grouping = reader.GetAttribute("description");
            String[] levels = this._grouping.Split(':');
            this.Text = levels[levels.Count() - 1] + ":    " + this.Name;

            if (reader.GetAttribute("on-off") == "True") this._on_off = true;
            else this._on_off = false;
            if (reader.GetAttribute("frozen") == "True") this._frozen = true;
            else this._frozen = false;
            if (reader.GetAttribute("plot-state") == "True") this._plot_state = true;
            else this._plot_state = false;
            if (reader.GetAttribute("locked") == "True") this._locked = true;
            else this._locked = false;

            this.ToolTipText = this.Name + ", " + this._line_type + ", " + this._line_file + ", " + this._line_weight + ", " + this._plot_style;

#if UseACAD // Can only use AutoCAD objects inside the AutoCAD application
            // Get the current document and database
            Document acDoc = AcadApp.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (var lck = acDoc.LockDocument())
            {
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Layer table for read
                    LayerTable acLyrTbl;
                    acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                    if (acTrans == null)
                        throw new ArgumentNullException("acTrans");

                    if (acLyrTbl == null)
                        throw new ArgumentNullException("acLyrTbl");

                    if (string.IsNullOrWhiteSpace(this.Name))
                        throw new ArgumentNullException("sLayerName");

                    SymbolUtilityServices.ValidateSymbolName(this.Name, false);

                    // Check if layer already in table
                    if (acLyrTbl.Has(this.Name))
                    {
                        this.ImageIndex = 3;
                        this.SelectedImageIndex = 3;
                    }
                    else
                    {
                        this.ImageIndex = 1;
                        this.SelectedImageIndex = 1;
                    }
                }
            }
#endif
        }


        public string CreateCADLayer()
        {
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (var lck = acDoc.LockDocument())
            {
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    Boolean Rollback = false;

                    // Open the Layer table for read
                    LayerTable acLyrTbl;
                    acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                    if (acTrans == null)
                        throw new ArgumentNullException("acTrans");

                    if (acLyrTbl == null)
                        throw new ArgumentNullException("acLyrTbl");

                    if (string.IsNullOrWhiteSpace(this.Name))
                        throw new ArgumentNullException("sLayerName");

                    SymbolUtilityServices.ValidateSymbolName(this.Name, false);

                    // Check if layer not already in table
                    if (!acLyrTbl.Has(this.Name))
                    {
                        // Otherwise create it
                        using (LayerTableRecord acLyrTblRec = new LayerTableRecord())
                        {

                            // Set name
                            acLyrTblRec.Name = this.Name;

                            // Set Lineweight
                            switch (this._line_weight)
                            {
                                case "LineWeight000":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight000;
                                    break;
                                case "LineWeight005":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight005;
                                    break;
                                case "LineWeight009":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight009;
                                    break;
                                case "LineWeight013":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight013;
                                    break;
                                case "LineWeight015":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight015;
                                    break;
                                case "LineWeight018":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight018;
                                    break;
                                case "LineWeight020":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight020;
                                    break;
                                case "LineWeight025":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight025;
                                    break;
                                case "LineWeight030":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight030;
                                    break;
                                case "LineWeight035":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight035;
                                    break;
                                case "LineWeight040":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight040;
                                    break;
                                case "LineWeight050":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight050;
                                    break;
                                case "LineWeight053":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight053;
                                    break;
                                case "LineWeight060":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight060;
                                    break;
                                case "LineWeight070":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight070;
                                    break;
                                case "LineWeight080":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight080;
                                    break;
                                case "LineWeight090":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight090;
                                    break;
                                case "LineWeight100":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight100;
                                    break;
                                case "LineWeight106":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight106;
                                    break;
                                case "LineWeight120":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight120;
                                    break;
                                case "LineWeight140":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight140;
                                    break;
                                case "LineWeight158":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight158;
                                    break;
                                case "LineWeight200":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight200;
                                    break;
                                case "LineWeight211":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.LineWeight211;
                                    break;
                                case "Default":
                                    acLyrTblRec.LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight.ByLineWeightDefault;
                                    break;

                            }


                            // Set Color
                            acLyrTblRec.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, this._colour);

                            // Set 

                            // Open the Layer table for read
                            LinetypeTable acLinTbl;
                            acLinTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                            if (acLinTbl.Has(this._line_type) == true)
                            {
                                // Set the linetype for the layer
                                acLyrTblRec.LinetypeObjectId = acLinTbl[this._line_type];
                            }
                            else
                            {
                                // TODO:  Add error checking for linestyle and file search
                                if (System.IO.File.Exists(this._line_file))
                                {
                                    acCurDb.LoadLineTypeFile(this._line_type, this._line_file);
                                    if (acLinTbl.Has(this._line_type) == true)
                                    {
                                        acLyrTblRec.LinetypeObjectId = acLinTbl[this._line_type];
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("**ERROR** Unable to locate linetype " + this._line_type + " from file " + this._line_file);
                                }
                            }

                            //=== Add data to LAyer Table
                            // Upgrade the Layer table for write if not write enabled
                            if (!acLyrTbl.IsWriteEnabled)
                                // if the transaction is a standard transaction, you should use:
                                acTrans.GetObject(acLyrTbl.ObjectId, OpenMode.ForWrite);
                            // instead of:
                            //acLyrTbl.UpgradeOpen();

                            // Append the new layer to the Layer table and the transaction
                            acLyrTbl.Add(acLyrTblRec);
                            acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
                        }
                    }

                    //Test of layer was created correctly
                    if (Rollback)
                    {
                        acTrans.Abort();
                    }
                    else
                    {
                        acTrans.Commit();
                        this.ImageIndex = 3;
                        this.SelectedImageIndex = 3;
                    }
                }
            }
            return this.Name;
        }

        internal void WriteXML(XmlWriter writer)
        {
            writer.WriteStartElement("Layer");
            writer.WriteAttributeString("name", this.Name);

            if (this._on_off == true)
                writer.WriteAttributeString("on_off", "True");
            else writer.WriteAttributeString("on_off", "False");
            if (this._frozen == true)
                writer.WriteAttributeString("frozen", "True");
            else writer.WriteAttributeString("frozen", "False");
            if (this._locked == true)
                writer.WriteAttributeString("locked", "True");
            else writer.WriteAttributeString("locked", "False");

            writer.WriteAttributeString("colour", this._colour.ToString());

            writer.WriteAttributeString("line-type", this._line_type);
            if (this._line_type != "CONTINUOUS") // If linetype is not CONTINUOUS then a line file is required
            {
                if (this._line_file == "")
                {
                    MessageBox.Show("A Linetype file name is required in Layer \n" + this._grouping);
                }
                else
                {
                    writer.WriteAttributeString("line-file", this._line_file);

                }
            }

            writer.WriteAttributeString("line-weight", this._line_weight);
            writer.WriteAttributeString("plot-style", this._plot_style);

            if (this._plot_state == true)
                writer.WriteAttributeString("plot-state", "True");
            else writer.WriteAttributeString("plot-state", "False");

            writer.WriteAttributeString("description", this._grouping);

            writer.WriteEndElement();
        }
    }
    #endregion
}
