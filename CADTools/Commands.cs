using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(CADTools.asdkCommands))]

namespace CADTools
{
    /// <summary>
    /// Summary description for asdkCommands.
    /// </summary>
    public class asdkCommands
    {
        static public Autodesk.AutoCAD.Windows.PaletteSet CadPaletteSet = null;
        public asdkCommands()
        {
        }

        // the main block view command, brings up Model dialog with GsView control
        [CommandMethod("cadbpx")]
        static public void calbpx()
        {
            if (CADTools.asdkCommands.CadPaletteSet == null) //Check that an instance of CadPaletteSet doesn't already exist.  Only open one at a time
            {
                // create a new instance of the dialog
                CadPaletteSet = new Autodesk.AutoCAD.Windows.PaletteSet("CAD tools", new Guid("{a296f834-7569-4468-aaa5-242ab14b5e78}"));

                // Add all the required controls to the pallette set
                CadPaletteSet.Add("CAD Tools", new CADToolsControl());
                CadPaletteSet.Visible = true;
                var existingSize = CadPaletteSet.Size;
                CadPaletteSet.Size = new System.Drawing.Size(400, 600);
                CadPaletteSet.Size = existingSize;

                CadPaletteSet.DockEnabled =
                    (DockSides)((int)DockSides.Left
                       + (int)DockSides.Right);
            }
            else
            {
                CADTools.asdkCommands.CadPaletteSet.Visible = true;
            }
        }
    }
}