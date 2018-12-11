using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;


namespace Estelada
{
    public partial class Main : Form
    {
        enum State { Catalonia, Japan, EU, US, France, Germany, Italy, UK, Denmark, Norway, Sweden, Finland, Poland, Romania, Belgium, Ireland, Bulgaria, Austria, Switzerland, Luxemberg, Czech, Russia, Lithuania, Latvia };
        State state = State.Catalonia;

        SolidBrush azureBrush = new SolidBrush(Color.FromArgb(0x0f, 0x47, 0xaf));
        SolidBrush gulesBrush = new SolidBrush(Color.FromArgb(0xcc, 0x00, 0x00));
        SolidBrush orBrush = new SolidBrush(Color.FromArgb(0xFF, 0xD7, 0x00));
        SolidBrush argentBrush = new SolidBrush(Color.FromArgb(0xFF, 0xFF, 0xFF));
        SolidBrush sableBrush = new SolidBrush(Color.FromArgb(0x0, 0x0, 0x0));

        Boolean wavy = true;

        public float starSize = 1.0f;
        int flagWidth = 0;
        int flagHeight = 0;

        public int offset = 10;

        Task task;
        Bitmap canvas;
        Bitmap[] imageList;
        int frame = 0;

        public Main()
        {
            InitializeComponent();

            int n = (int)(Math.PI / .5f /.1f);
            imageList = new Bitmap[n+1];            
            PaintFlag();

            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += (sender, e) =>
            {
                pictureBox1.Image = imageList[frame];
                frame++;
                if (imageList.Length <= frame)
                {
                    frame = 0;
                }
            };
            timer.Start();
            
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

        private void FillStar(int x0, int y0, int r, Graphics g, Brush brush)
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

            g.FillPolygon(brush, polygon);

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

        public Bitmap ByteArrayToBitmap(byte[] rgbValues, Bitmap org)
        {
            Bitmap bmp = new Bitmap(org.Width, org.Height);

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
            return (byte)((0x00 << 24) | (red << 16) | (green << 8) | blue);
        }

        private byte overflowAdd(byte x, int y)
        {
            byte res = (byte)(x + y);
            if (0xff < (x + y))
                res = 0xff;
            if ((x + y) < 0)
                res = 0;
            return res;
        }

        public Bitmap Wave(Bitmap image, float phase = 0)
        {
            byte[] array = BitmapToByteArray(image);
            int width = image.Width;
            int height = image.Height;
            int A = 0x50;
            int p = height / 4;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double a = Math.PI / p;
                    //a += (width - x) * .0001;
                    int delta = (int)(A * Math.Sin(a * (x - .9 * y)));

                    float rate = (float)(.15 * Math.Sin(Math.PI * ((float)x / width - (float)y / height * 0.8) / .125 + phase));

                    int weekly = (y * width + x) * 4;

                    array[weekly] = overflowAdd(array[weekly], (int)(rate * 0xff));
                    weekly++;
                    array[weekly] = overflowAdd(array[weekly], (int)(rate * 0xff));
                    weekly++;
                    array[weekly] = overflowAdd(array[weekly], (int)(rate * 0xff));

                    //array[(y * width + x) * 4 + 3] = overflowAdd(array[(y * width + x) * 4 + 3], (byte)delta);//alpha
                }
            }
            return ByteArrayToBitmap(array, image);
        }

        public void PaintFlag()
        {
            switch (state)
            {
                case State.Catalonia:
                    EsteladaFlag();
                    break;
                case State.Japan:
                    HinomaruFlag();
                    break;
                case State.EU:
                    EUFlag();
                    break;
                case State.US:
                    USFlag();
                    break;
                case State.France:
                    FranceFlag();
                    break;
                case State.Germany:
                    GermanyFlag();
                    break;
                case State.Italy:
                    ItalyFlag();
                    break;
                case State.UK:
                    UKFlag();
                    break;
                case State.Denmark:
                    UKFlag();
                    break;
                case State.Norway:
                    UKFlag();
                    break;
                case State.Sweden:
                    SwedenFlag();
                    break;
                case State.Finland:
                    FinlandFlag();
                    break;
                case State.Poland:
                    PolandFlag();
                    break;
                case State.Romania:
                    RomaniaFlag();
                    break;
                case State.Belgium:
                    BelgiumFlag();
                    break;
                case State.Ireland:
                    IrelandFlag();
                    break;
                case State.Bulgaria:
                    BulgariaFlag();
                    break;
                case State.Austria:
                    AustriaFlag();
                    break;
                case State.Switzerland:
                    SwitzerlandFlag();
                    break;
                case State.Luxemberg:
                    LuxembergFlag();
                    break;
                case State.Czech:
                    CzechFlag();
                    break;
                case State.Russia:
                    RussiaFlag();
                    break;
                case State.Lithuania:
                    LithuaniaFlag();
                    break;
                case State.Latvia:
                    LatviaFlag();
                    break;
            }

            if (wavy)
            {
                wavyToolStripMenuItem.Checked = wavy;
                float x = (float)(Math.PI / .5f);
                int i = 0;
                for (; 0 <= x; x -= .1f)
                {
                    imageList[i]=Wave(canvas, x);
                    i++;
                }
            }
        }

