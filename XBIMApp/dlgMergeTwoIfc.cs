using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XBIMApp
{
    public partial class dlgMergeTwoIfc : Form
    {
        public String ifcFileName1 = string.Empty;
        public String ifcFileName2 = string.Empty;
        public String ifcFileName3 = string.Empty;
        public String ifcFileName4 = string.Empty;
        public dlgMergeTwoIfc()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "IFC模型(*.ifc)|*.ifc";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ifcFileName1 = dlg.FileName;
                this.textBox1.Text = ifcFileName1;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "IFC模型(*.ifc)|*.ifc";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ifcFileName2 = dlg.FileName;
                this.textBox2.Text = ifcFileName2;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "IFC模型(*.ifc)|*.ifc";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ifcFileName3 = dlg.FileName;
                this.textBox3.Text = ifcFileName2;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "IFC模型(*.ifc)|*.ifc";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ifcFileName4 = dlg.FileName;
                this.textBox4.Text = ifcFileName2;
            }
        }
    }
}
