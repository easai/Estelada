using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;

namespace Estelada
{
    public partial class Main : Form
    {
        SolidBrush azureBrush = new SolidBrush(Color.FromArgb(0x0f, 0x47, 0xaf));
        SolidBrush gulesBrush = new SolidBrush(Color.FromArgb(0xcc, 0x00, 0x00));
        SolidBrush orBrush = new SolidBrush(Color.FromArgb(0xFF, 0xD7, 0x00));
        SolidBrush argentBrush = new SolidBrush(Color.FromArgb(0xFF, 0xFF, 0xFF));

        Boolean wavy = true;

        int starSize = -1;
        int flagWidth = 0;
        int flagHeight = 0;

        public int offset = 10;

        public Main()
        {
            InitializeComponent();

            PaintFlag();
        }

        public void setStarSize(int size)
        {
            starSize = size;
        }

        private Point[] setOffset(Point[] polygon, int offsetX, int offsetY)
        {
            Point[] res = new Point[polygon.Length];
            for (int i = 0; i < polygon.Length; i++)
            {
                res[i] = new Point(polygon[i].X + offsetX, polygon[i].Y + offsetY);
            }

            return res;
        }

        private Point ccw(float x, float y, float theta)
        {
            return new Point((int)(x * Math.Cos(theta) + y * Math.Sin(theta)), (int)(-x * Math.Sin(theta) + y * Math.Cos(theta)));
        }

        private Point[] flip(Point[] polygon)
        {
            Point[] res = new Point[polygon.Length];
            for (int i = 0; i < polygon.Length; i++)
            {
                res[i] = new Point(polygon[i].X, -polygon[i].Y);
            }
            return res;
        }

        private void drawStar(int x0, int y0, int r, Graphics g)
        {
            float theta = (float)(Math.PI * .4f);
            float alpha = theta * .5f;
            float beta = alpha * .5f;
            float y = (float)(r * Math.Cos(theta));
            float x = (float)((r - y) * Math.Tan(beta));
            Point[] polygon =
            {
                new Point(0,r),
                new Point((int)(x),(int)(y)),
                new Point((int)(r*Math.Sin(theta)),(int)(r*Math.Cos(theta))),
                ccw(x,y,theta),
                new Point((int)(r*Math.Sin(alpha)),(int)(-r*Math.Cos(alpha))),
                ccw(x,y,theta*2.0f),
                new Point((int)(-r*Math.Sin(alpha)),(int)(-r*Math.Cos(alpha))),
                ccw(x,y,theta*3.0f),
                new Point((int)(-r*Math.Sin(theta)),(int)(r*Math.Cos(theta))),
                ccw(x,y,theta*4.0f)
            };

            polygon = flip(polygon);
            polygon = setOffset(polygon, x0, y0);

            Pen pen = new Pen(Color.White);
            g.DrawPolygon(pen, polygon);
        }

        private void fillStar(int x0, int y0, int r, Graphics g)
        {

            float theta = (float)(Math.PI * .4f);
            float alpha = theta * .5f;
            float beta = alpha * .5f;
            float y = (float)(r * Math.Cos(theta));
            float x = (float)((r - y) * Math.Tan(beta));
            Point[] polygon =
            {
                new Point(0,r),
                new Point((int)(x),(int)(y)),
                new Point((int)(r*Math.Sin(theta)),(int)(r*Math.Cos(theta))),
                ccw(x,y,theta),
                new Point((int)(r*Math.Sin(alpha)),(int)(-r*Math.Cos(alpha))),
                ccw(x,y,theta*2.0f),
                new Point((int)(-r*Math.Sin(alpha)),(int)(-r*Math.Cos(alpha))),
                ccw(x,y,theta*3.0f),
                new Point((int)(-r*Math.Sin(theta)),(int)(r*Math.Cos(theta))),
                ccw(x,y,theta*4.0f)
            };

            polygon = flip(polygon);
            polygon = setOffset(polygon, x0, y0);

            g.FillPolygon(argentBrush, polygon);

        }

        public byte[] BitmapToByteArray(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            IntPtr ptr = bmpData.Scan0;

            int bytes = bmp.Width * bmp.Height * 4;
            byte[] rgbValues = new byte[bytes];

            Marshal.Copy(ptr, rgbValues, 0, bytes);

            bmp.UnlockBits(bmpData);
            return rgbValues;
        }

        public Bitmap ByteArrayToBitmap(byte[] rgbValues, Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            IntPtr ptr = bmpData.Scan0;

            Marshal.Copy(rgbValues, 0, ptr, rgbValues.Length);

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private byte addRGB(int red, int green, int blue, int p)
        {
            red += (p >> 16) & 0xff;
            if (0xff < red)
                red = 0xff;
            if (red < 0)
                red = 0;
            green += (p >> 8) & 0xff;
            if (0xff < green)
                green = 0xff;
            if (green < 0)
                green = 0;
            blue += p & 0xff;
            if (0xff < blue)
                blue = 0xff;
            if (blue < 0)
                blue = 0;
            return (byte)((0xff << 24) | (red << 16) | (green << 8) | blue);
        }

        public Bitmap Wave(Bitmap image)
        {
            byte[] array = BitmapToByteArray(image);
            int width = image.Width;
            int height = image.Height;
            int A = 0x80;
            int p = height / 7;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double a = Math.PI / p;
                    //a += (width - x) * .0001;
                    int delta = (int)(A * Math.Sin(a * (x - .9 * y)));
                    
                    array[(y * width + x) * 4] = addRGB(delta, delta, delta, array[(y * width + x) * 4]);

                }
            }
            return ByteArrayToBitmap(array, image);
        }

