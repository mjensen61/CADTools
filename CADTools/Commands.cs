using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(CADTools.asdkCommands))]

namespace CADTools
{
    /// <summary>
    /// # CADTools asdkCommands class
    /// asdkCommands class provides the initial command and toolpallete for use by the AutoCAD command system.
    /// </summary>

    public class asdkCommands
    {
        /** PaletteSet CadPaletteSet 
        *  The main pallete set 
        */
        static public Autodesk.AutoCAD.Windows.PaletteSet CadPaletteSet = null;
        public asdkCommands()
        {
        }

        ///
        /// the main block view command, brings up Model dialog with GsView control
        /// 
        [CommandMethod("cadbpdev")]
        static public void cadbpdev()
        {
            if (CADTools.asdkCommands.CadPaletteSet == null) //Check that an instance of CadPaletteSet doesn't already exist.  Only open one at a time
            {
                // create a new instance of the dialog
                CadPaletteSet = new Autodesk.AutoCAD.Windows.PaletteSet("CAD tools", new Guid("{ABF4D614-C90C-4B95-8096-4E455FD13808}"));

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