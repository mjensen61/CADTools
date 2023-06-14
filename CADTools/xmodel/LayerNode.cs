using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace CADTools.model
{
    //! CADClassFileState enumeration for the state of file to be read
    /*! 
        CADClassFileState.
    */
    public enum CADClassFileState
    {
        OK = 0,
        NoFile = 1,
        FileOpened = 2,
        Reading = 3,
        Writing = 4,
        Error = 5
    }
    //! CADClassTreeState enumeration for the state of tree to be constructed
    /*! 
        CADClassTreeState.
    */
    public enum CADClassTreeState
    {
        OK = 0,
        NoNodes = 1,
        Error = 2
    }

    //! LayerNode class
    /*! 
        LayerNode.
    */
    internal class LayerNode : DataNode
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

        internal LayerNode()
        {
            this.datatype = NodeType.layernode;
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

        internal LayerNode(XmlTextReader reader)
        {
            String tmpTxt = "";
            this.datatype = NodeType.layernode;

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
}
