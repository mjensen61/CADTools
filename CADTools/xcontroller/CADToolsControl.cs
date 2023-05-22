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

            model.InitializePaths();

            label1.Text = model.tplpath;
            label2.Text = model.blkpath;
            label3.Text = model.layfile;
            label3.Text = model.pdfpath;

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

            ListDirectory(treeView1, model.tplpath, model.tplextension);
            model.templatesLoaded = true;
        }

        public void ListDirectory(TreeView treeView, string path, string extension)
        {
            treeView.Nodes.Clear();

            Form alertform = new LoadingForm();
            alertform.Show();
            alertform.Refresh();

            var rootDirectoryInfo = new DirectoryInfo(path);
            TreeNode aRootNode = CreateDirectoryTree(rootDirectoryInfo, extension);

            treeView.Nodes.Add(aRootNode);
            aRootNode.Expand();
            aRootNode.ToolTipText = path;
            alertform.Hide();
            alertform.Close();
        }
        void LoadLayers(TreeView treeView, String filename)
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
            }
            else
                ACADConnector.WriteCADMessage("Unable to locate layer file: \"" + filename + "\"");
        }

        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode treeNode = treeView1.SelectedNode;
            DataNode dataNode = (DataNode)treeNode;

            string datatype = dataNode.datatype;

            if (datatype == "FileNode")
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

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode treeNode = treeView2.SelectedNode;
            DataNode dataNode = (DataNode)treeNode;

            string datatype = dataNode.datatype;

            if (datatype == "BlockNode")
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

            string datatype = dataNode.datatype;

            if (datatype == "BlockNode")
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

            string datatype = dataNode.datatype;

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
        public TreeNode CreateDirectoryTree(DirectoryInfo directoryInfo, string extension)
        {
            var directoryNode = new DirectoryNode(directoryInfo);
            directoryNode.ImageIndex = 0;
            directoryNode.BackColor = Color.White;
            directoryNode.directoryInfo = directoryInfo;

            foreach (var directory in directoryInfo.GetDirectories())
            {
                directoryNode.Nodes.Add(CreateDirectoryTree(directory, extension));
            }
            foreach (var file in directoryInfo.GetFiles())
            {
                if (file.Extension == extension)
                {
                    if (file.Extension == model.blkextension) // Item is a dwg file
                    {
                        var filenode = new BlockNode(file);
                        directoryNode.Nodes.Add(filenode);
                        filenode.ImageIndex = 1;
                    }
                    else
                    {
                        var filenode = new FileNode(file);
                        directoryNode.Nodes.Add(filenode);
                        filenode.ImageIndex = 1;
                    }
                }
                else if (file.Extension == model.lnkextension)
                {
                    var filenode = new LinkNode(file);
                    directoryNode.Nodes.Add(filenode);
                    filenode.ImageIndex = 1;
                }
            }
            return directoryNode;
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
            this.LoadLayers(treeView3, model.layfile);
        }

        private void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
            ACADConnector.OpenHTTPS("Author", "https://github.com/mjensen61/LibrarySync-executable");
        }

        private void button4_MouseUp(object sender, MouseEventArgs e)
        {
            ACADConnector.OpenHTTPS("License", "https://creativecommons.org/licenses/by-nd/4.0/legalcode");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ACADConnector.OpenPDF("Drawing Office", "C:/ProgramData/Autodesk/ApplicationPlugins/zzCalibreCADCust.bundle/Contents/Help/CalibreCADHelp.pdf");
        }

        // Demand loading of templates on first use
        private void tabPage1_Enter(object sender, EventArgs e)
        {
            if (model.templatesLoaded == false)
            {
                ListDirectory(treeView1, model.tplpath, model.tplextension);
                model.templatesLoaded = true;
            }
        }

        // Demand loading of blocks on first use
        private void tabPage2_Enter(object sender, EventArgs e)
        {
            if (model.blocksLoaded == false)
            {
                ListDirectory(treeView2, model.blkpath, model.blkextension);
                model.blocksLoaded = true;
            }
        }

        // Demand loading of layers on first use
        private void tabPage3_Enter(object sender, EventArgs e)
        {
            if (model.layersLoaded == false)
            {
                LoadLayers(treeView3, model.layfile);
                model.layersLoaded = true;
            }
        }

        // Demand loading of standards on first use
        private void tabPage4_Enter(object sender, EventArgs e)
        {
            if (model.standardsloaded == false)
            {
                ListDirectory(treeView4, model.pdfpath, model.pdfextension);
                model.standardsloaded = true;
            }
        }

        // Reload templates
        private void button5_Click(object sender, EventArgs e)
        {
            ListDirectory(treeView1, model.tplpath, model.tplextension);
            model.templatesLoaded = true;
        }

        // Reload blocks
        private void tabPage1_Click(object sender, EventArgs e)
        {
            ListDirectory(treeView2, model.blkpath, model.blkextension);
            model.blocksLoaded = true;
        }

        // Reload layers
        private void button3_Click(object sender, EventArgs e)
        {
            this.LoadLayers(treeView3, model.layfile);
        }

        // Reload standards
        private void button6_Click(object sender, EventArgs e)
        {
            ListDirectory(treeView4, model.pdfpath, model.pdfextension);
            model.standardsloaded = true;
        }

        private void CADToolsControl_Resize(object sender, EventArgs e)
        {

        }

        private void CADToolsControl_SizeChanged(object sender, EventArgs e)
        {
            if (this.Width < 200) this.Width = 200;
            if (this.Height < 300) this.Height = 300;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}