        public void EsteladaFlag()
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
            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
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
            FillStar((int)(flagHeight / 5.0 + offset), (int)(flagHeight * .5 + offset), (int)(starSize * flagHeight / 7), g, argentBrush);

            g.Dispose();

        }

        /// <summary>
        /// 日の丸を描画
        /// </summary>
        public void HinomaruFlag()
        {
            flagWidth = pictureBox1.Width - (offset << 1);

            flagHeight = (int)(flagWidth / 1.5);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight * 1.5);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

             canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            // 地          
            g.FillRectangle(argentBrush, offset, offset, flagWidth, flagHeight);

            // 日の丸
            int r = (int)(flagHeight * 3.0 / 5.0);
            g.FillEllipse(gulesBrush, ((flagWidth - r) >> 1) + offset, ((flagHeight - r) >> 1) + offset, r, r);

            g.Dispose();
        }

        /// <summary>
        /// 星条旗を描画
        /// </summary>
        public void USFlag()
        {
            SolidBrush azureBrush = new SolidBrush(Color.FromArgb(0x3c, 0x3b, 0x6e));//#3C3B6E
            SolidBrush gulesBrush = new SolidBrush(Color.FromArgb(0xb2, 0x22, 0x34));//#B22234

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 1.0/1.9;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.FillRectangle(argentBrush, offset, offset, flagWidth, flagHeight);

            // ストライプ
            float stripHeight = flagHeight / 13.0f;
            for (int i = 0; i < 13; i++)
            {
                if ((i & 1) == 0)
                {
                    g.FillRectangle(gulesBrush, offset, offset + i * stripHeight, flagWidth, stripHeight);
                }
            }

            // スター
            float cantonWidth = flagWidth * .4f;
            float cantonHeight = stripHeight * 7;
            g.FillRectangle(azureBrush, offset, offset, cantonWidth, cantonHeight);

            float x0 = cantonWidth / 12;
            float y0 = cantonHeight / 5;
            float r = .4f * stripHeight;
            for(int i = 0; i < 11;i++)
            {
                if ((i & 1) == 0)
                {
                    for(int j = 0; j < 5; j++)
                    {
                        FillStar((int)(offset + (i+1)*x0), (int)(offset + j*y0 + y0*.5), (int)r, g, argentBrush);
                    }
                }
                else
                {
                    for(int j = 0; j < 4; j++)
                    {
                        FillStar((int)(offset + (i + 1) * x0), (int)(offset + j * y0 + y0), (int)r, g, argentBrush);
                    }
                }
            }
            g.Dispose();
        }

        /// <summary>
        /// 欧州旗
        /// </summary>
        public void EUFlag()
        {
            SolidBrush azureBrush = new SolidBrush(Color.FromArgb(0x0, 0x33, 0x99));//#003399
            SolidBrush orBrush = new SolidBrush(Color.FromArgb(0xff, 0xcc, 0x0));//#ffcc00

            flagWidth = pictureBox1.Width - (offset << 1);

            flagHeight = (int)(flagWidth / 1.5);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight * 1.5);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

             canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            // 地          
            g.FillRectangle(azureBrush, offset, offset, flagWidth, flagHeight);

            // star     
            int starHeight = (int)((flagHeight >> 1) / 9.0f);

            float r = (float)(flagHeight / 3.0);
            for (int i = 0; i < 12; i++)
            {
                int x = (int)(r * Math.Cos(Math.PI / 6.0 * i));
                int y = (int)(r * Math.Sin(Math.PI / 6.0 * i));
                FillStar((int)(x + flagWidth * .5 + offset), (int)(y + flagHeight * .5 + offset), starHeight, g, orBrush);
            }

            g.Dispose();

        }

        /// <summary>
        /// 三色旗を描画
        /// </summary>
        public void FranceFlag()
        {
            SolidBrush azureBrush = new SolidBrush(Color.FromArgb(0x0, 0x55, 0xa4));//0055A4
            SolidBrush gulesBrush = new SolidBrush(Color.FromArgb(0xef, 0x41, 0x35));//#EF4135

            flagWidth = pictureBox1.Width - (offset << 1);

            flagHeight = (int)(flagWidth / 1.5);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight * 1.5);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            float total = 30f + 33f + 37f;
            float azureWidth = 30.0f / total *flagWidth;
            float argentWidth = 33.0f / total *flagWidth;
            float gulesWidth = 37.0f / total * flagWidth;
            g.FillRectangle(azureBrush, offset, offset, azureWidth, flagHeight);
            g.FillRectangle(argentBrush, offset+ azureWidth, offset, argentWidth, flagHeight);
            g.FillRectangle(gulesBrush, offset + azureWidth + argentWidth, offset, gulesWidth, flagHeight);

            g.Dispose();
        }

        /// <summary>
        /// ドイツ国旗を描画
        /// </summary>
        public void GermanyFlag()
        {
            SolidBrush gulesBrush = new SolidBrush(Color.FromArgb(0xff, 0x0, 0x0));
            SolidBrush orBrush = new SolidBrush(Color.FromArgb(0xff, 0xcc, 0x0));

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 3.0 / 5.0;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            float stripHeight = flagHeight / 3.0f;
            g.FillRectangle(sableBrush, offset, offset, flagWidth, stripHeight);
            g.FillRectangle(gulesBrush, offset, offset+ stripHeight, flagWidth, stripHeight);
            g.FillRectangle(orBrush, offset, offset+ stripHeight+ stripHeight, flagWidth, stripHeight);

            g.Dispose();
        }

        /// <summary>
        /// イタリア国旗を描画
        /// </summary>
        public void ItalyFlag()
        {
            SolidBrush vertBrush = new SolidBrush(Color.FromArgb(0x0, 0x8c, 0x45));//#008C45
            SolidBrush argentBrush = new SolidBrush(Color.FromArgb(0xf4, 0xf5, 0xf0));//#F4F5F0
            SolidBrush gulesBrush = new SolidBrush(Color.FromArgb(0xcd, 0x21, 0x2a));//#CD212A

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 3.0 / 5.0;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            float stripWidth = flagWidth / 3.0f;
            g.FillRectangle(vertBrush, offset, offset, stripWidth, flagHeight);
            g.FillRectangle(argentBrush, offset + stripWidth, offset, stripWidth, flagHeight);
            g.FillRectangle(gulesBrush, offset + stripWidth + stripWidth, offset, stripWidth, flagHeight);

            g.Dispose();
        }

        /// <summary>
        /// ユニオンジャックを描画
        /// </summary>
        public void UKFlag()
        {
            SolidBrush azureBrush = new SolidBrush(Color.FromArgb(0x01, 0x21, 0x69));//#012169	
            SolidBrush gulesBrush = new SolidBrush(Color.FromArgb(0xC8, 0x10, 0x2E));//#C8102E

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = .5;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.FillRectangle(azureBrush, offset, offset, flagWidth, flagHeight);

            float stripWidth = flagWidth / 3.0f;

            double theta = Math.Atan(.5d);
            float a = flagWidth * 1f / 10f;
            float x = (float)(.5 * a / Math.Sin(theta));
            int xStrip = (int)(x / 1.5f);
            float y = (float)(.5 * a / Math.Cos(theta));
            int yStrip = (int)(y / 1.5f);
            Point[] polygon =
            {
                new Point(0,0), new Point((int)x,0), new Point(flagWidth,flagHeight - (int)y), new Point(flagWidth,flagHeight), new Point(flagWidth-(int)x, flagHeight), new Point(0,(int)y)
            };
            g.FillPolygon(argentBrush, setOffset(polygon, offset, offset));
            int halfWidth = (int)(.5f * flagWidth);
            int halfHeight = (int)(.5f * flagHeight);
            Point[] downStrip =
            {
                new Point(0,0), new Point(halfWidth,halfHeight), new Point(halfWidth-xStrip,halfHeight), new Point(0,yStrip)
            };
            g.FillPolygon(gulesBrush, setOffset(downStrip, offset, offset));
            Point[] downLowerStrip =
            {
                new Point(halfWidth,halfHeight), new Point(flagWidth,flagHeight), new Point(flagWidth,flagHeight-yStrip), new Point(halfWidth+xStrip,halfHeight)
            };
            g.FillPolygon(gulesBrush, setOffset(downLowerStrip, offset, offset));
            Point[] upPolygon =
            {
                new Point(flagWidth-(int)x,0), new Point(flagWidth,0), new Point(flagWidth,(int)y), new Point((int)x,flagHeight), new Point(0, flagHeight), new Point(0,flagHeight-(int)y)
            };
            g.FillPolygon(argentBrush, setOffset(upPolygon, offset, offset));
            Point[] upStrip =
            {
                new Point(halfWidth,halfHeight), new Point(flagWidth,0), new Point(flagWidth-xStrip,0), new Point(halfWidth,halfHeight-yStrip)
            };
            g.FillPolygon(gulesBrush, setOffset(upStrip, offset, offset));
            Point[] upLowerStrip =
            {
                new Point(0,flagHeight), new Point(halfWidth,halfHeight), new Point(halfWidth,halfHeight+yStrip), new Point(xStrip,flagHeight)
            };
            g.FillPolygon(gulesBrush, setOffset(upLowerStrip, offset, offset));

            g.FillRectangle(argentBrush, offset + flagWidth * 25f / 60f, offset, flagWidth * 1f / 6f, flagHeight);
            g.FillRectangle(argentBrush, offset, offset + flagHeight * 1f / 3f, flagWidth, flagHeight * 1f / 3f);
            g.FillRectangle(gulesBrush, offset + flagWidth * 27f / 60f, offset, flagWidth * 6f / 60f, flagHeight);
            g.FillRectangle(gulesBrush, offset, offset + flagHeight * 12f / 30f, flagWidth, flagHeight * 6f / 30f);

            g.Dispose();
        }

        /// <summary>
        /// フィンランド国旗を描画
        /// Suomen lippu/Finlands flagga
        /// </summary>
        public void FinlandFlag()
        {
            SolidBrush azureBrush = new SolidBrush(Color.FromArgb(24,68,126));

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 11.0 / 18.0;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.FillRectangle(argentBrush, offset, offset, flagWidth, flagHeight);

            g.FillRectangle(azureBrush, offset, offset + flagHeight * 4f/11f, flagWidth, flagHeight*3f/11f);
            g.FillRectangle(azureBrush, offset + flagWidth * 5f/18f, offset, flagWidth * 3f/18f, flagHeight);

            g.Dispose();
        }

        /// <summary>
        /// スウェーデン国旗を描画
        /// Sveriges flagga
        /// </summary>
        public void SwedenFlag()
        {
            SolidBrush orBrush = new SolidBrush(Color.FromArgb(0xff, 0xd3, 0x0));//FFD300
            SolidBrush azureBrush = new SolidBrush(Color.FromArgb(0x0, 0x87, 0xbd));//0087BD

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 5.0 / 8.0;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.FillRectangle(azureBrush, offset, offset, flagWidth, flagHeight);

            g.FillRectangle(orBrush, offset, offset + flagHeight * 4f / 10f, flagWidth, flagHeight * 1f / 5f);
            g.FillRectangle(orBrush, offset + flagWidth * 5f / 16f, offset, flagWidth * 1f / 8f, flagHeight);

            g.Dispose();
        }


        /// <summary>
        /// ノルウェー国旗を描画
        /// 
        /// </summary>
        public void NorwayFlag()
        {
            SolidBrush gulesBrush = new SolidBrush(Color.FromArgb(0xff, 0xd3, 0x0));//FFD300
            SolidBrush azureBrush = new SolidBrush(Color.FromArgb(0x0, 0x87, 0xbd));//0087BD

            flagWidth = pictureBox1.Width - (offset << 1);

            flagHeight = (int)(flagWidth * 8f / 11f);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight * 1.5);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.FillRectangle(azureBrush, offset, offset, flagWidth, flagHeight);

            g.FillRectangle(orBrush, offset, offset + flagHeight * 4f / 10f, flagWidth, flagHeight * 1f / 5f);
            g.FillRectangle(orBrush, offset + flagWidth * 5f / 16f, offset, flagWidth * 1f / 8f, flagHeight);

            g.Dispose();
        }
        
        /// <summary>
        /// デンマーク国旗を描画
        /// Dannebrog
        /// </summary>
        public void DenmarkFlag()
        {
            SolidBrush gulesBrush = new SolidBrush(Color.FromArgb(0xff, 0xd3, 0x0));//FFD300
            SolidBrush azureBrush = new SolidBrush(Color.FromArgb(0x0, 0x87, 0xbd));//0087BD

            flagWidth = pictureBox1.Width - (offset << 1);

            flagHeight = (int)(flagWidth * 8f / 11f);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight * 1.5);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.FillRectangle(azureBrush, offset, offset, flagWidth, flagHeight);

            g.FillRectangle(orBrush, offset, offset + flagHeight * 4f / 10f, flagWidth, flagHeight * 1f / 5f);
            g.FillRectangle(orBrush, offset + flagWidth * 5f / 16f, offset, flagWidth * 1f / 8f, flagHeight);

            g.Dispose();
        }

        /// <summary>
        /// ポーランド国旗を描画
        /// 
        /// </summary>
        public void PolandFlag()
        {
            SolidBrush gulesBrush = new SolidBrush(Color.FromArgb(0xff, 0xd3, 0x0));//FFD300
   
            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 5.0 / 8.0;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.FillRectangle(argentBrush, offset, offset, flagWidth, flagHeight);

            g.FillRectangle(gulesBrush, offset, offset + flagHeight, flagWidth, flagHeight);

            g.Dispose();
        }


        /// <summary>
        /// ルーマニア旗を描画
        /// drapelul României
        /// </summary>
        public void RomaniaFlag()
        {
            SolidBrush azureBrush = new SolidBrush(Color.FromArgb(0x0, 0x2b, 0x7f));
            SolidBrush orBrush = new SolidBrush(Color.FromArgb(0xfc, 0xd1, 0x16));
            SolidBrush gulesBrush = new SolidBrush(Color.FromArgb(0xce, 0x11, 0x26));

            flagWidth = pictureBox1.Width - (offset << 1);

            flagHeight = (int)(flagWidth / 1.5);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight * 1.5);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            float stripWidth = flagWidth / 3f;
            g.FillRectangle(gulesBrush, offset + stripWidth + stripWidth, offset, stripWidth, flagHeight);
            g.FillRectangle(azureBrush, offset, offset, stripWidth, flagHeight);
            g.FillRectangle(orBrush, offset + stripWidth, offset, stripWidth, flagHeight);

            g.Dispose();
        }

        /// <summary>
        /// ベルギー旗を描画
        /// drapelul României
        /// </summary>
        public void BelgiumFlag()
        {
            SolidBrush orBrush = new SolidBrush(Color.FromArgb(0xFD,0xDA,0x24));//#
            SolidBrush gulesBrush = new SolidBrush(Color.FromArgb(0xEF,0x33,0x40));//#EF3340

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 13.0 / 15.0;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);

            g.SmoothingMode = SmoothingMode.HighQuality;

            float stripWidth = flagWidth / 3f;
            g.FillRectangle(gulesBrush, offset + stripWidth + stripWidth, offset, stripWidth, flagHeight);
            g.FillRectangle(sableBrush, offset, offset, stripWidth, flagHeight);
            g.FillRectangle(orBrush, offset + stripWidth, offset, stripWidth, flagHeight);

            g.Dispose();
        }

        /// <summary>
        /// アイルランド旗を描画
        /// bratach na hÉireann, trídhathach na hÉireann
        /// 
        /// </summary>
        public void IrelandFlag()
        {
            SolidBrush vertBrush = new SolidBrush(Color.FromArgb(0x16, 0x9B, 0x62));//#169B62
            SolidBrush orangeBrush = new SolidBrush(Color.FromArgb(0xFF, 0x88, 0x3E));//#FF883E

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = .5;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);

            g.SmoothingMode = SmoothingMode.HighQuality;

            float stripWidth = flagWidth / 3f;
            g.FillRectangle(vertBrush, offset, offset, stripWidth, flagHeight);
            g.FillRectangle(argentBrush, offset + stripWidth, offset, stripWidth, flagHeight);
            g.FillRectangle(orangeBrush, offset + stripWidth + stripWidth, offset, stripWidth, flagHeight);

            g.Dispose();
        }

        /// <summary>
        /// ブルガリア国旗を描画
        /// </summary>
        public void BulgariaFlag()
        {
            SolidBrush vertBrush = new SolidBrush(FromArgb(0x009875));
            SolidBrush gulesBrush = new SolidBrush(FromArgb(0xD01C1F));

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 3.0 / 5.0;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            float stripHeight = flagHeight / 3.0f;
            g.FillRectangle(argentBrush, offset, offset, flagWidth, stripHeight);
            g.FillRectangle(vertBrush, offset, offset + stripHeight, flagWidth, stripHeight);
            g.FillRectangle(gulesBrush, offset, offset + stripHeight + stripHeight, flagWidth, stripHeight);            

            g.Dispose();
        }

        /// <summary>
        /// オーストリア国旗を描画
        /// </summary>
        public void AustriaFlag()
        {
            SolidBrush gulesBrush = new SolidBrush(FromArgb(0xEF3340));

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 1.0 / 1.5;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            float stripHeight = flagHeight / 3.0f;
            g.FillRectangle(gulesBrush, offset, offset, flagWidth, stripHeight);
            g.FillRectangle(argentBrush, offset, offset + stripHeight, flagWidth, stripHeight);
            g.FillRectangle(gulesBrush, offset, offset + stripHeight + stripHeight, flagWidth, stripHeight);

            g.Dispose();
        }

        /// <summary>
        /// スイス国旗を描画
        /// </summary>
        public void SwitzerlandFlag()
        {
            SolidBrush gulesBrush = new SolidBrush(FromArgb(0xEF3340));

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 1.0;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.FillRectangle(gulesBrush, offset, offset, flagWidth, flagHeight);

            float stripHeight = flagHeight * 20f / 32f;
            float stripX = flagHeight * 13f / 32f;
            float stripY = flagHeight * 6f / 32f;
            g.FillRectangle(argentBrush, offset + stripX, offset + stripY, stripY, stripHeight);
            g.FillRectangle(argentBrush, offset + stripY, offset + stripX, stripHeight, stripY);

            g.Dispose();
        }

        /// <summary>
        /// ルクセンブルグ国旗を描画
        /// </summary>
        public void LuxembergFlag()
        {
            SolidBrush gulesBrush = new SolidBrush(FromArgb(0xF6343F));
            SolidBrush azureBrush = new SolidBrush(FromArgb(0x00A2E1));

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = .5;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.FillRectangle(gulesBrush, offset, offset, flagWidth, flagHeight);

            float stripHeight = flagHeight / 3.0f;
            g.FillRectangle(gulesBrush, offset, offset, flagWidth, stripHeight);
            g.FillRectangle(argentBrush, offset, offset + stripHeight, flagWidth, stripHeight);
            g.FillRectangle(azureBrush, offset, offset + stripHeight + stripHeight, flagWidth, stripHeight);

            g.Dispose();
        }

        /// <summary>
        /// チェコ国旗を描画
        /// </summary>
        public void CzechFlag()
        {
            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 1.0/1.5;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.FillRectangle(gulesBrush, offset, offset, flagWidth, flagHeight);

            int halfWidth = flagWidth >> 1;
            int halfHeight = flagHeight >> 1;
            g.FillRectangle(argentBrush, offset, offset, flagWidth, halfHeight);
            g.FillRectangle(gulesBrush, offset, offset + halfHeight, flagWidth, halfHeight);

            Point[] azure =
{
                new Point(0,0), new Point(halfWidth,halfHeight), new Point(0,flagHeight)
            };
            g.FillPolygon(azureBrush, setOffset(azure, offset, offset));

            g.Dispose();
        }

        /// <summary>
        /// ロシア国旗を描画
        /// </summary>
        public void RussiaFlag()
        {
            SolidBrush azureBrush = new SolidBrush(FromArgb(0x0039A6));
            SolidBrush gulesBrush = new SolidBrush(FromArgb(0xD00B0E));

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 1.0 / 1.5;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            float stripHeight = flagHeight / 3.0f;
            g.FillRectangle(argentBrush, offset, offset, flagWidth, stripHeight);
            g.FillRectangle(azureBrush, offset, offset + stripHeight, flagWidth, stripHeight);
            g.FillRectangle(gulesBrush, offset, offset + stripHeight + stripHeight, flagWidth, stripHeight);

            g.Dispose();
        }

        /// <summary>
        /// リトアニア国旗を描画
        /// Lietuvos vėliava
        /// 
        /// </summary>
        public void LithuaniaFlag()
        {
            SolidBrush orBrush = new SolidBrush(FromArgb(0xFDB913)); 
            SolidBrush vertBrush = new SolidBrush(FromArgb(0x06a44));
            SolidBrush gulesBrush = new SolidBrush(FromArgb(0xC1272D));

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = 3.0 / 5.0;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            float stripHeight = flagHeight / 3.0f;
            g.FillRectangle(orBrush, offset, offset, flagWidth, stripHeight);
            g.FillRectangle(vertBrush, offset, offset + stripHeight, flagWidth, stripHeight);
            g.FillRectangle(gulesBrush, offset, offset + stripHeight + stripHeight, flagWidth, stripHeight);

            g.Dispose();
        }

        /// <summary>
        /// ラトビア国旗を描画
        /// Lietuvos vėliava
        /// 
        /// </summary>
        public void LatviaFlag()
        {
            SolidBrush gulesBrush = new SolidBrush(FromArgb(0x9E1B34));

            flagWidth = pictureBox1.Width - (offset << 1);

            double ratio = .5;
            flagHeight = (int)(flagWidth * ratio);
            if (pictureBox1.Height - (offset << 1) < flagHeight)
            {
                flagHeight = pictureBox1.Height - (offset << 1);
                flagWidth = (int)(flagHeight / ratio);
            }

            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
            {
                return;
            }

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.FillRectangle(gulesBrush, offset, offset, flagWidth, flagHeight);
            float stripHeight = flagHeight / 5.0f;
            g.FillRectangle(argentBrush, offset, offset + stripHeight + stripHeight, flagWidth, stripHeight);

            g.Dispose();
        }

        private Color FromArgb(int argb)
        {
            return Color.FromArgb((argb >> 16), (argb >> 8) & 0xff, argb & 0xff);
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
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Bitmap bitmap = new Bitmap(flagWidth, flagHeight);
                Graphics g = Graphics.FromImage(bitmap);
                Rectangle destRect = new Rectangle(0, 0, flagWidth, flagHeight);
                Rectangle srcRect = new Rectangle(offset, offset, flagWidth, flagHeight);
                g.DrawImage(pictureBox1.Image, destRect, srcRect, GraphicsUnit.Pixel);
                string fileName = dlg.FileName;
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

        private void japanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Japan;
            PaintFlag();
        }

        private void cataloniaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Catalonia;
            PaintFlag();
        }

        private void eUToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.EU;
            PaintFlag();
        }

        public static void SaveAnimatedGif(string fileName, Bitmap[] baseImages, UInt16 delayTime, UInt16 loopCount)
        {
            FileStream writerFs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            BinaryWriter writer = new BinaryWriter(writerFs);

            MemoryStream ms = new MemoryStream();
            bool hasGlobalColorTable = false;
            int colorTableSize = 0;

            int imagesCount = baseImages.Length;
            for (int i = 0; i < imagesCount; i++)
            {
                Bitmap bmp = baseImages[i];
                bmp.Save(ms, ImageFormat.Gif);
                ms.Position = 0;

                if (i == 0)
                {
                    //ヘッダを書き込む
                    //Header
                    writer.Write(ReadBytes(ms, 6));

                    //Logical Screen Descriptor
                    byte[] screenDescriptor = ReadBytes(ms, 7);
                    //Global Color Tableがあるか確認
                    if ((screenDescriptor[4] & 0x80) != 0)
                    {
                        //Color Tableのサイズを取得
                        colorTableSize = screenDescriptor[4] & 0x07;
                        hasGlobalColorTable = true;
                    }
                    else
                    {
                        hasGlobalColorTable = false;
                    }
                    //Global Color Tableを使わない
                    //広域配色表フラグと広域配色表の寸法を消す
                    screenDescriptor[4] = (byte)(screenDescriptor[4] & 0x78);
                    writer.Write(screenDescriptor);

                    //Application Extension
                    writer.Write(GetApplicationExtension(loopCount));
                }
                else
                {
                    //HeaderとLogical Screen Descriptorをスキップ
                    ms.Position += 6 + 7;
                }

                byte[] colorTable = null;
                if (hasGlobalColorTable)
                {
                    //Color Tableを取得
                    colorTable = ReadBytes(ms, (int)Math.Pow(2, colorTableSize + 1) * 3);
                }

                //Graphics Control Extension
                writer.Write(GetGraphicControlExtension(delayTime));
                //基のGraphics Control Extensionをスキップ
                if (ms.GetBuffer()[ms.Position] == 0x21)
                {
                    ms.Position += 8;
                }

                //Image Descriptor
                byte[] imageDescriptor = ReadBytes(ms, 10);
                if (!hasGlobalColorTable)
                {
                    //Local Color Tableを持っているか確認
                    if ((imageDescriptor[9] & 0x80) == 0)
                        throw new Exception("Not found color table.");
                    //Color Tableのサイズを取得
                    colorTableSize = imageDescriptor[9] & 7;
                    //Color Tableを取得
                    colorTable = ReadBytes(ms, (int)Math.Pow(2, colorTableSize + 1) * 3);
                }
                //狭域配色表フラグ (Local Color Table Flag) と狭域配色表の寸法を追加
                imageDescriptor[9] = (byte)(imageDescriptor[9] | 0x80 | colorTableSize);
                writer.Write(imageDescriptor);

                //Local Color Tableを書き込む
                writer.Write(colorTable);

                //Image Dataを書き込む (終了部は書き込まない)
                writer.Write(ReadBytes(ms, (int)(ms.Length - ms.Position - 1)));

                if (i == imagesCount - 1)
                {
                    //終了部 (Trailer)
                    writer.Write((byte)0x3B);
                }

                //MemoryStreamをリセット
                ms.SetLength(0);
            }

            ms.Close();
            writer.Close();
            writerFs.Close();
        }

        private static byte[] ReadBytes(MemoryStream ms, int count)
        {
            byte[] bs = new byte[count];
            ms.Read(bs, 0, count);
            return bs;
        }

        private static byte[] GetApplicationExtension(UInt16 loopCount)
        {
            byte[] bs = new byte[19];

            //拡張導入符 (Extension Introducer)
            bs[0] = 0x21;
            //アプリケーション拡張ラベル (Application Extension Label)
            bs[1] = 0xFF;
            //ブロック寸法 (Block Size)
            bs[2] = 0x0B;
            //アプリケーション識別名 (Application Identifier)
            bs[3] = (byte)'N';
            bs[4] = (byte)'E';
            bs[5] = (byte)'T';
            bs[6] = (byte)'S';
            bs[7] = (byte)'C';
            bs[8] = (byte)'A';
            bs[9] = (byte)'P';
            bs[10] = (byte)'E';
            //アプリケーション確証符号 (Application Authentication Code)
            bs[11] = (byte)'2';
            bs[12] = (byte)'.';
            bs[13] = (byte)'0';
            //データ副ブロック寸法 (Data Sub-block Size)
            bs[14] = 0x03;
            //詰め込み欄 [ネットスケープ拡張コード (Netscape Extension Code)]
            bs[15] = 0x01;
            //繰り返し回数 (Loop Count)
            byte[] loopCountBytes = BitConverter.GetBytes(loopCount);
            bs[16] = loopCountBytes[0];
            bs[17] = loopCountBytes[1];
            //ブロック終了符 (Block Terminator)
            bs[18] = 0x00;

            return bs;
        }

        private static byte[] GetGraphicControlExtension(UInt16 delayTime)
        {
            byte[] bs = new byte[8];

            //拡張導入符 (Extension Introducer)
            bs[0] = 0x21;
            //グラフィック制御ラベル (Graphic Control Label)
            bs[1] = 0xF9;
            //ブロック寸法 (Block Size, Byte Size)
            bs[2] = 0x04;
            //詰め込み欄 (Packed Field)
            //透過色指標を使う時は+1
            //消去方法:そのまま残す+4、背景色でつぶす+8、直前の画像に戻す+12
            bs[3] = 0x00;
            //遅延時間 (Delay Time)
            byte[] delayTimeBytes = BitConverter.GetBytes(delayTime);
            bs[4] = delayTimeBytes[0];
            bs[5] = delayTimeBytes[1];
            //透過色指標 (Transparency Index, Transparent Color Index)
            bs[6] = 0x00;
            //ブロック終了符 (Block Terminator)
            bs[7] = 0x00;

            return bs;
        }

        private void saveAsGIFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rectangle destRect = new Rectangle(0, 0, flagWidth, flagHeight);
            Rectangle srcRect = new Rectangle(offset, offset, flagWidth, flagHeight);

            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Bitmap[] frameList = new Bitmap[imageList.Length];
                for (int i = 0; i < imageList.Length; i++)
                {
                    state = (State)i;
                    frameList[i] = new Bitmap(flagWidth, flagHeight);
                    Graphics g = Graphics.FromImage(frameList[i]);

                    g.DrawImage(imageList[i], destRect, srcRect, GraphicsUnit.Pixel);
                    g.Dispose();
                }

                string fileName = dlg.FileName;
                int index = fileName.IndexOf(".");
                if (index < 0)
                {
                    fileName += ".gif";
                }
                SaveAnimatedGif(fileName, frameList, 10, 0);

                //AVIWriter AVIWriter = new AVIWriter();

                //AVIWriter.FrameRate = 60;

                //AVIWriter.Codec = "DIB ";

                //AVIWriter.Open(stringAVIFilename, bitmapAVI.Width, bitmapAVI.Height);

                //int countFrame = 0;

                //static int NumOfFrames = 1000;

                //while (countFrame < NumOfFrames)

                //{

                //    AVIWriter.AddFrame(bitmapAVI);

                //    countFrame++;

                //}

                //AVIWriter.Close();

                //var writer = new AForge.Video.VFW.AVIWriter();
                //writer.Codec = "MSVC";
                //writer.FrameRate = 15;
                //writer.Open("out.avi", 800, 600); // 800x600

                //writer.AddFrame(new System.Drawing.Bitmap(800, 600));

                //writer.Close();

                //string series = fileName.Substring(0, fileName.Length - 4);
                //for (int i = 0; i < frameList.Length; i++)
                //{
                //    frameList[i].Save(series + i + ".png");
                //}
            }
        }

        private void uSAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.US;
            PaintFlag();
        }

        private void franceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.France;
            PaintFlag();
        }

        private void germanyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Germany;
            PaintFlag();
        }

        private void italyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Italy;
            PaintFlag();
        }

        private void uKToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.UK;
            PaintFlag();
        }

        private void finlandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Finland;
            PaintFlag();
        }

        private void swedenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Sweden;
            PaintFlag();
        }

        private void polandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Poland;
            PaintFlag();
        }

        private void romaniaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Romania;
            PaintFlag();
        }

        private void belgiumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Belgium;
            PaintFlag();
        }

        private void irelandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Ireland;
            PaintFlag();
        }

        private void bulgariaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Bulgaria;
            PaintFlag();
        }

        private void austriaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Austria;
            PaintFlag();
        }

        private void switzerlandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Switzerland;
            PaintFlag();
        }

        private void luxembergToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Luxemberg;
            PaintFlag();
        }

        private void czechToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Czech;
            PaintFlag();
        }

        private void russiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Russia;
            PaintFlag();
        }

        private void lithuaniaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Lithuania;
            PaintFlag();
        }

        private void latviaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = State.Latvia;
            PaintFlag();
        }
    }
}
