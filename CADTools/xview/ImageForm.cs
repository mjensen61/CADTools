using System;
using System.Windows.Forms;
using System.Drawing;

namespace CADTools
{
    public class ImageForm : Form
    {
        public ImageForm(Bitmap ToShow)
        {
            this.Height = 800;
            this.Width = 800;
            var pictureBox1 = new PictureBox() { Location = new System.Drawing.Point(10, 10), Height = 700, Width = 780 };
            var buttonOK = new Button() { Text = "Ok", DialogResult = DialogResult.OK, Width = 30, Location = new System.Drawing.Point(730, 730) };
            this.Controls.Add(pictureBox1);
            this.Controls.Add(buttonOK);

            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox1.BackgroundImage = ToShow;
        }
    }
}
