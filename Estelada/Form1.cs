using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Estelada
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            paintFlag();
        }



        private Point[] setOffset(Point[] polygon, int offsetX, int offsetY)
        {
            Point[] res = new Point[polygon.Length];
            for(int i=0;i<polygon.Length;i++)
            {
                res[i] = new Point(polygon[i].X + offsetX, polygon[i].Y + offsetY);
            }

            return res;
        }

        private void drawStar(int x, int y, int r, Graphics g)
        {
            float theta = (float)(Math.PI * .4f);
            float alpha = theta * .5f;
            Point[] polygon =
            {
                new Point(0,-r),
                //new Point((int)(r*Math.Sin(theta)),(int)(-r*Math.Cos(theta))),
                new Point((int)(r*Math.Sin(alpha)),(int)(r*Math.Cos(alpha))),
                //new Point((int)(-r*Math.Sin(alpha)),(int)(r*Math.Cos(alpha))),
                new Point((int)(-r*Math.Sin(theta)),(int)(-r*Math.Cos(theta))),
                new Point((int)(r*Math.Sin(theta)),(int)(-r*Math.Cos(theta))),
                new Point((int)(-r*Math.Sin(alpha)),(int)(r*Math.Cos(alpha)))
            };
            polygon = setOffset(polygon, x, y);
            Pen pen = new Pen(Color.White);
            g.DrawPolygon(pen, polygon);
        }

        private void fillStar(int x, int y, int r, Graphics g)
        {
            g.SmoothingMode = SmoothingMode.None;
            float theta = (float)(Math.PI * .4f);
            float alpha = theta * .5f;
            Point[] polygon =
            {
                new Point(0,-r),
                new Point((int)(r*Math.Sin(theta)),(int)(-r*Math.Cos(theta))),
                new Point((int)(r*Math.Sin(alpha)),(int)(r*Math.Cos(alpha))),
                new Point((int)(-r*Math.Sin(alpha)),(int)(r*Math.Cos(alpha))),
                new Point((int)(-r*Math.Sin(theta)),(int)(-r*Math.Cos(theta)))
            };
            polygon = setOffset(polygon, x, y);

            SolidBrush brush = new SolidBrush(Color.White);


            for (int i = 0; i < 5; i++)
            {
                int j = (i + (1 << 1)) % 5;
                g.FillPolygon(brush, new Point[] { polygon[i], new Point(x, y), polygon[j] });
            }
            
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

        public Bitmap wave(Bitmap image)
        {
            byte[] array = BitmapToByteArray(image);
            int width = image.Width;
            int height = image.Height;
            int A = 0x80;
            int p = height/7;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double a = Math.PI / p;
                    a += (width - x) * .0001;
                    int delta = (int)(A * Math.Sin(a * (x - .8 * y)));
                    array[(y * width + x)*4] = addRGB(delta, delta, delta, array[(y * width + x)*4]);
                    
                }
            }
            return ByteArrayToBitmap(array, image);
        }

        private void paintFlag()
        {
            int offset = 10;
            int flagWidth = pictureBox1.Width - (offset <<1);
            
            int flagHeight = (int)(flagWidth * 2.0 / 3.0);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight * 1.5);
            }

            if(pictureBox1.Width==0 || pictureBox1.Height == 0)
            {
                return;
            }
            Bitmap canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            // or
            SolidBrush or = new SolidBrush(Color.FromArgb(0xFF, 0xD7, 0x00));
            g.FillRectangle(or, offset, offset, flagWidth, flagHeight);

            // four bars gules
            SolidBrush gules = new SolidBrush(Color.FromArgb(0xcc, 0x00, 0x00));
            int interval = (int)(flagHeight / 9.0);
            for (int i = 0; i < 4; i++)
            {
                g.FillRectangle(gules, offset, interval * ((i<<1) + 1) + offset, flagWidth, interval);
            }

            // triangle
            Point[] polygon =
            {
                new Point(0,0), new Point(flagHeight>>1,flagHeight>>1), new Point(0,flagHeight)
            };
            SolidBrush azure=new SolidBrush(Color.FromArgb(0x0f, 0x47, 0xaf));
            g.FillPolygon(azure, setOffset(polygon,offset, offset));

            // star
            fillStar((int)(flagHeight /5.0 + offset), (int)(flagHeight*.5+offset), (int)(flagHeight /7.0), g);

            Pen pen = new Pen(Color.Black);
            g.DrawRectangle(pen, 0, 0, canvas.Width, canvas.Height);
            
            gules.Dispose();
            or.Dispose();
            g.Dispose();

           canvas = wave(canvas);
            pictureBox1.Image = canvas;

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            paintFlag();
           
        }
    }
}
