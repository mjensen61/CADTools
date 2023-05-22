
// need to define UseACAD if designing a DLL to run within AutoCAD
// for freestanding executables comment this out and the
// AutoCAD functionality will be disabled.
#define UseACAD

//#define DO_ACADCONN_DEBUG_MESSAGES

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Windows.Media.Imaging;
using System.Net;
using System.Threading.Tasks;

using IWshRuntimeLibrary;

//using GsRenderMode = Autodesk.AutoCAD.GraphicsSystem.RenderMode;
using System.Diagnostics;

// autocad stuff.
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsSystem;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Interop;

using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace CADTools
{
    internal class ACADConnector
    {
        //=================================================================================================
        #region Console output
        public static void WriteCADMessage(String message)
        {
            Document acDoc = AcadApp.DocumentManager.MdiActiveDocument;
            acDoc.Editor.WriteMessage("\n" + message);
        }
        #endregion

        //=================================================================================================
        #region Graphical output
        // HTML methods sourced from https://www.keanw.com/2014/04/adding-a-web-page-as-a-document-tab-in-autocad-2015-using-net.html
        class HeadClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var req = base.GetWebRequest(address);
                if (req.Method == "GET")
                    req.Method = "HEAD";
                return req;
            }
        }
        private async static Task<bool> PageExists(string url)
        {
            // First check whether the URL is valid

            Uri uriResult;
            if (
              !Uri.TryCreate(url, UriKind.Absolute, out uriResult) ||
              uriResult.Scheme != Uri.UriSchemeHttps
            )
                return false;

            // Then we try to peform a HEAD request on the page
            // (a WebException will be fired if it doesn't exist)

            try
            {
                using (var client = new HeadClient())
                {
                    await client.DownloadStringTaskAsync(url);
                }
                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }
        public async static void OpenHTTPS(string title, string url)
        {
            // As we're calling an async function, we need to await
            // (and mark the command itself as async)

            if (await PageExists(url))
            {
                // Now that we've validated the URL, we can call the
                // new API in AutoCAD 2015 to load our page

                Autodesk.AutoCAD.ApplicationServices.Application.DocumentWindowCollection.AddDocumentWindow(
                  title, new System.Uri(url)
                );
            }
            else
            {
                // Print a helpful message if the URL wasn't loadable

                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                ed.WriteMessage(
                  "\nCould not load url: \"{0}\".", url
                );
            }
        }

        public static void OpenPDF(string title, string file)
        {
            // As we're calling an async function, we need to await
            // (and mark the command itself as async)

            if (System.IO.File.Exists(file))
            {
                // Now that we've validated the URL, we can call the
                // new API in AutoCAD 2015 to load our page

                Autodesk.AutoCAD.ApplicationServices.Application.DocumentWindowCollection.AddDocumentWindow(
                  title, new System.Uri(file)
                );
            }
            else
            {
                // Print a helpful message if the URL wasn't loadable

                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                ed.WriteMessage(
                  "\nCould not load file: \"{0}\".", file
                );
            }
        }
        #endregion

        //=================================================================================================
        #region Dwg bitmap methods 
        public static void GetOuterDWGModelBitmap(string filename, System.Windows.Forms.PictureBox pictureBox)
        {
#if DO_ACADCONN_DEBUG_MESSAGES
            CADClasses.Utilities.WriteCADMessage("Getting Bitmap");
#endif
            ObjectIdCollection ids = new ObjectIdCollection();
            using (Database OuterDB = new Database())
            {
                OuterDB.ReadDwgFile(filename, System.IO.FileShare.Read, false, "");
                using (Transaction tr = OuterDB.TransactionManager.StartTransaction())
                {
                    BlockTable bt;
                    bt = (BlockTable)tr.GetObject(OuterDB.BlockTableId
                                                   , OpenMode.ForRead);

                    BlockTableRecord blk = (BlockTableRecord)tr.GetObject(bt["*Model_Space"], OpenMode.ForRead);


                    // Low resolution method
                    //var imgsrc = Autodesk.AutoCAD.Windows.Data.CMLContentSearchPreviews.GetBlockTRThumbnail(blk);

                    // High resolution method
                    int imgWidth = pictureBox.Width;
                    int imgHeight = pictureBox.Height;
                    var imgsrc = GetBlockImage(bt["*Model_Space"], imgWidth, imgHeight, Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 0));

                    var bmp = WinUtilities.ImageSourceToGDI(WinUtilities.Convert(imgsrc));

                    pictureBox.BackgroundImageLayout = ImageLayout.Stretch;
                    pictureBox.BackgroundImage = bmp as System.Drawing.Bitmap;

                    tr.Commit();
                }
            }
        }

        public static System.Drawing.Image GetBlockImage(
            ObjectId blockDefinitionId, int imgWidth, int imgHeight,
            Autodesk.AutoCAD.Colors.Color backColor)
        {
            return System.Drawing.Image.FromHbitmap(
                GetBlockImagePointer(blockDefinitionId, imgWidth, imgHeight, backColor));
        }
        #endregion

        //=================================================================================================
        #region Block methods 
        // Sourced from https://drive-cad-with-code.blogspot.com/2020/
        public static System.IntPtr GetBlockImagePointer(
            string blkName, Database db, int imgWidth, int imgHeight,
            Autodesk.AutoCAD.Colors.Color backColor)
        {
            var blockId = GetBlockTableRecordId(blkName, db);
            if (!blockId.IsNull)
            {
                var imgPtr = Utils.GetBlockImage(blockId, imgWidth, imgHeight, backColor);
                return imgPtr;
            }
            else
            {
                throw new ArgumentException(
                    $"Cannot find block definition in current database: \"{blkName}\".");
            }
        }

        private static ObjectId GetBlockTableRecordId(string blkName, Database db)
        {
            var blkId = ObjectId.Null;

            using (var tran = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tran.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (bt.Has(blkName))
                {
                    blkId = bt[blkName];
                }
                tran.Commit();
            }

            return blkId;
        }

        public static System.IntPtr GetBlockImagePointer(
            ObjectId blockDefinitionId, int imgWidth, int imgHeight,
            Autodesk.AutoCAD.Colors.Color backColor)
        {
            var imgPtr = Utils.GetBlockImage(blockDefinitionId, imgWidth, imgHeight, backColor);
            return imgPtr;
        }

        public static System.Drawing.Image GetBlockImage(
            string blkName, Database db, int imgWidth, int imgHeight,
            Autodesk.AutoCAD.Colors.Color backColor)
        {
            return System.Drawing.Image.FromHbitmap(
                GetBlockImagePointer(blkName, db, imgWidth, imgHeight, backColor));

        }

        public static System.Windows.Media.ImageSource GetImageSource(
            string blkName, Database db, int imgWidth, int imgHeight,
            Autodesk.AutoCAD.Colors.Color backColor)
        {
            var imgPtr = GetBlockImagePointer(blkName, db, imgWidth, imgHeight, backColor);
            return WinUtilities.ConvertBitmapToImageSource(imgPtr);
        }

        public static System.Windows.Media.ImageSource GetImageSource(
            ObjectId blockDefinitionId, int imgWidth, int imgHeight,
            Autodesk.AutoCAD.Colors.Color backColor)
        {
            var imgPtr = GetBlockImagePointer(blockDefinitionId, imgWidth, imgHeight, backColor);
            return WinUtilities.ConvertBitmapToImageSource(imgPtr);
        }

        #endregion

        //=================================================================================================
        #region AutoCAD file search utilities
        public static string AcadFindFile(string fileToFind)
        {
            AcadPreferences acadPrefs = (AcadPreferences)Autodesk.AutoCAD.ApplicationServices.Application.Preferences;
            string[] pathsToSearchIn = acadPrefs.Files.SupportPath.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToArray<string>();

            return AcadFindFile(fileToFind, pathsToSearchIn);
        }

        public static string AcadFindFile(string fileToFind, string[] pathsToSearchIn)
        {
            string fileFound = "";

            // Test of file to find already has full path
            if (Path.IsPathRooted(fileToFind))
            {
                if (System.IO.File.Exists(fileToFind))
                    return fileToFind;

                string fileFound2 = Path.GetFileName(fileToFind);
            }


            var initialFolderList = pathsToSearchIn.ToArray<string>();
            var existingFolderList = initialFolderList.Where(path => new DirectoryInfo(path).Exists);
            foreach (var path in existingFolderList)
            {

                string[] filesfound = Directory.GetFiles(path, fileToFind);
                foreach (string filename in filesfound)
                {
                    if (System.IO.File.Exists(filename))
                    {
                        return filename;
                    }
                }
            }
            return fileFound;
        }
        #endregion


        //=================================================================================================
        #region Block Manager Classes
        public class MeasurementUnitsConverter
        {
            private readonly Dictionary<UnitsValue, double> _linkBetweenDrawingUnitsAndMilimeters;

            public MeasurementUnitsConverter()
            {
                _linkBetweenDrawingUnitsAndMilimeters = new Dictionary<UnitsValue, double>
                                                    {
                                                        {
                                                            UnitsValue.Angstroms,
                                                            0.0000001
                                                        },
                                                        {
                                                            UnitsValue.Astronomical,
                                                            149600000000000
                                                        },
                                                        {
                                                            UnitsValue.Centimeters, 10
                                                        },
                                                        {
                                                            UnitsValue.Decimeters, 100
                                                        },
                                                        {
                                                            UnitsValue.Dekameters,
                                                            10000
                                                        },
                                                        { UnitsValue.Feet, 304.8 },
                                                        {
                                                            UnitsValue.Gigameters,
                                                            1000000000000
                                                        },
                                                        {
                                                            UnitsValue.Hectometers,
                                                            100000
                                                        },
                                                        { UnitsValue.Inches, 25.4 },
                                                        {
                                                            UnitsValue.Kilometers,
                                                            1000000
                                                        },
                                                        {
                                                            UnitsValue.LightYears,
                                                            9460700000000000000
                                                        },
                                                        { UnitsValue.Meters, 1000 },
                                                        {
                                                            UnitsValue.MicroInches,
                                                            0.0000254
                                                        },
                                                        { UnitsValue.Microns, 0.001 },
                                                        {
                                                            UnitsValue.Miles, 1609344.0
                                                        },
                                                        { UnitsValue.Millimeters, 1 },
                                                        { UnitsValue.Mils, 25400 },
                                                        {
                                                            UnitsValue.Nanometers,
                                                            0.000001
                                                        },
                                                        { UnitsValue.Undefined, 1.0 },
                                                        { UnitsValue.Yards, 914.4 }
                                                    };
                //_linkBetweenDrawingUnitsAndMilimeters.Add(UnitsValue.Parsecs, 30857000000000000000);
            }

            public double GetScaleRatio(UnitsValue sourceUnits, UnitsValue targetUnits)
            {
                if (sourceUnits == UnitsValue.Undefined || targetUnits == UnitsValue.Undefined
                    || !_linkBetweenDrawingUnitsAndMilimeters.ContainsKey(sourceUnits)
                    || !_linkBetweenDrawingUnitsAndMilimeters.ContainsKey(targetUnits))
                {
                    return 1;
                }
                return _linkBetweenDrawingUnitsAndMilimeters[sourceUnits]
                       / _linkBetweenDrawingUnitsAndMilimeters[targetUnits];
            }
        }

        /// <summary>
        /// the source drawig should be drawn as number of
        /// separate entites with or without attributes.
        /// Throws NotImplementedException if invoked with .dxf file
        /// </summary>
        /// <param name="sourceDrawing"></param>
        /// <param name="insertionPoint"></param>
        /// <returns>ObjectID of the Block Def that was imported.</returns>
        public static void ImportDwgAsBlock(string sourceDrawing, Point3d insertionPoint)
        {
            DocumentCollection dm = AcadApp.DocumentManager;
            Document _doc = dm.MdiActiveDocument;
            Editor _ed = _doc.Editor;
            Autodesk.AutoCAD.DatabaseServices.Database destinationDb = _doc.Database;
            Matrix3d ucs = _ed.CurrentUserCoordinateSystem;

            string blockname = sourceDrawing.Remove(0, sourceDrawing.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            blockname = blockname.Substring(0, blockname.Length - 4); // remove the extension

            try
            {
                using (_doc.LockDocument())
                {
                    using (var inMemoryDb = new Autodesk.AutoCAD.DatabaseServices.Database(false, true))
                    {
                        #region Load the drawing into temporary inmemory database
                        if (sourceDrawing.LastIndexOf(".dwg", StringComparison.Ordinal) > 0)
                        {
                            inMemoryDb.ReadDwgFile(sourceDrawing, System.IO.FileShare.Read, true, "");
                        }
                        else if (sourceDrawing.LastIndexOf(".dxf", StringComparison.Ordinal) > 0)
                        {
                            throw new NotImplementedException("Importing .dxf is not supported in this version.");
                            //inMemoryDb.DxfIn("@" + sourceDrawing, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\log\\import_block_dxf_log.txt");
                        }
                        else
                        {
                            throw new ArgumentException("This is not a valid drawing.");
                        }
                        #endregion

                        using (var transaction = destinationDb.TransactionManager.StartTransaction())
                        {
                            BlockTable destDbBlockTable = (BlockTable)transaction.GetObject(destinationDb.BlockTableId, OpenMode.ForRead);
                            BlockTableRecord destDbCurrentSpace = (BlockTableRecord)destinationDb.CurrentSpaceId.GetObject(OpenMode.ForWrite);

                            //==================================================================================================
                            // If the destination DWG already contains this block definition
                            // we will create a block reference from the "inMemoryDb" and not a copy of the same definition
                            //==================================================================================================
                            ObjectId sourceBlockId;
                            if (destDbBlockTable.Has(blockname))
                            {
#if DO_DEBUG_MESSAGES
                                ACADConnector.WriteCADMessage("Blockname " + blockname + "is already in the drawing");
#endif
                                BlockTableRecord destDbBlockDefinition = (BlockTableRecord)transaction.GetObject(destDbBlockTable[blockname], OpenMode.ForRead);
                                sourceBlockId = destDbBlockDefinition.ObjectId;

                                // Create a block reference to the existing block definition
                                using (var blockReference = new BlockReference(insertionPoint, sourceBlockId))
                                {
                                    _ed.CurrentUserCoordinateSystem = Matrix3d.Identity;
                                    blockReference.TransformBy(ucs);
                                    _ed.CurrentUserCoordinateSystem = ucs;
                                    var converter = new MeasurementUnitsConverter();
                                    var scaleFactor = converter.GetScaleRatio(inMemoryDb.Insunits, destinationDb.Insunits);
                                    blockReference.ScaleFactors = new Scale3d(scaleFactor);
                                    destDbCurrentSpace.AppendEntity(blockReference);
                                    transaction.AddNewlyCreatedDBObject(blockReference, true);
                                    //================  CHECK FOR ATTRIBUTES ==================
                                    if (destDbBlockDefinition.HasAttributeDefinitions)
                                    {
#if DO_DEBUG_MESSAGES
                                        ACADConnector.WriteCADMessage("Processing Attributes");
#endif
                                        foreach (ObjectId id in destDbBlockDefinition)
                                        {
                                            DBObject obj =
                                              transaction.GetObject(id, OpenMode.ForRead);
                                            AttributeDefinition ad =
                                              obj as AttributeDefinition;
#if DO_DEBUG_MESSAGES
                                            ACADConnector.WriteCADMessage("     Processing Attribute");
#endif

                                            if (ad != null && !ad.Constant)
                                            {
#if DO_DEBUG_MESSAGES
                                                ACADConnector.WriteCADMessage("         ad noty null");
#endif
                                                AttributeReference ar =
                                                  new AttributeReference();
                                                ar.SetAttributeFromBlock(ad, blockReference.BlockTransform);
                                                ar.Position =
                                                  ad.Position.TransformBy(blockReference.BlockTransform);

#if DO_DEBUG_MESSAGES
                                                ACADConnector.WriteCADMessage("         Set string " + ad.TextString);
#endif
                                                ar.TextString = ad.TextString;

#if DO_DEBUG_MESSAGES
                                                ACADConnector.WriteCADMessage("         Append Attribute ");
#endif
                                                blockReference.AttributeCollection.AppendAttribute(ar);
                                                transaction.AddNewlyCreatedDBObject(ar, true);
                                            }
                                        }
                                    }

                                    _ed.Regen();
                                    transaction.Commit();
                                    // At this point the Bref has become a DBObject and (can be disposed) and will be disposed by the transaction
                                }
                                return;
                            }
                            //==================================================================================================
                            else // There is not such block definition, so we are inserting/creating new one
                                 //==================================================================================================
                            {
#if DO_DEBUG_MESSAGES
                                ACADConnector.WriteCADMessage("Insert new Block " + sourceDrawing + " into the drawing");
#endif

#if DO_DEBUG_MESSAGES
                                ACADConnector.WriteCADMessage("Move block " + sourceDrawing + " from 'inMemoryDb' into the drawing");
#endif
                                sourceBlockId = destinationDb.Insert(sourceDrawing, inMemoryDb, true);

#if DO_DEBUG_MESSAGES
                                ACADConnector.WriteCADMessage("Move block " + blockname + " from 'inMemoryDb' into the drawing");
#endif
                                BlockTableRecord destDbBlockDefinition = (BlockTableRecord)sourceBlockId.GetObject(OpenMode.ForRead);
                                destDbBlockDefinition.UpgradeOpen();
                                destDbBlockDefinition.Name = blockname;
                                destDbCurrentSpace.DowngradeOpen();
                                var sourceBlockMeasurementUnits = inMemoryDb.Insunits;

                                try
                                {
#if DO_DEBUG_MESSAGES
                                    ACADConnector.WriteCADMessage("Create a block reference to the new block definition");
#endif
                                    // Create a block reference to the new block definition
                                    using (var blockReference = new BlockReference(insertionPoint, destDbBlockTable[destDbBlockDefinition.Name]))
                                    {
                                        var space = (BlockTableRecord)transaction.GetObject(destinationDb.CurrentSpaceId, OpenMode.ForWrite);
                                        space.AppendEntity(blockReference);
                                        transaction.AddNewlyCreatedDBObject(blockReference, true);

                                        //================  CHECK FOR ATTRIBUTES ==================
                                        if (destDbBlockDefinition.HasAttributeDefinitions)
                                        {
#if DO_DEBUG_MESSAGES
                                            ACADConnector.WriteCADMessage("Processing Attributes");
#endif
                                            foreach (ObjectId id in destDbBlockDefinition)
                                            {
                                                DBObject obj =
                                                  transaction.GetObject(id, OpenMode.ForRead);
                                                AttributeDefinition ad =
                                                  obj as AttributeDefinition;
#if DO_DEBUG_MESSAGES
                                                ACADConnector.WriteCADMessage("     Processing Attribute");
#endif

                                                if (ad != null && !ad.Constant)
                                                {
#if DO_DEBUG_MESSAGES
                                                    ACADConnector.WriteCADMessage("         ad noty null");
#endif
                                                    AttributeReference ar =
                                                      new AttributeReference();
                                                    ar.SetAttributeFromBlock(ad, blockReference.BlockTransform);
                                                    ar.Position =
                                                      ad.Position.TransformBy(blockReference.BlockTransform);

#if DO_DEBUG_MESSAGES
                                                    ACADConnector.WriteCADMessage("         Set string " + ad.TextString);
#endif
                                                    ar.TextString = ad.TextString;

#if DO_DEBUG_MESSAGES
                                                    ACADConnector.WriteCADMessage("         Append Attribute ");
#endif
                                                    blockReference.AttributeCollection.AppendAttribute(ar);
                                                    transaction.AddNewlyCreatedDBObject(ar, true);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (ArgumentException argumentException)
                                {
                                    ACADConnector.WriteCADMessage("Error. Check inner exception." + argumentException.ToString());
                                }

                                _ed.Regen();
                                transaction.Commit();

                                return;
                            }
                        }
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception exception)
            {
                ACADConnector.WriteCADMessage("Error in ImportDrawingAsBlock()." + exception.ToString());

            }
        }
        public static void InsertBlock(string sourceDrawing)//, String blockname)
        {
#if DO_DEBUG_MESSAGES
            ACADConnector.WriteCADMessage("InsertBlock()");
#endif
            Document acDoc = AcadApp.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");
            // Prompt for the start point
            pPtOpts.Message = "\nEnter the insertion point of the block: ";
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptStart = pPtRes.Value;
            // Exit if the user presses ESC or cancels the command
            if (pPtRes.Status == PromptStatus.Cancel) return;
            // Prompt for the end point

            ImportDwgAsBlock(sourceDrawing, ptStart);
        }
        public static void ListdwgBlocks(string sourceFileName)
        {
            DocumentCollection dm = AcadApp.DocumentManager;
            Editor ed = dm.MdiActiveDocument.Editor;
            Database destDb = dm.MdiActiveDocument.Database;

            Transaction desttr = destDb.TransactionManager.StartTransaction();
            // Open the block table
            BlockTable localbt = (BlockTable)desttr.GetObject(destDb.BlockTableId, OpenMode.ForRead, false);

            // Check each block in the block
            // 
            foreach (ObjectId btrId in localbt)
            {
                BlockTableRecord btr =
                (BlockTableRecord)desttr.GetObject(btrId, OpenMode.ForRead, false);

                // Only add named & non-layout blocks to the copy list
                string msg = btr.Name;
                ed.WriteMessage("\nLocal Block[{0}] ", msg);
                btr.Dispose();
            }
            desttr.Abort();

            Database sourceDb = new Database(false, true);
            sourceDb.ReadDwgFile(sourceFileName, System.IO.FileShare.ReadWrite, true, "");
            Transaction sourcetr = sourceDb.TransactionManager.StartTransaction();
            // Open the block table
            BlockTable sourcebt = (BlockTable)desttr.GetObject(destDb.BlockTableId, OpenMode.ForRead, false);

            // Check each block in the block
            // 
            foreach (ObjectId btrId in sourcebt)
            {
                BlockTableRecord btr =
                (BlockTableRecord)desttr.GetObject(btrId, OpenMode.ForRead, false);

                // Only add named & non-layout blocks to the copy list
                string msg = btr.Name;
                ed.WriteMessage("\nSource Block[{0}] ", msg);
                btr.Dispose();
            }
            sourcetr.Abort();

        }


        #endregion
    }
}
