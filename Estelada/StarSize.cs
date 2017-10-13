using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Estelada
{
    public partial class StarSize : Form
    {
        Main form = null;

        static float scaleMin = .3f;
        static float scaleMax = 1.4f;        
        static int nTicks = 30;
        float tickScale = (float)((scaleMax - scaleMin) / nTicks);

        public StarSize(Main form)
        {
            this.form = form;
            InitializeComponent();
            trackBar1.Value = FromScale(form.starSize);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        /// <summary>
        /// Range: scaleMin - scaleMax
        /// #Ticks: nTick
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private float FromTick(int pos)
        {
            float res;
            res=(float)(pos * tickScale + scaleMin);
            return res;
        }

        private int FromScale(float scale)
        {
            int res;            
            res =(int)( (scale - scaleMin) / tickScale);
            return res;
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            form.starSize = FromTick(trackBar1.Value);           
            form.PaintFlag();
            Dispose();
        }
    }
}
