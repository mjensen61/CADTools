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
using System.Diagnostics;
using Autodesk.AutoCAD.DatabaseServices;
using System.Windows.Media;

namespace CADTools
{
    public class WinUtilities
    {
        #region shell methods
        public static void OpenWithDefaultProgram(string path)
        {
            Process fileopener = new Process();

            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }
        public static void OpenShortcut(string linkPathName)
        {
            if (System.IO.File.Exists(linkPathName))
            {
                // WshShellClass shell = new WshShellClass();
                WshShell shell = new WshShell(); //Create a new WshShell Interface
                IWshShortcut link = (IWshShortcut)shell.CreateShortcut(linkPathName); //Link the interface to our shortcut

                DialogResult dlgresult = MessageBox.Show(link.TargetPath,
                      "*** Do you wish to open the following folder? ***",
                      MessageBoxButtons.YesNo);
                //if (dlgresult == DialogResult.Yes)
                //{
                //if (System.IO.File.Exists(linkPathName))
                //{
                Process.Start(link.TargetPath);//Open the target path
                                               //}
                                               //}
            }
        }
        #endregion

        //=================================================================================================
        #region image methods
        public static System.Drawing.Image ImageSourceToGDI(System.Windows.Media.Imaging.BitmapSource src)
        {
            var ms = new MemoryStream();
            var encoder =
              new System.Windows.Media.Imaging.BmpBitmapEncoder();
            encoder.Frames.Add(
              System.Windows.Media.Imaging.BitmapFrame.Create(src)
            );
            encoder.Save(ms);
            ms.Flush();
            return System.Drawing.Image.FromStream(ms);
        }

        //https://james-ramsden.com/c-convert-image-bitmapimage/
        public static BitmapImage Convert(System.Drawing.Image img)
        {
            using (var memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public static System.Windows.Media.ImageSource ConvertBitmapToImageSource(IntPtr imgHandle)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                imgHandle,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

        }
        #endregion

    }
}