        public void PaintFlag()
        {

            flagWidth = pictureBox1.Width - (offset << 1);

            flagHeight = (int)(flagWidth * 2.0 / 3.0);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight * 1.5);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }
            Bitmap canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            // or          
            g.FillRectangle(orBrush, offset, offset, flagWidth, flagHeight);

            // four bars gules            
            int interval = (int)(flagHeight / 9.0);
            for (int i = 0; i < 4; i++)
            {
                g.FillRectangle(gulesBrush, offset, interval * ((i << 1) + 1) + offset, flagWidth, interval);
            }

            // triangle
            Point[] polygon =
            {
                new Point(0,0), new Point((flagHeight>>1)+1,flagHeight>>1), new Point(0,flagHeight)
            };
            g.FillPolygon(azureBrush, setOffset(polygon, offset - 1, offset));

            // star
            if (starSize == -1)
            {
                starSize = 1;
            }
            fillStar((int)(flagHeight / 5.0 + offset), (int)(flagHeight * .5 + offset), (int)(starSize * flagHeight / 7), g);

            g.Dispose();

            if (wavy)
            {
                canvas = Wave(canvas);
            }
            wavyToolStripMenuItem.Checked = wavy;

            pictureBox1.Image = canvas;

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            PaintFlag();

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().Show();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        int getRGB(Color c)
        {
            return (c.B << 16) + (c.G << 8) + c.R;
        }

        private void azureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = azureBrush.Color;
            dlg.CustomColors = new int[] {
               getRGB(azureBrush.Color),
               getRGB(argentBrush.Color),
               getRGB(orBrush.Color),
               getRGB(gulesBrush.Color)
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                azureBrush.Color = dlg.Color;
                PaintFlag();
            }
        }

        private void gulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = gulesBrush.Color;
            dlg.CustomColors = new int[] {
               getRGB(azureBrush.Color),
               getRGB(argentBrush.Color),
               getRGB(orBrush.Color),
               getRGB(gulesBrush.Color)
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                gulesBrush.Color = dlg.Color;
                PaintFlag();
            }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            azureBrush.Dispose();
            gulesBrush.Dispose();
            orBrush.Dispose();

        }

        private void orToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = orBrush.Color;
            dlg.CustomColors = new int[] {
               getRGB(azureBrush.Color),
               getRGB(argentBrush.Color),
               getRGB(orBrush.Color),
               getRGB(gulesBrush.Color)
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                orBrush.Color = dlg.Color;
                PaintFlag();
            }
        }

        private void wavyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wavy = !wavy;
            PaintFlag();
        }

        private void starToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new StarSize(this).Show();
        }

        private void saveasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveAsFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap bitmap = new Bitmap(flagWidth, flagHeight);
                Graphics g = Graphics.FromImage(bitmap);
                Rectangle destRect = new Rectangle(0, 0, flagWidth, flagHeight);
                Rectangle srcRect = new Rectangle(offset, offset, flagWidth, flagHeight);
                g.DrawImage(pictureBox1.Image, destRect, srcRect, GraphicsUnit.Pixel);
                string fileName = saveAsFileDialog.FileName;
                int index = fileName.IndexOf(".");
                if (index < 0)
                {
                    fileName += ".png";
                }
                bitmap.Save(fileName);
            }

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                XmlDocument document = new XmlDocument();

                XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement root = document.CreateElement("estelada");

                document.AppendChild(declaration);
                document.AppendChild(root);

                XmlElement or = document.CreateElement("or");
                or.InnerText = String.Format("{0:X}", orBrush.Color.ToArgb());
                XmlElement gules = document.CreateElement("gules");
                gules.InnerText = String.Format("{0:X}", gulesBrush.Color.ToArgb());
                XmlElement azure = document.CreateElement("azure");
                azure.InnerText = String.Format("{0:X}", azureBrush.Color.ToArgb());

                root.AppendChild(or);
                root.AppendChild(gules);
                root.AppendChild(azure);

                string fileName = saveFileDialog1.FileName;
                int index = fileName.IndexOf(".");
                if (index < 0)
                {
                    fileName += ".xml";
                }
                document.Save(fileName);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                XmlDocument document = new XmlDocument();

                document.Load(openFileDialog1.FileName);

                foreach (XmlElement element in document.DocumentElement)
                {

                    string text = element.InnerText;
                    int num = int.Parse(text, System.Globalization.NumberStyles.AllowHexSpecifier);
                    if (element.Name == "or")
                    {
                        orBrush.Color = Color.FromArgb(num);
                    }
                    else if (element.Name == "gules")
                    {
                        gulesBrush.Color = Color.FromArgb(num);
                    }
                    else if (element.Name == "azure")
                    {
                        azureBrush.Color = Color.FromArgb(num);
                    }

                }

                PaintFlag();
            }
        }

        private void argentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = argentBrush.Color;
            dlg.CustomColors = new int[] {
               getRGB(azureBrush.Color),
               getRGB(argentBrush.Color),
               getRGB(orBrush.Color),
               getRGB(gulesBrush.Color)
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                argentBrush.Color = dlg.Color;
                PaintFlag();
            }
        }
    }
}
