using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CADTools
{
    public enum DataValidateCond
    {
        Valid = 0,
        Invalid = 1
    }

    public partial class LayerForm : Form
    {
        private String currfilename = "";
        private Layers layers = new Layers();
        Layer currlayer = null;
        private Boolean _table_edited = false;
        private Boolean _item_edited = false;
        private DataValidateCond _datavalidated = DataValidateCond.Invalid;

        //=======================================================================
        // FORM COMPONENT FUNCTIONS
        //=======================================================================
        public LayerForm()
        {
            InitializeComponent();
            currlayer = new Layer();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Opening file, Please wait!";
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.InitialDirectory = System.IO.Path.GetDirectoryName(currfilename);
            openDialog.Title = "Select A File";
            openDialog.Filter = "XML Files (*.xml)|*.xml";
            openDialog.ShowDialog();

            if (openDialog.FileName != "")
            {
                this.currfilename = openDialog.FileName;
                LoadFile();
                this.Text = "Layer Settings: " + currfilename;
            }
            toolStripStatusLabel1.Text = "File opened";
            toolStripStatusLabel2.Text = "Item Count: " + layers._layers.Count.ToString();
        }
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportFile();
            this._table_edited = true;
            button2.Visible = true;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Save table to file
            SaveFile();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Check if item edited and ask if need to apply.
            if (this._item_edited == true)
            {
                DialogResult retval = MessageBox.Show("Previous item has been amended.  Do you wish to Apply changes?",
                                                      "Item edited",
                                                      MessageBoxButtons.YesNo);
                if (retval == DialogResult.Yes)
                {
                    _datavalidated = Form1_ApplyItemChanges();
                }
            }
            // Transfer new selected entry from table to edit form if data is validated.
            if ((e.Node.Nodes.Count == 0) && (_datavalidated == DataValidateCond.Valid))
            {
                currlayer = (Layer)e.Node;
                Form1_LoadItem();
                this._datavalidated = DataValidateCond.Valid;
            }
        }
        private void dataChanged(object sender, EventArgs e)
        {
            // The selected item has been edited.
            this._item_edited = true;
            this._datavalidated = DataValidateCond.Invalid;
            button1.Visible = true;
            toolStripStatusLabel1.Text = "Data is edited";
            toolStripStatusLabel2.Text = "Item Count: " + layers._layers.Count.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Transfer edited item edit form to Layers table 
            _datavalidated = Form1_ApplyItemChanges();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Save table to file
            SaveFile();
        }
        //=======================================================================
        // DATA VALIDATION FUNCTIONS
        //=======================================================================

        public enum DataValidateCond
        {
            Valid = 0,
            Invalid = 1
        }
        private DataValidateCond Form1_ApplyItemChanges()
        {
            DataValidateCond retval = DataValidateCond.Valid;
            toolStripStatusLabel1.Text = "Applying Changes";

            // Transfer edited item edit form to Layers table
            currlayer.Name = textBox1.Text;
            currlayer._line_file = textBox2.Text;
            currlayer._line_weight = comboBox3.Text;
            currlayer._plot_style = textBox4.Text;
            currlayer._grouping = textBox5.Text;

            currlayer._line_type = comboBox1.Text;
            currlayer._colour = Int16.Parse(comboBox2.Text);

            currlayer._on_off = checkBox1.Checked;
            currlayer._frozen = checkBox2.Checked;
            currlayer._locked = checkBox3.Checked;
            currlayer._plot_state = checkBox4.Checked;

            if (currlayer._line_type != "CONTINUOUS") // If linetype is not CONTINUOUS then a line file is required
            {
                if (currlayer._line_file == "")
                {
                    toolStripStatusLabel1.Text = "There is an error in the data!";
                    MessageBox.Show("a Linetype file is required");
                    retval = DataValidateCond.Invalid;
                }
                else
                {
                    this._item_edited = false;
                    button1.Visible = false;
                    this._table_edited = true;
                    button2.Visible = true;
                }
            }
            else
            {
                this._item_edited = false;
                button1.Visible = false;
                this._table_edited = true;
                button2.Visible = true;
                toolStripStatusLabel1.Text = "Data validated and changes stored";
                toolStripStatusLabel2.Text = "Item Count: " + layers._layers.Count.ToString();
            }

            return retval;
        }
        //=======================================================================
        // AUXILIARY FUNCTIONS
        //=======================================================================
        private void Form1_LoadItem()
        {
            // Transfer new selected entry from table to edit form.
            textBox1.Text = currlayer.Name;
            textBox2.Text = currlayer._line_file;
            comboBox3.Text = currlayer._line_weight;
            textBox4.Text = currlayer._plot_style;
            textBox5.Text = currlayer._grouping;

            comboBox1.Text = currlayer._line_type;
            comboBox2.Text = currlayer._colour.ToString();

            checkBox1.Checked = currlayer._on_off;
            checkBox2.Checked = currlayer._frozen;
            checkBox3.Checked = currlayer._locked;
            checkBox4.Checked = currlayer._plot_state;
            this._item_edited = false;
            button1.Visible = false;
            toolStripStatusLabel1.Text = "Review Item";
            toolStripStatusLabel2.Text = "Item Count: " + layers._layers.Count.ToString();
        }
        private void LoadFile()
        {
            toolStripStatusLabel1.Text = "Loading File";

            DialogResult retval = MessageBox.Show("Do you wish to clear the layer tree?",
                                  "Table edited",
                                  MessageBoxButtons.YesNo);
            if (retval == DialogResult.Yes)
            {
                treeView1.Nodes.Clear();
                layers._layers.Clear();
            }

            layers.ReadFile(currfilename);

            if (layers.Populate(treeView1) == CADClassTreeState.OK)
            {
                this._datavalidated = DataValidateCond.Valid;
                this._table_edited = false;
                this._item_edited = false;
                button1.Visible = false;
                button2.Visible = false;
                this.Text = "Layer Settings: " + currfilename;
            }
            toolStripStatusLabel1.Text = "File Loaded";
            toolStripStatusLabel2.Text = "Item Count: " + layers._layers.Count.ToString();
        }
        private void ImportFile()
        {
            toolStripStatusLabel1.Text = "Importing File";
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.InitialDirectory = System.IO.Path.GetDirectoryName(currfilename);
            openDialog.Title = "Select A File";
            openDialog.Filter = "Layer manager files (*.las)|*.las";
            //"Drawing Templates (*.tpl)|*.tpl|"+
            //"AutoCAD Drawings (*.dwg)|*.dwg";
            openDialog.ShowDialog();

            if (openDialog.FileName != "")
            {
                DialogResult retval = MessageBox.Show("Do you wish to clear the layer tree?",
                                      "Table edited",
                                      MessageBoxButtons.YesNo);
                if (retval == DialogResult.Yes)
                {
                    treeView1.Nodes.Clear();
                    layers._layers.Clear();
                }

                layers.ImportLayers(openDialog.FileName);
                layers.Populate(treeView1);

                this._datavalidated = DataValidateCond.Valid;
                this._item_edited = false;
                button1.Visible = false;
            }
            toolStripStatusLabel1.Text = "File Imported";
            toolStripStatusLabel2.Text = "Item Count: " + layers._layers.Count.ToString();
        }
        private void SaveFile()
        {
            toolStripStatusLabel1.Text = "Saving File";

            // Save table to file
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.InitialDirectory = System.IO.Path.GetDirectoryName(currfilename);
            saveDialog.Title = "Select A File";
            saveDialog.Filter = "XML Files (*.xml)|*.xml";
            saveDialog.ShowDialog();

            if (saveDialog.FileName != "")
            {
                this.currfilename = saveDialog.FileName;
                if (layers.WriteXml(currfilename) == CADClassFileState.OK)
                {
                    this._datavalidated = DataValidateCond.Valid;
                    this._table_edited = false;
                    this._item_edited = false;
                    button1.Visible = false;
                    button2.Visible = false;
                }
            }
            toolStripStatusLabel1.Text = "File Saved";

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            toolStripStatusLabel1.Text = "Closing Form";

            // Check if item edited and ask if need to apply changes.
            if (this._item_edited == true)
            {
                DialogResult retval = MessageBox.Show("Previous item has been amended.  Do you wish to Apply changes?",
                                                      "Item edited",
                                                      MessageBoxButtons.YesNo);
                if (retval == DialogResult.Yes)
                {
                    _datavalidated = Form1_ApplyItemChanges();
                }
            }
            // Check if table edited and ask if need to save.
            if (this._table_edited == true)
            {
                DialogResult retval = MessageBox.Show("Layers have been amended.  Do you wish to Save changes?",
                                                      "Table edited",
                                                      MessageBoxButtons.YesNo);
                if (retval == DialogResult.Yes)
                {
                    SaveFile();
                }
            }
            // it table is currently being edited then cannot close
            if ((this._table_edited == true) || (this._item_edited == true))
            {
                DialogResult retval = MessageBox.Show("Changes have not been saved.  Do you wish to Close the form?",
                                                      "Warning!",
                                                      MessageBoxButtons.YesNo);
                if (retval == DialogResult.No)
                {
                    e.Cancel = true;
                    toolStripStatusLabel1.Text = "Error Closing Form";

                    return;
                }
            }
        }

        private void LayerForm_Load(object sender, EventArgs e)
        {

        }

        private void insertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode mynode = treeView1.SelectedNode;
            if (mynode == currlayer)
            {
                layers.Insert(currlayer);
            }
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode mynode = treeView1.SelectedNode;
            if (mynode == currlayer)
            {
                treeView1.SelectedNode = treeView1.SelectedNode.Parent;
                layers.Remove(currlayer);
                layers.Populate(treeView1);
                this._table_edited = true;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogResult dlgresult = MessageBox.Show("Are you sure wish to sort the layer tree?",
          "Sort Layers?",
          MessageBoxButtons.YesNo);
            if (dlgresult == DialogResult.Yes)
            {
                layers.Sort();
                layers.Populate(treeView1);
                this._table_edited = true;
            }
        }
    }
}
