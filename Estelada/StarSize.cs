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
        public StarSize(Main form)
        {
            this.form = form;
            InitializeComponent();
            trackBar1.Minimum = 0;
            trackBar1.Maximum = 10;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            int size = (int)(trackBar1.Value/10.0f*3);
            form.setStarSize(size);
            form.PaintFlag();
            Dispose();
        }
    }
}
