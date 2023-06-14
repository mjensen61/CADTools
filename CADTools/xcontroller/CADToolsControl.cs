using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

// autocad stuff
using Autodesk.AutoCAD.ApplicationServices;

//using GsRenderMode = Autodesk.AutoCAD.GraphicsSystem.RenderMode;
using System.Diagnostics;

using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

using CADTools.model;

namespace CADTools
{
    public partial class CADToolsControl : UserControl
    {
        // GLOBALS
        Layers layers = new Layers();
        Model  model  = new Model();

        public CADToolsControl()
        {
            InitializeComponent(); // Windows generated code

            //--------------------------------------------------------------
            // Label assembly version, author and copyright on form.
            //--------------------------------------------------------------
            Assembly asm = Assembly.GetExecutingAssembly();
            object[] obj = asm.GetCustomAttributes(false);
            var appName = asm.GetName().Name;
            this.labelappname.Text = String.Format("{0}", appName);
            var appVersion = asm.GetName().Version;
            this.labelappversion.Text = String.Format("{0}", appVersion);
            this.label11.Text = String.Format("{0}", appVersion);
            var attribute = (GuidAttribute)asm.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            this.labelGuid.Text = attribute.Value;
            textBox1.Text = string.Format("{0}.{1}.{2} Rev {3} " + textBox1.Text,
                appVersion.Major, appVersion.Minor, appVersion.Build, appVersion.Revision);
            foreach (object o in obj)
            {
                if (o.GetType() == typeof(System.Reflection.AssemblyCompanyAttribute))
                {
                    AssemblyCompanyAttribute aca = (AssemblyCompanyAttribute)o;
                    this.labelapppublisher.Text = String.Format("{0}", aca.Company);
                }
                else if (o.GetType() == typeof(System.Reflection.AssemblyCopyrightAttribute))
                {
                    AssemblyCopyrightAttribute aca = (AssemblyCopyrightAttribute)o;
                    this.labelappcopyright.Text = String.Format("{0}", aca.Copyright);
                }
            }
            //--------------------------------------------------------------

            //--------------------------------------------------------------
            // Initialise configuration paths and load model.
            //--------------------------------------------------------------
            CADTools.model.Model model = new CADTools.model.Model();

            label1.Text = model.TplPath;
            label2.Text = model.BlkPath;
            label3.Text = model.LayFile;
            label3.Text = model.StandardsPath;

            ListDirectory(treeView1, Model.ModelType.template);
        }

        public bool ListDirectory(TreeView treeView, Model.ModelType modeltype)
        {
            treeView.Nodes.Clear();
            if (model.IsLoaded(modeltype))
            {
                Form alertform = new LoadingForm();
                alertform.Show();
                alertform.Refresh();

                TreeNode aRootNode = model.GetNode(modeltype);

                treeView.Nodes.Add(aRootNode);
                aRootNode.Expand();
                alertform.Hide();
                alertform.Close();

                return true;
            }
            else
            {
                return false;
            }
        }
        bool LoadLayers(TreeView treeView, String filename)
        {
            if (File.Exists(filename))
            {
                treeView.Nodes.Clear();
                layers._layers.Clear();

                Form alertform = new LoadingForm();
                alertform.Show();
                alertform.Refresh();

                layers.ReadFile(filename);
                layers.Populate(treeView3);

                alertform.Close();
                return true;
            }
            else
            {
                ACADConnector.WriteCADMessage("Unable to locate layer file: \"" + filename + "\"");
                return false;
            }
        }

        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode treeNode = treeView1.SelectedNode;
            DataNode dataNode = (DataNode)treeNode;

            string nodetype = dataNode.GetNodeType(); ;

            if (nodetype == "FileNode")
            {
                FileNode fileNode = (FileNode)treeNode;
                string fileName = fileNode.fileInfo.FullName;
                if (File.Exists(fileName))
                {
                    DocumentCollection acDocMgr = AcadApp.DocumentManager;
                    Document acDoc = acDocMgr.Add(fileName);
                    acDocMgr.MdiActiveDocument = acDoc;
                }
                else
                {
                    MessageBox.Show("Unable to locate " + fileName);
                }
            }
            if (nodetype == "LinkNode")
            {
                LinkNode linkNode = (LinkNode)treeNode;
                string linkName = linkNode.fileInfo.FullName;
                if (File.Exists(linkName))
                {
                    WinUtilities.OpenShortcut(linkName);
                }
                else
                {
                    MessageBox.Show("Unable to locate " + linkName);
                }
            }
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode treeNode = treeView2.SelectedNode;
            DataNode dataNode = (DataNode)treeNode;

            string nodetype = dataNode.GetNodeType();

            if (nodetype == "BlockNode")
            {
                BlockNode fileNode = (BlockNode)treeNode;
                string fileName = fileNode.fileInfo.FullName;

                if (File.Exists(fileName))
                {
                    ACADConnector.GetOuterDWGModelBitmap(fileName, pictureBox2);
                }
                else
                {
                    MessageBox.Show("Unable to locate " + fileName);
                }
            }
        }

        private void pictureBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var frm = new ImageForm(pictureBox2.BackgroundImage as System.Drawing.Bitmap);
            frm.ShowDialog();
        }

        private void treeView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode treeNode = treeView2.SelectedNode;
            DataNode dataNode = (DataNode)treeNode;

            string nodetype = dataNode.GetNodeType();

            if (nodetype == "BlockNode")
            {
                BlockNode fileNode = (BlockNode)treeNode;
                string fileName = fileNode.fileInfo.FullName;
                if (File.Exists(fileName))
                {
                    ACADConnector.InsertBlock(fileName);
                }
                else
                {
                    MessageBox.Show("Unable to locate " + fileName);
                }
            }
            if (nodetype == "LinkNode")
            {
                LinkNode linkNode = (LinkNode)treeNode;
                string linkName = linkNode.fileInfo.FullName;
                if (File.Exists(linkName))
                {
                    WinUtilities.OpenShortcut(linkName);
                }
                else
                {
                    MessageBox.Show("Unable to locate " + linkName);
                }
            }
        }
        private void treeView3_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

            if ((e.Node.ImageIndex == 1) || (e.Node.ImageIndex == 2))
            {
                DataNode clicked = (DataNode)e.Node;
                switch (clicked.ImageIndex)
                {
                    case 1:
                        clicked.ImageIndex = 2;
                        clicked.SelectedImageIndex = 2;
                        clicked.Checked = true;
                        break;
                    case 2:
                        clicked.ImageIndex = 1;
                        clicked.SelectedImageIndex = 1;
                        clicked.Checked = false;
                        break;
                    default:
                        clicked.Checked = false;
                        break;

                }
            }
        }
        private void treeView4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode treeNode = treeView4.SelectedNode;
            DataNode dataNode = (DataNode)treeNode;

            string datatype = dataNode.GetNodeType();

            if (datatype == "FileNode")
            {
                FileNode fileNode = (FileNode)treeNode;
                string fileName = fileNode.fileInfo.FullName;
                if (File.Exists(fileName))
                {
                    WinUtilities.OpenWithDefaultProgram(fileName);
                }
                else
                {
                    MessageBox.Show("Unable to locate " + fileName);
                }
            }
            if (datatype == "LinkNode")
            {
                LinkNode linkNode = (LinkNode)treeNode;
                string linkName = linkNode.fileInfo.FullName;
                if (File.Exists(linkName))
                {
                    WinUtilities.OpenShortcut(linkName);
                }
                else
                {
                    MessageBox.Show("Unable to locate " + linkName);
                }
            }

        }
        
        private void button1_Click_1(object sender, EventArgs e)
        {
            layers.CreateLayers();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            CADTools.LayerForm layerform = new LayerForm();
            layerform.ShowDialog();
            layerform.Dispose();
            this.LoadLayers(treeView3, model.LayFile);
        }

        private void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
            ACADConnector.OpenHTTPS("Author", "https://github.com/mjensen61/CADTools");
        }

        private void button4_MouseUp(object sender, MouseEventArgs e)
        {
            ACADConnector.OpenHTTPS("License", "https://creativecommons.org/licenses/by-nd/4.0/legalcode");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ACADConnector.OpenPDF("Drawing Office", model.LibraryFilePath +"\\Standards\\CADToolsHelp.pdf");
        }

        // Demand loading of templates on first use
        private void tabPage1_Enter(object sender, EventArgs e)
        {
            if (model.TemplatesLoaded == false)
            {
                ListDirectory(treeView1, Model.ModelType.template);
            }
        }

        // Demand loading of blocks on first use
        private void tabPage2_Enter(object sender, EventArgs e)
        {
            if (model.BlocksLoaded == false)
            {
                ListDirectory(treeView2, Model.ModelType.block);
            }
        }

        // Demand loading of layers on first use
        private void tabPage3_Enter(object sender, EventArgs e)
        {
            if (model.LayersLoaded == false)
            {
                LoadLayers(treeView3, model.LayFile);
            }
        }

        // Demand loading of standards on first use
        private void tabPage4_Enter(object sender, EventArgs e)
        {
            if (model.StandardsLoaded == false)
            {
                ListDirectory(treeView4, Model.ModelType.standard);
            }
        }

        // Reload templates
        private void button5_Click(object sender, EventArgs e)
        {
            ListDirectory(treeView1, Model.ModelType.template);
        }

        // Reload blocks

        // Reload layers
        private void button3_Click(object sender, EventArgs e)
        {
            LoadLayers(treeView3, model.LayFile);
        }

        // Reload standards
        private void button6_Click(object sender, EventArgs e)
        {
            ListDirectory(treeView2, Model.ModelType.block);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ListDirectory(treeView4, Model.ModelType.standard);
        }

        private void CADToolsControl_SizeChanged(object sender, EventArgs e)
        {
            if (this.Width < 200) this.Width = 200;
            if (this.Height < 300) this.Height = 300;
        }
    }
}
